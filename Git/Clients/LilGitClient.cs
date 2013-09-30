using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Inedo.BuildMasterExtensions.Git.Clients
{
    internal sealed class LilGitClient : GitClientBase
    {
        public LilGitClient(IGitSourceControlProvider provider)
            : base(provider)
        {
        }

        protected override string GitExePath
        {
            get
            {
                return this.Provider.Agent.CombinePath(this.Provider.Agent.GetBaseWorkingDirectory(), string.Format(@"ExtTemp\{0}\lilgit.exe", typeof(LilGitClient).Assembly.GetName().Name));
            }
        }

        public override IEnumerable<string> EnumBranches(IGitRepository repo)
        {
            var result = this.ExecuteGitCommand(repo, "branches", "\"" + repo.RemoteRepositoryUrl + "\"");
            if (result.ExitCode != 0)
                throw new InvalidOperationException(string.Join(Environment.NewLine, result.Error.ToArray()));

            return result.Output;
        }

        public override void UpdateLocalRepo(IGitRepository repo, string branch, string tag)
        {
            ProcessResults result;

            var refspec = string.Format("refs/heads/{0}", string.IsNullOrEmpty(branch) ? "master" : branch);

            if (string.IsNullOrEmpty(tag))
                result = this.ExecuteGitCommand(repo, "get", "\"" + repo.RemoteRepositoryUrl + "\"", "\"" + refspec + "\"");
            else
                result = this.ExecuteGitCommand(repo, "gettag", "\"" + repo.RemoteRepositoryUrl + "\"", "\"" + tag + "\"", "\"" + refspec + "\"");

            if (result.ExitCode != 0)
                throw new InvalidOperationException(string.Join(Environment.NewLine, result.Error.ToArray()));
        }

        public override void ApplyTag(IGitRepository repo, string tag)
        {
            var result = this.ExecuteGitCommand(repo, "tag", "\"" + repo.RemoteRepositoryUrl + "\"", "\"" + tag + "\"", "BuildMaster", "\"Tagged by BuildMaster\"");
            if (result.ExitCode != 0)
                throw new InvalidOperationException(string.Join(Environment.NewLine, result.Error.ToArray()));
        }

        public override GitCommit GetLastCommit(IGitRepository repo, string branch)
        {
            var result = this.ExecuteGitCommand(repo, "lastcommit");
            if (result.ExitCode != 0)
                throw new InvalidOperationException(string.Join(Environment.NewLine, result.Error.ToArray()));

            var revStr = string.Join(string.Empty, result.Output.ToArray()).Trim();
            return new GitCommit(revStr);
        }

        public override void CloneRepo(IGitRepository repo)
        {
            var result = this.ExecuteGitCommand(repo, "clone", "\"" + repo.RemoteRepositoryUrl + "\"");
            if (result.ExitCode != 0)
                throw new InvalidOperationException(string.Join(Environment.NewLine, result.Error.ToArray()));
        }

        public override void ValidateConnection()
        {
            var repo = this.Provider.Repositories.FirstOrDefault();
            if (repo != null)
            {
                var result = this.ExecuteGitCommand(repo, "lastcommit");
                if (result.ExitCode != 0)
                    throw new InvalidOperationException(string.Join(Environment.NewLine, result.Error.ToArray()));
            }
        }
    }
}
