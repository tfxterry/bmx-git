using System;

namespace Inedo.BuildMasterExtensions.GitHub
{
    [Serializable]
    internal sealed class GitHubRepository
    {
        public GitHubRepository(string owner, string name)
        {
            this.Owner = owner;
            this.Name = name;
        }

        public string Owner { get; private set; }
        public string Name { get; private set; }
    }
}
