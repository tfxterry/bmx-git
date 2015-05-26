using System;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.IssueTrackerConnections;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.GitHub
{
    [Serializable]
    [CustomEditor(typeof(GitHubApplicationFilterEditor))]
    public sealed class GitHubApplicationFilter : IssueTrackerApplicationConfiguration
    {
        [Persistent]
        public string Owner { get; set; }

        [Persistent]
        public string Repository { get; set; }
    }
}
