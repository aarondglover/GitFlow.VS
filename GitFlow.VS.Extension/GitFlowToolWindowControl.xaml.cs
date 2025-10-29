using GitFlowVS.Extension.Dialogs;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GitFlowVS.Extension
{
    /// <summary>
    /// Interaction logic for GitFlowToolWindowControl.
    /// </summary>
    public partial class GitFlowToolWindowControl : UserControl
    {
        private IGitExt gitService;
        private IVsOutputWindowPane outputWindow;

        public GitFlowToolWindowControl()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            try
            {
                // Get Git service
                gitService = await ServiceProvider.GetGlobalServiceAsync(typeof(IGitExt)) as IGitExt;
                
                // Setup output window
                var outWindow = await ServiceProvider.GetGlobalServiceAsync(typeof(SVsOutputWindow)) as IVsOutputWindow;
                if (outWindow != null)
                {
                    var customGuid = new Guid("B85225F6-B15E-4A8A-AF6E-2BE96A4FE672");
                    outWindow.CreatePane(ref customGuid, "GitFlow.VS", 1, 1);
                    outWindow.GetPane(ref customGuid, out outputWindow);
                }

                if (gitService != null)
                {
                    gitService.PropertyChanged += OnGitServicePropertyChanged;
                    RefreshContent();
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                System.Windows.MessageBox.Show($"Error initializing GitFlow: {ex.Message}", "GitFlow Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnGitServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshContent();
        }

        public void RefreshContent()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            var activeRepo = gitService?.ActiveRepositories?.FirstOrDefault();
            if (activeRepo == null)
            {
                CurrentBranchText.Text = "(no repository)";
                StatusContent.Children.Clear();
                StatusContent.Children.Add(new TextBlock 
                { 
                    Text = "No Git repository is currently open. Please open a Git repository to use GitFlow.", 
                    Margin = new Thickness(0, 0, 0, 0), 
                    TextWrapping = TextWrapping.Wrap 
                });
                
                // Disable all buttons
                UpdateButtonStates(false, false);
                
                // Clear settings
                ClearSettings();
                return;
            }

            CurrentBranchText.Text = activeRepo.CurrentBranch?.Name ?? "(unknown)";
            StatusContent.Children.Clear();

            // Check if GitFlow is installed
            if (!GitFlowHelper.IsGitFlowInstalled())
            {
                var installMessage = new TextBlock
                {
                    Text = "GitFlow is not installed. The extension will attempt to install it when you initialize a repository.",
                    Margin = new Thickness(0, 0, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                };
                StatusContent.Children.Add(installMessage);
                UpdateButtonStates(true, false);
                ClearSettings();
                return;
            }

            // Check if repository is initialized for GitFlow
            var gf = new VsGitFlowWrapper(activeRepo.RepositoryPath, outputWindow);
            if (!gf.IsInitialized)
            {
                // Show init message
                var initMessage = new TextBlock
                {
                    Text = "Repository is not initialized for GitFlow. Click 'Initialize GitFlow' above to configure GitFlow for this repository.",
                    Margin = new Thickness(0, 0, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                };
                StatusContent.Children.Add(initMessage);
                UpdateButtonStates(true, false);
                ClearSettings();
                return;
            }

            // Show message that GitFlow is ready
            var infoText = new TextBlock
            {
                Text = "GitFlow is initialized and ready. Use the buttons above to manage your workflow.",
                Margin = new Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap
            };
            StatusContent.Children.Add(infoText);

            // Add a link to show existing features and releases
            var featuresExpander = new Expander
            {
                Header = "Current Features",
                IsExpanded = true,
                Margin = new Thickness(0, 0, 0, 10)
            };
            var featuresStack = new StackPanel();
            var features = gf.AllFeatures;
            if (features != null && features.Any())
            {
                foreach (var feature in features)
                {
                    featuresStack.Children.Add(new TextBlock { Text = $"• {feature}", Margin = new Thickness(10, 2, 0, 2) });
                }
            }
            else
            {
                featuresStack.Children.Add(new TextBlock { Text = "No active features", Margin = new Thickness(10, 2, 0, 2), FontStyle = FontStyles.Italic });
            }
            featuresExpander.Content = featuresStack;
            StatusContent.Children.Add(featuresExpander);

            var releasesExpander = new Expander
            {
                Header = "Current Releases",
                IsExpanded = true,
                Margin = new Thickness(0, 0, 0, 0)
            };
            var releasesStack = new StackPanel();
            var releases = gf.AllReleases;
            if (releases != null && releases.Any())
            {
                foreach (var release in releases)
                {
                    releasesStack.Children.Add(new TextBlock { Text = $"• {release}", Margin = new Thickness(10, 2, 0, 2) });
                }
            }
            else
            {
                releasesStack.Children.Add(new TextBlock { Text = "No active releases", Margin = new Thickness(10, 2, 0, 2), FontStyle = FontStyles.Italic });
            }
            releasesExpander.Content = releasesStack;
            StatusContent.Children.Add(releasesExpander);

            // Update button states
            UpdateButtonStates(true, true);
            
            // Load settings
            LoadSettings(gf);
        }

        private void UpdateButtonStates(bool enableInit, bool enableOthers)
        {
            InitButton.IsEnabled = enableInit;
            StartFeatureButton.IsEnabled = enableOthers;
            FinishFeatureButton.IsEnabled = enableOthers;
            StartReleaseButton.IsEnabled = enableOthers;
            FinishReleaseButton.IsEnabled = enableOthers;
            StartHotfixButton.IsEnabled = enableOthers;
            FinishHotfixButton.IsEnabled = enableOthers;
        }

        private void LoadSettings(VsGitFlowWrapper gf)
        {
            // Load settings from git config (per-repository)
            SettingsMasterBranch.Text = gf.MasterBranch ?? "master";
            SettingsDevelopBranch.Text = gf.DevelopBranch ?? "develop";
            SettingsFeaturePrefix.Text = gf.FeaturePrefix ?? "feature/";
            SettingsReleasePrefix.Text = gf.ReleasePrefix ?? "release/";
            SettingsHotfixPrefix.Text = gf.HotfixPrefix ?? "hotfix/";
            SettingsSupportPrefix.Text = gf.SupportPrefix ?? "support/";
            SettingsVersionTagPrefix.Text = gf.TagPrefix ?? "";
        }

        private void ClearSettings()
        {
            SettingsMasterBranch.Text = "";
            SettingsDevelopBranch.Text = "";
            SettingsFeaturePrefix.Text = "";
            SettingsReleasePrefix.Text = "";
            SettingsHotfixPrefix.Text = "";
            SettingsSupportPrefix.Text = "";
            SettingsVersionTagPrefix.Text = "";
        }

        // Button click handlers
        private void InitButton_Click(object sender, RoutedEventArgs e)
        {
            var activeRepo = gitService?.ActiveRepositories?.FirstOrDefault();
            if (activeRepo == null)
            {
                MessageBox.Show("No active repository. Please open a Git repository.", "GitFlow", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new InitDialog();
            if (dialog.ShowDialog() == true)
            {
                GitFlowCommands.ExecuteInitGitFlow(activeRepo.RepositoryPath, outputWindow, dialog);
                RefreshContent();
            }
        }

        private void StartFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            var activeRepo = gitService?.ActiveRepositories?.FirstOrDefault();
            if (activeRepo == null) return;

            var dialog = new StartFeatureDialog();
            if (dialog.ShowDialog() == true)
            {
                GitFlowCommands.ExecuteStartFeature(activeRepo.RepositoryPath, outputWindow, dialog.FeatureName);
                RefreshContent();
            }
        }

        private void FinishFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            var activeRepo = gitService?.ActiveRepositories?.FirstOrDefault();
            if (activeRepo == null) return;

            var gf = new VsGitFlowWrapper(activeRepo.RepositoryPath, outputWindow);
            var features = gf.AllFeatures;
            
            if (features == null || !features.Any())
            {
                MessageBox.Show("No active features to finish.", "GitFlow", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new FinishFeatureDialog(features);
            if (dialog.ShowDialog() == true)
            {
                GitFlowCommands.ExecuteFinishFeature(activeRepo.RepositoryPath, outputWindow, dialog.SelectedFeature);
                RefreshContent();
            }
        }

        private void StartReleaseButton_Click(object sender, RoutedEventArgs e)
        {
            var activeRepo = gitService?.ActiveRepositories?.FirstOrDefault();
            if (activeRepo == null) return;

            var dialog = new StartReleaseDialog();
            if (dialog.ShowDialog() == true)
            {
                GitFlowCommands.ExecuteStartRelease(activeRepo.RepositoryPath, outputWindow, dialog.ReleaseName);
                RefreshContent();
            }
        }

        private void FinishReleaseButton_Click(object sender, RoutedEventArgs e)
        {
            var activeRepo = gitService?.ActiveRepositories?.FirstOrDefault();
            if (activeRepo == null) return;

            var gf = new VsGitFlowWrapper(activeRepo.RepositoryPath, outputWindow);
            var releases = gf.AllReleases;
            
            if (releases == null || !releases.Any())
            {
                MessageBox.Show("No active releases to finish.", "GitFlow", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new FinishReleaseDialog(releases);
            if (dialog.ShowDialog() == true)
            {
                GitFlowCommands.ExecuteFinishRelease(activeRepo.RepositoryPath, outputWindow, dialog.SelectedRelease);
                RefreshContent();
            }
        }

        private void StartHotfixButton_Click(object sender, RoutedEventArgs e)
        {
            var activeRepo = gitService?.ActiveRepositories?.FirstOrDefault();
            if (activeRepo == null) return;

            var dialog = new StartHotfixDialog();
            if (dialog.ShowDialog() == true)
            {
                GitFlowCommands.ExecuteStartHotfix(activeRepo.RepositoryPath, outputWindow, dialog.HotfixName);
                RefreshContent();
            }
        }

        private void FinishHotfixButton_Click(object sender, RoutedEventArgs e)
        {
            var activeRepo = gitService?.ActiveRepositories?.FirstOrDefault();
            if (activeRepo == null) return;

            var gf = new VsGitFlowWrapper(activeRepo.RepositoryPath, outputWindow);
            var hotfixes = gf.AllHotfixes;
            
            if (hotfixes == null || !hotfixes.Any())
            {
                MessageBox.Show("No active hotfixes to finish.", "GitFlow", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new FinishHotfixDialog(hotfixes);
            if (dialog.ShowDialog() == true)
            {
                GitFlowCommands.ExecuteFinishHotfix(activeRepo.RepositoryPath, outputWindow, dialog.SelectedHotfix);
                RefreshContent();
            }
        }
    }
}
