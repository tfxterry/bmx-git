using System;
using System.Text;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Extensibility.Providers.SourceControl;
using Inedo.BuildMaster.Files;
using Inedo.BuildMaster.Web;
using Inedo.BuildMasterExtensions.Git;
using Inedo.Linq;

namespace Inedo.BuildMasterExtensions.GitHub
{
    /// <summary>
    /// A provider that integrates with the Git source control system that is optimized for GitHub.
    /// </summary>
    [ProviderProperties("GitHub", "Git integration optimized for use with GitHub.com; requires Git to be installed on the server for use with an SSH Agent.")]
    [CustomEditor(typeof(GitHubSourceControlProviderEditor))]
    [RequiresInterface(typeof(IRemoteProcessExecuter))]
    [RequiresInterface(typeof(IFileOperationsExecuter))]
    public sealed class GitHubSourceControlProvider : SourceControlProviderBase, IVersioningProvider, IRevisionProvider, IGitSourceControlProvider
    {
        private GitSourceControlProviderCommon wrappedProvider;
        private Lazy<GitHubRepository[]> repositories;
        private Lazy<GitHub> github;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubSourceControlProvider"/> class.
        /// </summary>
        public GitHubSourceControlProvider()
        {
            this.wrappedProvider = new GitSourceControlProviderCommon(this, this.GitExecutablePath);
            this.github = new Lazy<GitHub>(() => new GitHub { OrganizationName = this.OrganizationName, UserName = this.UserName, Password = this.Password });
            this.repositories = new Lazy<GitHubRepository[]>(
                () => this.GitHub
                    .EnumRepositories()
                    .Select(r => new GitHubRepository(r, this.UserName, this.Password))
                    .ToArray()
            );
        }

        /// <summary>
        /// Gets or sets the Git executable path.
        /// </summary>
        /// <remarks>
        /// If not specified, the built-in client will be used.
        /// </remarks>
        [Persistent]
        public string GitExecutablePath { get; set; }
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

        public override char DirectorySeparator
        {
            get { return '/'; }
        }

        private GitHub GitHub
        {
            get { return this.github.Value; }
        }

        public override DirectoryEntryInfo GetDirectoryEntryInfo(string sourcePath)
        {
            return this.wrappedProvider.GetDirectoryEntryInfo(sourcePath);
        }

        public override byte[] GetFileContents(string filePath)
        {
            return this.wrappedProvider.GetFileContents(filePath);
        }

        public override void GetLatest(string sourcePath, string targetPath)
        {
            this.wrappedProvider.GetLatest(sourcePath, targetPath);
        }

        public override bool IsAvailable()
        {
            return this.wrappedProvider.IsAvailable();
        }

        public override string ToString()
        {
            var s = new StringBuilder("GitHub");
            
            if (!string.IsNullOrEmpty(this.OrganizationName))
                s.AppendFormat(" ({0} organization)", this.OrganizationName);
            
            if (!string.IsNullOrEmpty(this.UserName))
                s.AppendFormat(" using {0}", this.UserName);

            return s.ToString();
        }

        public override void ValidateConnection()
        {
            this.wrappedProvider.ValidateConnection();
        }

        public void ApplyLabel(string label, string sourcePath)
        {
            this.wrappedProvider.ApplyLabel(label, sourcePath);
        }

        public void GetLabeled(string label, string sourcePath, string targetPath)
        {
            this.wrappedProvider.GetLabeled(label, sourcePath, targetPath);
        }

        public byte[] GetCurrentRevision(string path)
        {
            return this.wrappedProvider.GetCurrentRevision(path);
        }

        IGitRepository[] IGitSourceControlProvider.Repositories
        {
            get { return this.repositories.Value; }
        }

        IFileOperationsExecuter IGitSourceControlProvider.Agent
        {
            get { return (IFileOperationsExecuter)this.Agent; }
        }

        Git.Clients.ProcessResults IGitSourceControlProvider.ExecuteCommandLine(string fileName, string arguments, string workingDirectory)
        {
            // This is not an ideal way to do this, but the idea here is to show an error if this is not used on a Windows client
            // Currently, only the Windows agent is an IRemoteCommandExecuter, so this will work
            if (!(this.Agent is IRemoteCommandExecuter))
                throw new NotAvailableException("The integrated Git client cannot be used on Linux. Enable the Use Standard Git Client option of the GitHub Provider.");

            var results = this.ExecuteCommandLine(fileName, arguments, workingDirectory);
            return new Git.Clients.ProcessResults(results.ExitCode, results.Output, results.Error);
        }

        private sealed class GitHubRepository : IGitRepository
        {
            public GitHubRepository(JavaScriptObject repo, string userName, string password)
            {
                this.RepositoryName = (string)repo["name"];
                var url = new UriBuilder((string)repo["clone_url"]);
                url.UserName = userName;
                url.Password = password;
                this.RemoteRepositoryUrl = url.ToString();
                this.RepositoryPath = GitPath.BuildPathFromUrl(url.ToString());
            }

            public string RepositoryName { get; private set; }
            public string RepositoryPath { get; private set; }
            public string RemoteRepositoryUrl { get; private set; }
        }
    }
}
