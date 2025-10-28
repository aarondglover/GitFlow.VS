using System.Windows;

namespace GitFlowVS.Extension.Dialogs
{
    public partial class InitDialog : Window
    {
        public string MasterBranch => MasterBranchText.Text;
        public string DevelopBranch => DevelopBranchText.Text;
        public string FeaturePrefix => FeaturePrefixText.Text;
        public string ReleasePrefix => ReleasePrefixText.Text;
        public string HotfixPrefix => HotfixPrefixText.Text;
        public string SupportPrefix => SupportPrefixText.Text;
        public string VersionTagPrefix => VersionTagPrefixText.Text;

        public InitDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
