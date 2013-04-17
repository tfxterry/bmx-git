using System;
using Inedo.BuildMaster.Extensibility.Providers.IssueTracking;

namespace Inedo.BuildMasterExtensions.GitHub
{
    /// <summary>
    /// Represents a GitHub issue tracking category (repository).
    /// </summary>
    [Serializable]
    internal sealed class GitHubCategory : CategoryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubCategory"/> class.
        /// </summary>
        /// <param name="categoryId">The category id.</param>
        /// <param name="categoryName">Name of the category.</param>
        public GitHubCategory(string categoryId, string categoryName)
            : base(categoryId, categoryName, null)
        {
        }
    }
}
