using System;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Extensibility.Providers.SourceControl;
using Inedo.BuildMaster.Files;
using Inedo.BuildMasterExtensions.Git.Clients;
using Inedo.Linq;

namespace Inedo.BuildMasterExtensions.Git
{
    /// <summary>
    /// Used to share an implementation betweeen the Git provider and the GitHub provider
    /// since they have to inherit different base classes.
    /// </summary>
    internal sealed class GitSourceControlProviderCommon : IVersioningProvider, IRevisionProvider, IGitSourceControlProvider
    {
        private IGitSourceControlProvider owner;
        private Lazy<GitClientBase> gitClient;

        public GitSourceControlProviderCommon(IGitSourceControlProvider ownerProvider, string gitExePath)
        {
            this.owner = ownerProvider;
            this.gitClient = new Lazy<GitClientBase>(
                () =>
                {
                    if (string.IsNullOrEmpty(gitExePath))
                        return new LilGitClient(this);
                    else
                        return new StandardGitClient(this, gitExePath);
                }
            );
        }

        private GitClientBase GitClient
        {
            get { return this.gitClient.Value; }
        }

        public void GetLatest(string sourcePath, string targetPath)
        {
            if (targetPath == null)
                throw new ArgumentNullException("targetPath");

            var gitSourcePath = new GitPath(this, sourcePath);
            if (gitSourcePath.Repository == null)
                throw new ArgumentException(sourcePath + " does not represent a valid Git path.", "sourcePath");

            this.EnsureRepoIsPresent(gitSourcePath.Repository);
            this.GitClient.UpdateLocalRepo(gitSourcePath.Repository, gitSourcePath.Branch, null);
            this.CopyNonGitFiles(gitSourcePath.PathOnDisk, targetPath);
        }

        public DirectoryEntryInfo GetDirectoryEntryInfo(string sourcePath)
        {
            return this.GetDirectoryEntryInfo(new GitPath(this, sourcePath));
        }

        public void ApplyLabel(string label, string sourcePath)
        {
            if (string.IsNullOrEmpty(label))
                throw new ArgumentNullException("label");

            var gitSourcePath = new GitPath(this, sourcePath);
            if (gitSourcePath.Repository == null)
                throw new ArgumentException(sourcePath + " does not represent a valid Git path.", "sourcePath");

            this.EnsureRepoIsPresent(gitSourcePath.Repository);
            this.GitClient.UpdateLocalRepo(gitSourcePath.Repository, gitSourcePath.Branch, null);
            this.GitClient.ApplyTag(gitSourcePath.Repository, label);
        }

        public void GetLabeled(string label, string sourcePath, string targetPath)
        {
            if (string.IsNullOrEmpty(label))
                throw new ArgumentNullException("label");
            if (string.IsNullOrEmpty(targetPath))
                throw new ArgumentNullException("targetPath");

            var gitSourcePath = new GitPath(this, sourcePath);
            if (gitSourcePath.Repository == null)
                throw new ArgumentException(sourcePath + " does not represent a valid Git path.", "sourcePath");

            this.EnsureRepoIsPresent(gitSourcePath.Repository);
            this.GitClient.UpdateLocalRepo(gitSourcePath.Repository, gitSourcePath.Branch, label);
            this.CopyNonGitFiles(gitSourcePath.PathOnDisk, targetPath);
        }

        public byte[] GetCurrentRevision(string path)
        {
            var gitSourcePath = new GitPath(this, path);
            if (gitSourcePath.Repository == null)
                throw new ArgumentException(path + " does not represent a valid Git path.", "sourcePath");

            this.EnsureRepoIsPresent(gitSourcePath.Repository);
            this.GitClient.UpdateLocalRepo(gitSourcePath.Repository, gitSourcePath.Branch, null);
            return this.GitClient.GetLastCommit(gitSourcePath.Repository, gitSourcePath.Branch);
        }

        public void ValidateConnection()
        {
            this.GitClient.ValidateConnection();
        }

        public bool IsAvailable()
        {
            return true;
        }

        public byte[] GetFileContents(string filePath)
        {
            var gitSourcePath = new GitPath(this, filePath);
            if (gitSourcePath.Repository == null)
                throw new ArgumentException(filePath + " does not represent a valid Git path.", "filePath");

            this.EnsureRepoIsPresent(gitSourcePath.Repository);
            this.GitClient.UpdateLocalRepo(gitSourcePath.Repository, gitSourcePath.Branch, null);

            return ((IFileOperationsExecuter)this.Agent).ReadAllFileBytes(gitSourcePath.PathOnDisk);
        }

