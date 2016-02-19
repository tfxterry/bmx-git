using System;
using Inedo.BuildMaster.Documentation;
using Inedo.BuildMaster.Extensibility.IssueTrackerConnections;
using Inedo.BuildMaster.Web;
using Inedo.Serialization;

namespace Inedo.BuildMasterExtensions.GitHub
{
    [Serializable]
    [CustomEditor(typeof(GitHubApplicationFilterEditor))]
    public sealed class GitHubApplicationFilter : IssueTrackerApplicationConfigurationBase
    {
        [Persistent]
        public string Owner { get; set; }

        [Persistent]
        public string Repository { get; set; }

        public override RichDescription GetDescription()
        {
            return new RichDescription(
                "Repository: ", new Hilite(this.Repository), ", Owner: ", new Hilite(this.Owner)
            );
        }
    }
}
