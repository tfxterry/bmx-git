using System;
using System.Collections.Generic;
using System.Linq;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.IssueTrackerConnections;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.GitHub
{
    /// <summary>
    /// Issue tracking provider for GitHub.
    /// </summary>
    [ProviderProperties(
        "GitHub",
        "Provides issue tracking integration for GitHub.")]
    [CustomEditor(typeof(GitHubIssueTrackingProviderEditor))]
    public sealed partial class GitHubIssueTrackingProvider : IssueTrackerConnectionBase, IReleaseManager, IIssueCloser, IIssueCommenter
    {
        private Lazy<GitHub> github;

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

        private GitHub GitHub
        {
            get { return this.github.Value; }
        }

        public override ExtensionComponentDescription GetDescription()
        {
            if (!string.IsNullOrWhiteSpace(this.OrganizationName))
            {
                return new ExtensionComponentDescription(
                    "GitHub (",
                    new Hilite(this.OrganizationName),
                    ")"
                );
            }
            else
            {
                return new ExtensionComponentDescription("GitHub");
            }
        }
        public override IEnumerable<IIssueTrackerIssue> EnumerateIssues(IssueTrackerConnectionContext context)
        {
            var filter = this.GetFilter(context);

            return this.GitHub
                .EnumIssues(context.ReleaseNumber, filter.Owner, filter.Repository)
                .Select(i => new GitHubIssue(i));
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
                //this.GetCategories();
            }
            catch (Exception ex)
            {
                throw new NotAvailableException(ex.Message, ex);
            }
        }

        void IReleaseManager.CreateRelease(IssueTrackerConnectionContext context)
        {
            var filter = this.GetFilter(context);
            this.GitHub.CreateMilestone(context.ReleaseNumber, filter.Owner, filter.Repository);
        }
        void IReleaseManager.DeployRelease(IssueTrackerConnectionContext context)
        {
            var filter = this.GetFilter(context);
            this.GitHub.CloseMilestone(context.ReleaseNumber, filter.Owner, filter.Repository);
        }

        void IIssueCloser.CloseAllIssues(IssueTrackerConnectionContext context)
        {
            var issues = this.EnumerateIssues(context).ToList();
            var filter = this.GetFilter(context);
            foreach (var issue in issues)
            {
                this.GitHub.UpdateIssue(
                    issue.Id,
                    filter.Owner,
                    filter.Repository,
                    new { state = "closed" }
                );
            }
        }
        void IIssueCloser.CloseIssue(IssueTrackerConnectionContext context, string issueId)
        {
            var filter = this.GetFilter(context);

            this.GitHub.UpdateIssue(
                issueId,
                filter.Owner,
                filter.Repository,
                new { state = "closed" }
            );
        }

        void IIssueCommenter.AddComment(IssueTrackerConnectionContext context, string issueId, string commentText)
        {
            var filter = this.GetFilter(context);
            this.GitHub.CreateComment(issueId, filter.Owner, filter.Repository, commentText);
        }

        private GitHubApplicationFilter GetFilter(IssueTrackerConnectionContext context)
        {
            return (GitHubApplicationFilter)context.ApplicationConfiguration ?? this.legacyFilter;
        }
    }
}
