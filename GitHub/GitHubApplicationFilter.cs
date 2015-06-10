using System;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.IssueTrackerConnections;
using Inedo.BuildMaster.Web;

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

        public override ExtensionComponentDescription GetDescription()
        {
            return new ExtensionComponentDescription(
                "Repository: ", new Hilite(this.Repository), ", Owner: ", new Hilite(this.Owner)
            );
        }
    }
}
