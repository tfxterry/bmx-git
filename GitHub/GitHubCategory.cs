using System;
using Inedo.BuildMaster.Extensibility.Providers.IssueTracking;

namespace Inedo.BuildMasterExtensions.GitHub
{
    [Serializable]
    internal sealed class GitHubCategory : IssueTrackerCategory
    {
        public GitHubCategory(string categoryId, string categoryName)
            : base(categoryId, categoryName, null)
        {
        }
    }
}
