using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Providers.SourceControl;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Git
{
    [CustomEditor(typeof(GitRepositoryEditor))]
    public sealed class GitRepository : RepositoryBase, IGitRepository
    {
        /// <summary>
        /// Gets or sets the url of the remote repository.
        /// </summary>
        [Persistent]
        public string RemoteRepositoryUrl { get; set; }

        public override string RepositoryName
        {
            get { return this.RemoteRepositoryUrl; }
        }
    }
}
