using System.Collections.Generic;
using Inedo.BuildMaster.Extensibility.Providers.SourceControl;

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

        public abstract IEnumerable<string> EnumBranches(SourceRepository repo);
        public abstract void UpdateLocalRepo(SourceRepository repo, string branch, string tag);
        public abstract void ApplyTag(SourceRepository repo, string tag);
        public abstract GitCommit GetLastCommit(SourceRepository repo, string branch);
        public abstract void CloneRepo(SourceRepository repo);
        public abstract void ValidateConnection(SourceRepository repo);

        protected ProcessResults ExecuteGitCommand(SourceRepository repo, string command, params string[] args)
        {
            return this.Provider.ExecuteCommandLine(this.GitExePath, command + " " + string.Join(" ", args), repo.GetDiskPath(this.Provider.Agent));
        }
    }
}
