using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.GitHub
{
    /// <summary>
    /// Custom editor for the <see cref="GitHubIssueTrackingProvider"/> class.
    /// </summary>
    internal sealed class GitHubIssueTrackingProviderEditor : ProviderEditorBase
    {
        private ValidatingTextBox txtOrganizationName;
        private ValidatingTextBox txtUserName;
        private PasswordTextBox txtPassword;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubIssueTrackingProviderEditor"/> class.
        /// </summary>
        public GitHubIssueTrackingProviderEditor()
        {
        }

        public override void BindToForm(ProviderBase extension)
        {
            this.EnsureChildControls();

            var provider = (GitHubIssueTrackingProvider)extension;
            this.txtOrganizationName.Text = provider.OrganizationName;
            this.txtUserName.Text = provider.UserName;
            this.txtPassword.Text = provider.Password;
        }
        public override ProviderBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new GitHubIssueTrackingProvider
            {
                OrganizationName = this.txtOrganizationName.Text,
                UserName = this.txtUserName.Text,
                Password = this.txtPassword.Text
            };
        }

        protected override void CreateChildControls()
        {
            this.txtOrganizationName = new ValidatingTextBox { Required = false, DefaultText = "Optional", Width = 300 };
            this.txtUserName = new ValidatingTextBox { Required = true, Width = 300 };
            this.txtPassword = new PasswordTextBox { Required = true, Width = 300 };

            this.Controls.Add(
                new FormFieldGroup(
                    "Organization",
                    "Optionally provide the organization name which owns the repositories.",
                    false,
                    new StandardFormField(
                        "Organization:",
                        this.txtOrganizationName
                    )
                ),
                new FormFieldGroup(
                    "Authentication",
                    "Provide the user name and password of a GitHub user which has access to the desired repositories.",
                    false,
                    new StandardFormField(
                        "User Name:",
                        this.txtUserName
                    ),
                    new StandardFormField(
                        "Password:",
                        this.txtPassword
                    )
                )
            );
        }
    }
}
