using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.GitHub
{
    internal sealed class GitHubIssueTrackingProviderEditor : ProviderEditorBase
    {
        private ValidatingTextBox txtOrganizationName;
        private ValidatingTextBox txtUserName;
        private PasswordTextBox txtPassword;

        public override void BindToForm(ProviderBase extension)
        {
            var provider = (GitHubIssueTrackingProvider)extension;
            this.txtOrganizationName.Text = provider.OrganizationName;
            this.txtUserName.Text = provider.UserName;
            this.txtPassword.Text = provider.Password;
        }
        public override ProviderBase CreateFromForm()
        {
            return new GitHubIssueTrackingProvider
            {
                OrganizationName = this.txtOrganizationName.Text,
                UserName = this.txtUserName.Text,
                Password = this.txtPassword.Text
            };
        }

        protected override void CreateChildControls()
        {
            this.txtOrganizationName = new ValidatingTextBox { DefaultText = "none" };

            this.txtUserName = new ValidatingTextBox { Required = true };

            this.txtPassword = new PasswordTextBox { Required = true };

            this.Controls.Add(
                new SlimFormField("Organization:", this.txtOrganizationName),
                new SlimFormField("User name:", this.txtUserName),
                new SlimFormField("Password:", this.txtPassword)
            );
        }
    }
}
