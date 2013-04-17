using System;
using System.Web;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Extensibility.Providers.IssueTracking;
using Inedo.BuildMaster.Web;
using Inedo.Linq;

namespace Inedo.BuildMasterExtensions.GitHub
{
    /// <summary>
    /// Issue tracking provider for GitHub.
    /// </summary>
    [ProviderProperties(
        "GitHub",
        "Provides issue tracking integration for GitHub.")]
    [CustomEditor(typeof(GitHubIssueTrackingProviderEditor))]
    public sealed class GitHubIssueTrackingProvider : IssueTrackingProviderBase, ICategoryFilterable, IUpdatingProvider, IReleaseNumberCreator, IReleaseNumberCloser
    {
        private Lazy<GitHub> github;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubIssueTrackingProvider"/> class.
        /// </summary>
        public GitHubIssueTrackingProvider()
        {
            this.github = new Lazy<GitHub>(() => new GitHub { OrganizationName = this.OrganizationName, UserName = this.UserName, Password = this.Password });
        }

        /// <summary>
        /// Gets or sets the organization name which owns the repositories.
        /// </summary>
        [Persistent]
        public string OrganizationName { get; set; }
        /// <summary>
        /// Gets or sets the user name to use to authenticate with GitHub.
        /// </summary>
        [Persistent]
        public string UserName { get; set; }
        /// <summary>
        /// Gets or sets the password of the user name to use to authenticate with GitHub.
        /// </summary>
        [Persistent]
        public string Password { get; set; }

        public string[] CategoryIdFilter { get; set; }
        public string[] CategoryTypeNames
        {
            get { return new[] { "Repository" }; }
        }
        public bool CanAppendIssueDescriptions
        {
            get { return true; }
        }
        public bool CanChangeIssueStatuses
        {
            get { return true; }
        }
        public bool CanCloseIssues
        {
            get { return true; }
        }

        private FilterContext CategoryFilter
        {
            get
            {
                if (this.CategoryIdFilter == null || this.CategoryIdFilter.Length == 0 || string.IsNullOrEmpty(this.CategoryIdFilter[0]))
                    return null;

                var categoryParts = this.CategoryIdFilter[0].Split(new[] { '/' }, 2, StringSplitOptions.None);
                return new FilterContext(categoryParts[0], categoryParts[1]);
            }
        }
        private GitHub GitHub
        {
            get { return this.github.Value; }
        }

        public override string ToString()
        {
            return "Provides issue tracking integration for GitHub.";
        }
        public override Issue[] GetIssues(string releaseNumber)
        {
            var filter = this.CategoryFilter;
            if (filter == null)
                return new Issue[0];

            return this.GitHub
                .EnumIssues(releaseNumber, filter.Owner, filter.Repository)
                .Select(i => new GitHubIssue(i["number"].ToString(), (string)i["state"], (string)i["title"], (string)i["body"], releaseNumber, (string)i["html_url"]))
                .ToArray();
        }
        public override bool IsIssueClosed(Issue issue)
        {
            if (issue == null)
                throw new ArgumentNullException("issue");

            return string.Equals(issue.IssueStatus, "closed", StringComparison.OrdinalIgnoreCase);
        }
        public override bool IsAvailable()
        {
            // Always available - no special client installation required
            return true;
        }
        public override void ValidateConnection()
        {
            try
            {
                this.GetCategories();
            }
            catch (Exception ex)
            {
                throw new NotAvailableException(ex.Message, ex);
            }
        }
        public CategoryBase[] GetCategories()
        {
            return this.GitHub
                .EnumRepositories()
                .Select(r => new GitHubCategory(((JavaScriptObject)r["owner"])["login"] + "/" + r["name"], (string)r["name"]))
                .ToArray();
        }
        public void AppendIssueDescription(string issueId, string textToAppend)
        {
            if (string.IsNullOrEmpty(issueId))
                throw new ArgumentNullException("issueId");
            if (string.IsNullOrEmpty(textToAppend))
                return;

            var filter = this.CategoryFilter;

            var issue = this.GitHub.GetIssue(issueId, filter.Owner, filter.Repository);
            var desc = (string)issue["body"] ?? string.Empty;

            if (!string.IsNullOrEmpty(desc))
                desc += "\n" + textToAppend;
            else
                desc = textToAppend;

            this.GitHub.UpdateIssue(issueId, filter.Owner, filter.Repository, new { body = desc });
        }
        public void ChangeIssueStatus(string issueId, string newStatus)
        {
            if (string.IsNullOrEmpty(issueId))
                throw new ArgumentNullException("issueId");
            if (string.IsNullOrEmpty(newStatus))
                throw new ArgumentNullException("newStatus");
            if (!string.Equals(newStatus, "open", StringComparison.OrdinalIgnoreCase) && !string.Equals(newStatus, "closed", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Status must be either open or closed.");

            var filter = this.CategoryFilter;

            this.GitHub.UpdateIssue(
                issueId,
                filter.Owner,
                filter.Repository,
                new { state = newStatus.ToLowerInvariant() }
            );
        }
        public void CloseIssue(string issueId)
        {
            this.ChangeIssueStatus(issueId, "closed");
        }
        public void CreateReleaseNumber(string releaseNumber)
        {
            var filter = this.CategoryFilter;
            this.GitHub.CreateMilestone(releaseNumber, filter.Owner, filter.Repository);
        }
        public void CloseReleaseNumber(string releaseNumber)
        {
            var filter = this.CategoryFilter;
            this.GitHub.CloseMilestone(releaseNumber, filter.Owner, filter.Repository);
        }
        public override string GetIssueUrl(Issue issue)
        {
            if (issue == null)
                throw new ArgumentNullException("issue");

            return ((GitHubIssue)issue).HtmlUrl;
        }

        private sealed class FilterContext
        {
            public FilterContext(string owner, string repository)
            {
                this.Owner = HttpUtility.UrlEncode(owner);
                this.Repository = HttpUtility.UrlEncode(repository);
            }

            public string Owner { get; private set; }
            public string Repository { get; private set; }
        }
    }
}
