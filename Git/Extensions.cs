using System;
using System.IO;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Files;

namespace Inedo.BuildMasterExtensions.Git
{
    internal static class Extensions
    {
        public static string GetFullRepositoryPath(this IGitRepository repo, IFileOperationsExecuter agent)
        {
            return agent.CombinePath(agent.GetBaseWorkingDirectory(), "GitRepositories", repo.RepositoryPath);
        }
    }
}
