using System.Collections.Generic;
using System.Windows;

namespace GitFlowVS.Extension.Dialogs
{
    public partial class FinishHotfixDialog : Window
    {
        public string SelectedHotfix => HotfixComboBox.SelectedItem as string;

        public FinishHotfixDialog(IEnumerable<string> hotfixes)
        {
            InitializeComponent();
            
            foreach (var hotfix in hotfixes)
            {
                HotfixComboBox.Items.Add(hotfix);
            }

            if (HotfixComboBox.Items.Count > 0)
            {
                HotfixComboBox.SelectedIndex = 0;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (HotfixComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a hotfix to finish.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
