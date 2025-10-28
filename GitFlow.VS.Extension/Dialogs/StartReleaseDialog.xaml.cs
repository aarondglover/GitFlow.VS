using System.Windows;

namespace GitFlowVS.Extension.Dialogs
{
    public partial class StartReleaseDialog : Window
    {
        public string ReleaseVersion => ReleaseVersionText.Text.Trim();

        public StartReleaseDialog()
        {
            InitializeComponent();
            ReleaseVersionText.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReleaseVersion))
            {
                MessageBox.Show("Please enter a release version.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
