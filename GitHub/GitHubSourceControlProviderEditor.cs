using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.GitHub
{
    internal sealed class GitHubSourceControlProviderEditor : ProviderEditorBase
    {
        private ValidatingTextBox txtOrganizationName;
        private ValidatingTextBox txtUserName;
        private PasswordTextBox txtPassword;
        private CheckBox chkUseStandardGitClient;
        private SourceControlFileFolderPicker txtGitExecutablePath;

        public GitHubSourceControlProviderEditor()
        {
            this.ValidateBeforeSave += this.GitHubSourceControlProviderEditor_ValidateBeforeSave;
        }

        public override void BindToForm(ProviderBase extension)
        {
            this.EnsureChildControls();

            var provider = (GitHubSourceControlProvider)extension;
            this.txtGitExecutablePath.Text = provider.GitExecutablePath;
            this.txtOrganizationName.Text = provider.OrganizationName;
            this.txtUserName.Text = provider.UserName;
            this.txtPassword.Text = provider.Password;
        }

        public override ProviderBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new GitHubSourceControlProvider
            {
                GitExecutablePath = this.txtGitExecutablePath.Text,
                OrganizationName = this.txtOrganizationName.Text,
                UserName = this.txtUserName.Text,
                Password = this.txtPassword.Text
            };
        }

        protected override void CreateChildControls()
        {
            this.chkUseStandardGitClient = new CheckBox
            {
                Text = "Use Standard Git Client"
            };

            this.txtGitExecutablePath = new SourceControlFileFolderPicker
            {
                ServerId = this.EditorContext.ServerId,
                Required = false
            };

            this.txtOrganizationName = new ValidatingTextBox { Required = false, DefaultText = "Optional", Width = 300 };
            this.txtUserName = new ValidatingTextBox { Required = true, Width = 300 };
            this.txtPassword = new PasswordTextBox { Required = true, Width = 300 };

            var ctlExePathField = new StandardFormField("Git Executable Path:", this.txtGitExecutablePath);

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
                ),
                new FormFieldGroup(
                    "Git Client",
                    "This extension includes a lightweight Git client for Windows. To use an alternate Git client, check the box and provide the path of the other client.",
                    false,
                    new StandardFormField(string.Empty, this.chkUseStandardGitClient),
                    ctlExePathField
                )
            );

            this.Controls.BindVisibility(this.chkUseStandardGitClient, ctlExePathField);
        }

        private void GitHubSourceControlProviderEditor_ValidateBeforeSave(object sender, ValidationEventArgs<ProviderBase> e)
        {
            if (this.chkUseStandardGitClient.Checked && string.IsNullOrEmpty(this.txtGitExecutablePath.Text))
            {
                e.ValidLevel = ValidationLevels.Error;
                e.Message = "You must provide a Git client to use if not using the built-in Git client.";
            }
        }
    }
}
