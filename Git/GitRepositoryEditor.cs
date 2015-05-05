//using Inedo.BuildMaster.Extensibility.Providers.SourceControl;
//using Inedo.BuildMaster.Web.Controls;
//using Inedo.BuildMaster.Web.Controls.Extensions;
//using Inedo.Web.Controls;

//namespace Inedo.BuildMasterExtensions.Git
//{
//    internal sealed class GitRepositoryEditor : RepositoryEditorBase
//    {
//        private ValidatingTextBox txtRemoteRepoPath;

//        protected override void CreateChildControls()
//        {
//            this.txtRemoteRepoPath = new ValidatingTextBox { Width = 300, Required = true };

//            this.Controls.Add(
//                new StandardFormField(
//                    "Remote Repository URL:",
//                    this.txtRemoteRepoPath)
//            );
//        }

//        public override void BindToForm(RepositoryBase _extension)
//        {
//            var extension = (GitRepository)_extension;

//            this.EnsureChildControls();
//            this.txtRemoteRepoPath.Text = extension.RemoteRepositoryUrl;
//        }

//        public override RepositoryBase CreateFromForm()
//        {
//            this.EnsureChildControls();
//            return new GitRepository
//            {
//                RepositoryPath = GitPath.BuildPathFromUrl(this.txtRemoteRepoPath.Text),
//                RemoteRepositoryUrl = this.txtRemoteRepoPath.Text
//            };
//        }
//    }
//}
