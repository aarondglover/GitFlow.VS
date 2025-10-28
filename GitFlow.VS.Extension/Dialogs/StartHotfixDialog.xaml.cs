using System.Windows;

namespace GitFlowVS.Extension.Dialogs
{
    public partial class StartHotfixDialog : Window
    {
        public string HotfixVersion => HotfixVersionText.Text.Trim();

        public StartHotfixDialog()
        {
            InitializeComponent();
            HotfixVersionText.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(HotfixVersion))
            {
                MessageBox.Show("Please enter a hotfix version.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
