using GitFlowVS.Extension.UI;
using GitFlowVS.Extension.ViewModels;
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

        private void RefreshContent()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            var activeRepo = gitService?.ActiveRepositories?.FirstOrDefault();
            if (activeRepo == null)
            {
                CurrentBranchText.Text = "(no repository)";
                MainContent.Children.Clear();
                MainContent.Children.Add(new TextBlock 
                { 
                    Text = "No Git repository is currently open. Please open a Git repository to use GitFlow.", 
                    Margin = new Thickness(0, 10, 0, 0), 
                    TextWrapping = TextWrapping.Wrap 
                });
                return;
            }

            CurrentBranchText.Text = activeRepo.CurrentBranch ?? "(unknown)";
            MainContent.Children.Clear();

            // Check if GitFlow is installed
            if (!GitFlowHelper.IsGitFlowInstalled())
            {
                var installUI = new InstallGitFlowUI();
                MainContent.Children.Add(installUI);
                return;
            }

            // Check if repository is initialized for GitFlow
            var gf = new VsGitFlowWrapper(activeRepo.RepositoryPath, outputWindow);
            if (!gf.IsInitialized)
            {
                // Show init UI
                var initSection = new GitFlowInitSection();
                // The section needs context that we don't have here, so just show a message
                var initMessage = new TextBlock
                {
                    Text = "Repository is not initialized for GitFlow. Please use 'Extensions > GitFlow > Initialize GitFlow' or open the GitFlow page in Team Explorer to initialize.",
                    Margin = new Thickness(0, 10, 0, 10),
                    TextWrapping = TextWrapping.Wrap
                };
                MainContent.Children.Add(initMessage);
                return;
            }

            // Show message that GitFlow is ready
            var infoText = new TextBlock
            {
                Text = "GitFlow is initialized. Use the menu commands under Extensions > GitFlow to start/finish features, releases, and hotfixes.",
                Margin = new Thickness(0, 10, 0, 10),
                TextWrapping = TextWrapping.Wrap
            };
            MainContent.Children.Add(infoText);

            // Add a link to show existing features and releases
            var featuresExpander = new Expander
            {
                Header = "Current Features",
                IsExpanded = false,
                Margin = new Thickness(0, 5, 0, 5)
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
            MainContent.Children.Add(featuresExpander);

            var releasesExpander = new Expander
            {
                Header = "Current Releases",
                IsExpanded = false,
                Margin = new Thickness(0, 5, 0, 5)
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
            MainContent.Children.Add(releasesExpander);
        }
    }
}
