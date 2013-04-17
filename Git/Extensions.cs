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

        /// <summary>
        /// Determine if a directory on a remote agent exists.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is hack to work around a limitation in the IFileOperationsExecuter for BM less than 3.6
        /// </remarks>
        public static bool DirectoryExists2(this IFileOperationsExecuter agent, string path)
        {
            var proxyAgent = agent as IPersistedObjectExecuter;
            if (proxyAgent != null)
                return (bool)proxyAgent.ExecuteMethodOnXmlPersistedObject(Util.Persistence.SerializeToPersistedObjectXml(new ProxyHelper()), "DirectoryExists", new object[] { path });

            var results = agent.GetDirectoryEntry(new GetDirectoryEntryCommand { Path = path, Recurse = false });
            return results != null && results.Entry != null && results.Exceptions == null;
        }
    }

    [Serializable]
    internal sealed class ProxyHelper
    {
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }
    }
}
