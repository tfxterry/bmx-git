using System;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.IssueTrackerConnections;

namespace Inedo.BuildMasterExtensions.GitHub
{
    [Serializable]
    public sealed class GitHubApplicationFilter : IssueTrackerApplicationConfiguration
    {
        [Persistent]
        public string Owner { get; set; }

        [Persistent]
        public string Repository { get; set; }
    }
}
