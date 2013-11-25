using System;
using Inedo.BuildMaster.Extensibility.Providers.IssueTracking;

namespace Inedo.BuildMasterExtensions.GitHub
{
    /// <summary>
    /// Represents a GitHub issue.
    /// </summary>
    [Serializable]
    internal sealed class GitHubIssue : IssueTrackerIssue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubIssue"/> class.
        /// </summary>
        /// <param name="issueId">The issue ID.</param>
        /// <param name="issueStatus">The issue status.</param>
        /// <param name="issueTitle">The issue title.</param>
        /// <param name="issueDescription">The issue description.</param>
        /// <param name="releaseNumber">The release number of the issue.</param>
        /// <param name="htmlUrl">The URL of the HTML issue description.</param>
        public GitHubIssue(string issueId, string issueStatus, string issueTitle, string issueDescription, string releaseNumber, string htmlUrl)
            : base(issueId, issueStatus, issueTitle, issueDescription, releaseNumber)
        {
            this.HtmlUrl = htmlUrl;
        }

        /// <summary>
        /// Gets or sets the URL of the HTML issue description.
        /// </summary>
        public string HtmlUrl { get; private set; }
    }
}
