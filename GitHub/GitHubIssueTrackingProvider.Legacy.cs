using System;
using System.Collections.Generic;
using System.Linq;
using Inedo.BuildMaster.Extensibility.Providers.IssueTracking;

namespace Inedo.BuildMasterExtensions.GitHub
{
    partial class GitHubIssueTrackingProvider : ICategoryFilterable
    {
        private GitHubApplicationFilter legacyFilter;

        string[] ICategoryFilterable.CategoryIdFilter
        {
            get
            {
                if (this.legacyFilter != null)
                    return new[] { this.legacyFilter.Owner + "/" + this.legacyFilter.Repository };
                else
                    return null;
            }
            set
            {
                if (value != null && value.Length > 0)
                {
                    var categoryParts = value[0].Split(new[] { '/' }, 2, StringSplitOptions.None);
                    this.legacyFilter = new GitHubApplicationFilter
                    {
                        Owner = categoryParts[0],
                        Repository = categoryParts[1]
                    };
                }
                else
                {
                    this.legacyFilter = null;
                }
            }
        }
        string[] ICategoryFilterable.CategoryTypeNames
        {
            get { return new[] { "Repository" }; }
        }

        IssueTrackerCategory[] ICategoryFilterable.GetCategories()
        {
            return this.GitHub
                .EnumRepositories()
                .Select(r => new GitHubCategory(((Dictionary<string, object>)r["owner"])["login"] + "/" + r["name"], (string)r["name"]))
                .ToArray();
        }
    }
}