        public IGitRepository[] Repositories
        {
            get { return this.owner.Repositories; }
        }

        public IFileOperationsExecuter Agent
        {
            get { return this.owner.Agent; }
        }

        public Clients.ProcessResults ExecuteCommandLine(string fileName, string arguments, string workingDirectory)
        {
            return this.owner.ExecuteCommandLine(fileName, arguments, workingDirectory);
        }

        /// <summary>
        /// Copies files and subfolders from sourceFolder to targetFolder.
        /// </summary>
        /// <param name="sourceFolder">A path of the folder to be copied</param>
        /// <param name="targetFolder">A path of a folder to copy files to.  If targetFolder doesn't exist, it is created.</param>
        private void CopyNonGitFiles(string sourceFolder, string targetFolder)
        {
            var agent = (IFileOperationsExecuter)this.Agent;

            if (!agent.DirectoryExists2(sourceFolder))
                return;

            agent.CreateDirectory(targetFolder);

            var entry = agent.GetDirectoryEntry(new GetDirectoryEntryCommand
            {
                Path = sourceFolder,
                IncludeRootPath = true,
                Recurse = true
            }).Entry;

            char separator = agent.GetDirectorySeparator();
            string[] filesToCopy = entry.Flatten().SelectMany(di => di.Files).Select(fi => fi.Path).Where(path => !path.Contains(@"\.git\")).ToArray();

            Func<string, string, string> combinePaths = (p1, p2) =>
            {
                return p1.TrimEnd(separator) + separator + p2.TrimStart(separator);
            };

            agent.FileCopyBatch(
                sourceFolder,
                filesToCopy,
                targetFolder,
                filesToCopy.Select(name => combinePaths(targetFolder, name.Substring(sourceFolder.Length))).ToArray(),
                true,
                true
             );
        }

        private void EnsureRepoIsPresent(IGitRepository repo)
        {
            var fileOps = (IFileOperationsExecuter)this.Agent;
            var repoPath = repo.GetFullRepositoryPath(fileOps);
            if (!fileOps.DirectoryExists2(repoPath) || !fileOps.DirectoryExists2(fileOps.CombinePath(repoPath, ".git")))
            {
                fileOps.CreateDirectory(repoPath);
                this.GitClient.CloneRepo(repo);
            }
        }

        private DirectoryEntryInfo GetDirectoryEntryInfo(GitPath path)
        {
            if (path.Repository == null)
            {
                return new DirectoryEntryInfo(
                    string.Empty,
                    string.Empty,
                    Repositories.Select(repo => new DirectoryEntryInfo(repo.RepositoryName, repo.RepositoryName, null, null)).ToArray(),
                    null
                );
            }
            else if (path.PathSpecifiedBranch == null)
            {
                this.EnsureRepoIsPresent(path.Repository);

                return new DirectoryEntryInfo(
                    path.Repository.RepositoryName,
                    path.Repository.RepositoryName,
                    this.GitClient.EnumBranches(path.Repository)
                        .Select(branch => new DirectoryEntryInfo(branch, GitPath.BuildSourcePath(path.Repository.RepositoryName, branch, null), null, null))
                        .ToArray(),
                    null
                );
            }
            else
            {
                this.EnsureRepoIsPresent(path.Repository);
                this.GitClient.UpdateLocalRepo(path.Repository, path.PathSpecifiedBranch, null);

                var de = ((IFileOperationsExecuter)this.Agent).GetDirectoryEntry(new GetDirectoryEntryCommand()
                {
                    Path = path.PathOnDisk,
                    Recurse = false,
                    IncludeRootPath = false
                }).Entry;

                var subDirs = de.SubDirectories
                    .Where(entry => !entry.Name.StartsWith(".git"))
                    .Select(subdir => new DirectoryEntryInfo(subdir.Name, GitPath.BuildSourcePath(path.Repository.RepositoryName, path.PathSpecifiedBranch, subdir.Path.Replace('\\', '/')), null, null))
                    .ToArray();

                var files = de.Files
                    .Select(file => new FileEntryInfo(file.Name, GitPath.BuildSourcePath(path.Repository.RepositoryName, path.PathSpecifiedBranch, file.Path.Replace('\\', '/'))))
                    .ToArray();

                return new DirectoryEntryInfo(
                    de.Name,
                    path.ToString(),
                    subDirs,
                    files
                );
            }
        }
    }
}
