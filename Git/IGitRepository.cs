namespace Inedo.BuildMasterExtensions.Git
{
    internal interface IGitRepository
    {
        string RepositoryName { get; }
        string RepositoryPath { get; }
        string RemoteRepositoryUrl { get; }
    }
}
