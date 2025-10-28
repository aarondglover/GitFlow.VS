using GitFlowVS.Extension.UI;
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
                MainContent.Children.Add(new TextBlock { Text = "No Git repository is currently open. Please open a Git repository to use GitFlow.", Margin = new Thickness(0, 10, 0, 0), TextWrapping = TextWrapping.Wrap });
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
                var initUI = new InitUI();
                MainContent.Children.Add(initUI);
                return;
            }

            // Show the main GitFlow page UI sections
            ShowGitFlowSections(activeRepo.RepositoryPath);
        }

        private void ShowGitFlowSections(string repoPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            MainContent.Children.Clear();

            // Add action section
            var actionSection = new GitFlowActionSection();
            if (actionSection.SectionContent != null)
            {
                MainContent.Children.Add(actionSection.SectionContent as UIElement);
            }

            // Add features section
            var featuresSection = new GitFlowFeaturesSection();
            if (featuresSection.SectionContent != null)
            {
                var expander = new Expander
                {
                    Header = "Features",
                    Content = featuresSection.SectionContent,
                    IsExpanded = true,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                MainContent.Children.Add(expander);
            }

            // Add releases section
            var releasesSection = new GitFlowReleasesSection();
            if (releasesSection.SectionContent != null)
            {
                var expander = new Expander
                {
                    Header = "Releases",
                    Content = releasesSection.SectionContent,
                    IsExpanded = false,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                MainContent.Children.Add(expander);
            }
        }
    }
}
