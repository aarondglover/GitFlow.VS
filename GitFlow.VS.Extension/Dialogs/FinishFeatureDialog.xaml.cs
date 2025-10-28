using System.Collections.Generic;
using System.Windows;

namespace GitFlowVS.Extension.Dialogs
{
    public partial class FinishFeatureDialog : Window
    {
        public string SelectedFeature => FeatureComboBox.SelectedItem as string;

        public FinishFeatureDialog(IEnumerable<string> features)
        {
            InitializeComponent();
            
            foreach (var feature in features)
            {
                FeatureComboBox.Items.Add(feature);
            }

            if (FeatureComboBox.Items.Count > 0)
            {
                FeatureComboBox.SelectedIndex = 0;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (FeatureComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a feature to finish.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
