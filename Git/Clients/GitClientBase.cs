using System.Collections.Generic;

namespace Inedo.BuildMasterExtensions.Git.Clients
{
    internal abstract class GitClientBase
    {
        protected GitClientBase(IGitSourceControlProvider provider)
        {
            this.Provider = provider;
        }

        protected IGitSourceControlProvider Provider { get; private set; }
        protected abstract string GitExePath { get; }

        public abstract IEnumerable<string> EnumBranches(IGitRepository repo);
        public abstract void UpdateLocalRepo(IGitRepository repo, string branch, string tag);
        public abstract void ApplyTag(IGitRepository repo, string tag);
        public abstract GitCommit GetLastCommit(IGitRepository repo, string branch);
        public abstract void CloneRepo(IGitRepository repo);
        public abstract void ValidateConnection();

        protected ProcessResults ExecuteGitCommand(IGitRepository repo, string command, params string[] args)
        {
            return this.Provider.ExecuteCommandLine(this.GitExePath, command + " " + string.Join(" ", args), repo.GetFullRepositoryPath(this.Provider.Agent));
        }
    }
}
