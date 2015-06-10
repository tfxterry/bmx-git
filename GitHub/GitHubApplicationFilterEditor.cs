using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Extensibility.IssueTrackerConnections;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.GitHub
{
    internal sealed class GitHubApplicationFilterEditor : IssueTrackerApplicationConfigurationEditorBase
    {
        private ComboSelect ddlRepositories;

        public override void BindToForm(IssueTrackerApplicationConfigurationBase extension)
        {
            var filter = (GitHubApplicationFilter)extension;

            if (!string.IsNullOrEmpty(filter.Owner) || !string.IsNullOrEmpty(filter.Repository))
                this.ddlRepositories.SelectedValue = filter.Owner + "/" + filter.Repository;
        }
        public override IssueTrackerApplicationConfigurationBase CreateFromForm()
        {
            var parts = this.ddlRepositories.SelectedValue.Split(new[] { '/' }, 2, StringSplitOptions.None);

            return new GitHubApplicationFilter
            {
                Owner = parts[0],
                Repository = parts[1]
            };
        }

        protected override void CreateChildControls()
        {
            var application = StoredProcs.Applications_GetApplication(this.EditorContext.ApplicationId)
                .Execute()
                .Applications_Extended
                .First();

            var repositories = GetProjects(application);

            this.ddlRepositories = new ComboSelect();
            this.ddlRepositories.Items.AddRange(
                from r in repositories
                orderby r.Name
                select new ListItem(r.Name, r.Owner + "/" + r.Name)
            );

            this.Controls.Add(
                new SlimFormField("GitHub repository:", this.ddlRepositories)
            );
        }

        private static IEnumerable<GitHubRepository> GetProjects(Tables.Applications_Extended application)
        {
            if (application.IssueTracking_Provider_Id == null)
                return Enumerable.Empty<GitHubRepository>();

            using (var provider = (GitHubIssueTrackingProvider)Util.Providers.CreateProviderFromId<IssueTrackerConnectionBase>((int)application.IssueTracking_Provider_Id))
            {
                return provider.GetRepositories();
            }
        }
    }
}
