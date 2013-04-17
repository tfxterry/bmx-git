using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMasterExtensions.Git.Clients;

namespace Inedo.BuildMasterExtensions.Git
{
    internal interface IGitSourceControlProvider
    {
        IGitRepository[] Repositories { get; }

        IFileOperationsExecuter Agent { get; }

        ProcessResults ExecuteCommandLine(string fileName, string arguments, string workingDirectory);
    }
}
