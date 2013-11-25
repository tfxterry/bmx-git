using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace Inedo.BuildMasterExtensions.Git
{
    /// <summary>
    /// Wraps functionality for Git repositories and paths.
    /// </summary>
    internal sealed class GitPath
    {
        private static readonly Regex PathSanitizerRegex = new Regex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]", RegexOptions.Compiled);

        public static char RepositorySeparatorChar = '|';

        public static char BranchSeparatorChar = ':';

        /// <summary>
        /// Gets the branch specified in the path, or null if no branch is specified.
        /// </summary>
        public string PathSpecifiedBranch { get; private set; }

        /// <summary>
        /// Gets the branch specified in the path or if no path is specified, falls back to the pre-v3.2 provider-level branch.
        /// </summary>
        public string Branch { get; private set; }

        /// <summary>
        /// Gets the Git repository.
        /// </summary>
        public IGitRepository Repository { get; private set; }

        /// <summary>
        /// Gets or sets the full source path on disk.
        /// </summary>
        public string PathOnDisk { get; private set; }

        /// <summary>
        /// Gets the path relative to the repository path.
        /// </summary>
        public string RelativePath { get; private set; }

        public GitPath(IGitSourceControlProvider provider, string sourcePath)
        {
            if (provider == null) throw new ArgumentNullException("provider");
            if (string.IsNullOrEmpty(sourcePath)) return;

            // pathParts => [repoName][repoPath]
            var pathParts = (sourcePath ?? "").Split(new[] { GitPath.RepositorySeparatorChar }, 2);
            if (pathParts.Length != 2) pathParts = new[] { pathParts[0], "" };

            this.Repository = provider.Repositories.SingleOrDefault(repo => repo.RepositoryName == pathParts[0]);

            // now split out the branch
            var relativePathParts = pathParts[1].Split(new[] { GitPath.BranchSeparatorChar }, 2);
            if (relativePathParts.Length == 2)
            {
                this.PathSpecifiedBranch = relativePathParts[0];
                this.RelativePath = relativePathParts[1];
            }
            else
            {
                this.RelativePath = pathParts[1];
            }

            var agent = provider.Agent;
            this.PathOnDisk = agent.CombinePath(this.Repository.GetFullRepositoryPath(agent), this.RelativePath);

            this.Branch = this.PathSpecifiedBranch;
            if (string.IsNullOrEmpty(this.Branch))
                this.Branch = "master";
        }

        public override string ToString()
        {
            if (this.Repository == null)
                return string.Empty;
            if (this.PathSpecifiedBranch == null)
                return this.Repository.RepositoryName;

            return string.Format("{0}{1}{2}{3}{4}", this.Repository.RepositoryName, GitPath.RepositorySeparatorChar, this.PathSpecifiedBranch, GitPath.BranchSeparatorChar, this.RelativePath);
        }

        public static string BuildSourcePath(string repositoryName, string branch, string relativePath)
        {
            if (string.IsNullOrEmpty(repositoryName))
                return string.Empty;
            if (string.IsNullOrEmpty(branch))
                return repositoryName;
            if (relativePath == null)
                return string.Format("{0}{1}{2}{3}", repositoryName, GitPath.RepositorySeparatorChar, branch, GitPath.BranchSeparatorChar);

            // the DirectoryEntryInfo will include the directory of the repository (which is already handled by the repository name),
            // so it must be trimmmed from the front of the relative path in order for the Git actions to refer to the correct path
            string rootPathToTrim = repositoryName + "/";
            if (relativePath.StartsWith(rootPathToTrim))
                relativePath = relativePath.Substring(rootPathToTrim.Length);

            return string.Format("{0}{1}{2}{3}{4}", repositoryName, GitPath.RepositorySeparatorChar, branch, GitPath.BranchSeparatorChar, relativePath);
        }

        public static string BuildPathFromUrl(string url)
        {
            var uri = new UriBuilder(url);
            uri.UserName = null;
            uri.Password = null;

            return PathSanitizerRegex.Replace(uri.Uri.Authority + uri.Uri.AbsolutePath, "_");
        }
    }
}
