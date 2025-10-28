using System.Windows;

namespace GitFlowVS.Extension.Dialogs
{
    public partial class StartFeatureDialog : Window
    {
        public string FeatureName => FeatureNameText.Text.Trim();

        public StartFeatureDialog()
        {
            InitializeComponent();
            FeatureNameText.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FeatureName))
            {
                MessageBox.Show("Please enter a feature name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
