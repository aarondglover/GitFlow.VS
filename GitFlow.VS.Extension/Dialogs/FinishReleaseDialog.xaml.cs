using System.Collections.Generic;
using System.Windows;

namespace GitFlowVS.Extension.Dialogs
{
    public partial class FinishReleaseDialog : Window
    {
        public string SelectedRelease => ReleaseComboBox.SelectedItem as string;

        public FinishReleaseDialog(IEnumerable<string> releases)
        {
            InitializeComponent();
            
            foreach (var release in releases)
            {
                ReleaseComboBox.Items.Add(release);
            }

            if (ReleaseComboBox.Items.Count > 0)
            {
                ReleaseComboBox.SelectedIndex = 0;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReleaseComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a release to finish.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
