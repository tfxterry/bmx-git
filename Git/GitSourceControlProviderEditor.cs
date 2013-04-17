using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Git
{
    internal sealed class GitSourceControlProviderEditor : ProviderEditorBase
    {
        private CheckBox chkUseStandardGitClient;
        private SourceControlFileFolderPicker txtGitExecutablePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitSourceControlProviderEditor"/> class.
        /// </summary>
        public GitSourceControlProviderEditor()
        {
            this.ValidateBeforeSave += this.GitSourceControlProviderEditor_ValidateBeforeSave;
        }

        public override void BindToForm(ProviderBase extension)
        {
            this.EnsureChildControls();

            var provider = (GitSourceControlProvider)extension;
            this.txtGitExecutablePath.Text = provider.GitExecutablePath;
        }

        public override ProviderBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new GitSourceControlProvider
            {
                GitExecutablePath = this.txtGitExecutablePath.Text
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
                Required = false,
            };

            var ctlExePathField = new StandardFormField("Git Executable Path:", this.txtGitExecutablePath);

            this.Controls.Add(
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

        private void GitSourceControlProviderEditor_ValidateBeforeSave(object sender, ValidationEventArgs<ProviderBase> e)
        {
            if (this.chkUseStandardGitClient.Checked && string.IsNullOrEmpty(this.txtGitExecutablePath.Text))
            {
                e.ValidLevel = ValidationLevels.Error;
                e.Message = "You must provide a Git client to use if not using the built-in Git client.";
            }
        }
    }
}
