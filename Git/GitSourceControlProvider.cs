using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Extensibility.Providers.SourceControl;
using Inedo.BuildMaster.Files;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Git
{
    /// <summary>
    /// A provider that integrates with the Git source control system.
    /// </summary>
    [ProviderProperties("Git", "Supports most versions of Git; requires Git to be installed on the server for use with an SSH Agent.")]
    [CustomEditor(typeof(GitSourceControlProviderEditor))]
    [RequiresInterface(typeof(IRemoteProcessExecuter))]
    [RequiresInterface(typeof(IFileOperationsExecuter))]
    public sealed partial class GitSourceControlProvider : MultipleRepositoryProviderBase<GitRepository>, IVersioningProvider, IRevisionProvider, IClientCommandProvider, IGitSourceControlProvider
    {
        private GitSourceControlProviderCommon wrappedProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitSourceControlProvider"/> class.
        /// </summary>
        public GitSourceControlProvider()
        {
        }

        /// <summary>
        /// Gets or sets the Git executable path.
        /// </summary>
        [Persistent]
        public string GitExecutablePath { get; set; }

        /// <summary>
        /// Gets or sets the name of the branch to fall back on for pre-v3.2 if there is no branch specified in the path.
        /// </summary>
        [Persistent]
        public string BranchName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the standard Git client.
        /// </summary>
        [Persistent]
        public bool UseStandardGitClient { get; set; }

        public override char DirectorySeparator
        {
            get { return '/'; }
        }

        private GitSourceControlProviderCommon WrappedProvider
        {
            get
            {
                if (this.wrappedProvider == null)
                    this.wrappedProvider = new GitSourceControlProviderCommon(this, this.UseStandardGitClient ? this.GitExecutablePath : null);

                return this.wrappedProvider;
            }
        }

        public override void GetLatest(string sourcePath, string targetPath)
        {
            this.WrappedProvider.GetLatest(sourcePath, targetPath);
        }

        public void ApplyLabel(string label, string sourcePath)
        {
            this.WrappedProvider.ApplyLabel(label, sourcePath);
        }

        public void GetLabeled(string label, string sourcePath, string targetPath)
        {
            this.WrappedProvider.GetLabeled(label, sourcePath, targetPath);
        }

        public override DirectoryEntryInfo GetDirectoryEntryInfo(string sourcePath)
        {
            return this.WrappedProvider.GetDirectoryEntryInfo(sourcePath);
        }

        public override byte[] GetFileContents(string filePath)
        {
            return this.WrappedProvider.GetFileContents(filePath);
        }

        public override bool IsAvailable()
        {
            return this.WrappedProvider.IsAvailable();
        }

        public override void ValidateConnection()
        {
            this.WrappedProvider.ValidateConnection();
        }

        public override string ToString()
        {
            if (this.Repositories.Length == 1)
                return "Git at " + Util.CoalesceStr(this.Repositories[0].RemoteRepositoryUrl, this.Repositories[0].RepositoryPath);
            else
                return "Git";
        }

        public byte[] GetCurrentRevision(string path)
        {
            return this.WrappedProvider.GetCurrentRevision(path);
        }

        public void ExecuteClientCommand(string commandName, string arguments)
        {
            if (!this.UseStandardGitClient)
            {
                this.LogError("Client commands only supported when using the standard Git client.");
                return;
            }

            var results = this.ExecuteCommandLine(
                this.GitExecutablePath,
                commandName + " " + arguments,
                null
            );

            foreach (var line in results.Output)
                this.LogInformation(line);

            foreach (var line in results.Error)
                this.LogError(line);
        }

        public IEnumerable<ClientCommand> GetAvailableCommands()
        {
            using (var stream = typeof(GitSourceControlProvider).Assembly.GetManifestResourceStream("Inedo.BuildMasterExtensions.Git.GitCommands.txt"))
            using (var reader = new StreamReader(stream))
            {
                var line = reader.ReadLine();
                while (line != null)
                {
                    var commandInfo = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    yield return new ClientCommand(commandInfo[0].Trim(), commandInfo[1].Trim());

                    line = reader.ReadLine();
                }
            }
        }

        public string GetClientCommandHelp(string commandName)
        {
            if (!this.UseStandardGitClient)
                throw new InvalidOperationException("Client commands only supported when using the standard Git client.");

            try
            {
                // the only way to get client help in Git output to the command line is to pass in an invalid command...
                return string.Join(Environment.NewLine, ExecuteCommandLine(this.GitExecutablePath, commandName + " -?", this.Repositories[0].RepositoryPath).Error.ToArray())
                    .Replace("error: unknown switch `?'", "");
            }
            catch (Exception e)
            {
                return "Help not available for the \"" + commandName + "\" command. The specific error message was: " + e.Message;
            }
        }

        public string GetClientCommandPreview()
        {
            return string.Empty;
        }

        public bool SupportsCommandHelp
        {
            get { return true; }
        }

        IGitRepository[] IGitSourceControlProvider.Repositories
        {
            get { return this.Repositories; }
        }

        IFileOperationsExecuter IGitSourceControlProvider.Agent
        {
            get { return (IFileOperationsExecuter)this.Agent; }
        }

        Clients.ProcessResults IGitSourceControlProvider.ExecuteCommandLine(string fileName, string arguments, string workingDirectory)
        {
            // This is not an ideal way to do this, but the idea here is to show an error if this is not used on a Windows client
            // Currently, only the Windows agent is an IRemoteCommandExecuter, so this will work
            if (!(this.Agent is IRemoteCommandExecuter))
                throw new NotAvailableException("The integrated Git client cannot be used on Linux. Enable the Use Standard Git Client option of the Git Provider.");

            var results = this.ExecuteCommandLine(fileName, arguments, workingDirectory);
            return new Clients.ProcessResults(results.ExitCode, results.Output, results.Error);
        }
    }
}
