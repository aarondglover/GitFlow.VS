using System;
using System.ComponentModel.Design;
using System.Linq;
using GitFlowVS.Extension.Dialogs;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using Task = System.Threading.Tasks.Task;

namespace GitFlowVS.Extension
{
    /// <summary>
    /// Command handler for GitFlow commands
    /// </summary>
    internal sealed class GitFlowCommands
    {
        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid(GuidList.GuidGitFlowVsExtensionCmdSetString);

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;
        
        private IVsOutputWindowPane outputWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitFlowCommands"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private GitFlowCommands(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            // Register commands
            RegisterCommand(commandService, 0x0100, this.ShowToolWindow);
            RegisterCommand(commandService, 0x0101, this.InitGitFlow);
            RegisterCommand(commandService, 0x0102, this.StartFeature);
            RegisterCommand(commandService, 0x0103, this.FinishFeature);
            RegisterCommand(commandService, 0x0104, this.StartRelease);
            RegisterCommand(commandService, 0x0105, this.FinishRelease);
            RegisterCommand(commandService, 0x0106, this.StartHotfix);
            RegisterCommand(commandService, 0x0107, this.FinishHotfix);
            
            InitializeOutputWindow();
        }
        
        private async void InitializeOutputWindow()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var outWindow = await ServiceProvider.GetServiceAsync(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (outWindow != null)
            {
                var customGuid = new Guid("B85225F6-B15E-4A8A-AF6E-2BE96A4FE672");
                outWindow.CreatePane(ref customGuid, "GitFlow.VS", 1, 1);
                outWindow.GetPane(ref customGuid, out outputWindow);
            }
        }

        private void RegisterCommand(OleMenuCommandService commandService, int commandId, EventHandler handler)
        {
            var menuCommandID = new CommandID(CommandSet, commandId);
            var menuItem = new MenuCommand(handler, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GitFlowCommands Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IAsyncServiceProvider ServiceProvider => this.package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in GitFlowCommands's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new GitFlowCommands(package, commandService);
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.package.FindToolWindow(typeof(GitFlowToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        private async void InitGitFlow(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            try
            {
                var gitService = await ServiceProvider.GetServiceAsync(typeof(IGitExt)) as IGitExt;
                var activeRepo = gitService?.ActiveRepositories?.FirstOrDefault();
                
                if (activeRepo == null)
                {
                    System.Windows.MessageBox.Show(
                        "No Git repository is currently open. Please open a Git repository first.",
                        "Initialize GitFlow",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var dialog = new InitDialog();
                if (dialog.ShowDialog() == true)
                {
                    var gf = new VsGitFlowWrapper(activeRepo.RepositoryPath, outputWindow);
                    
                    var result = gf.Init(new GitFlowRepoSettings
                    {
                        DevelopBranch = dialog.DevelopBranch,
                        MasterBranch = dialog.MasterBranch,
                        FeatureBranch = dialog.FeaturePrefix,
                        ReleaseBranch = dialog.ReleasePrefix,
                        HotfixBranch = dialog.HotfixPrefix,
                        SupportBranch = dialog.SupportPrefix,
                        VersionTag = dialog.VersionTagPrefix
                    });

                    if (result.Success)
                    {
                        System.Windows.MessageBox.Show(
                            "GitFlow has been successfully initialized for this repository!",
                            "Initialize GitFlow",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Information);
                        
                        // Refresh the tool window if it's open
                        RefreshToolWindow();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(
                            $"Failed to initialize GitFlow:\n{result.CommandOutput}",
                            "Initialize GitFlow",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                System.Windows.MessageBox.Show(
                    $"Error initializing GitFlow: {ex.Message}",
                    "Initialize GitFlow",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private async void StartFeature(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            try
            {
                var gitService = await ServiceProvider.GetServiceAsync(typeof(IGitExt)) as IGitExt;
                var activeRepo = gitService?.ActiveRepositories?.FirstOrDefault();
                
                if (activeRepo == null)
                {
                    System.Windows.MessageBox.Show("No Git repository is currently open.", "Start Feature",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var gf = new VsGitFlowWrapper(activeRepo.RepositoryPath, outputWindow);
                if (!gf.IsInitialized)
                {
                    System.Windows.MessageBox.Show("Repository is not initialized for GitFlow. Please initialize GitFlow first.",
                        "Start Feature", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var dialog = new StartFeatureDialog();
                if (dialog.ShowDialog() == true)
                {
                    var result = gf.StartFeature(dialog.FeatureName);
                    
                    if (result.Success)
                    {
                        System.Windows.MessageBox.Show($"Feature '{dialog.FeatureName}' started successfully!",
                            "Start Feature", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        RefreshToolWindow();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show($"Failed to start feature:\n{result.CommandOutput}",
                            "Start Feature", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                System.Windows.MessageBox.Show($"Error starting feature: {ex.Message}", "Start Feature",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async void FinishFeature(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            try
            {
                var gitService = await ServiceProvider.GetServiceAsync(typeof(IGitExt)) as IGitExt;
                var activeRepo = gitService?.ActiveRepositories?.FirstOrDefault();
                
                if (activeRepo == null)
                {
                    System.Windows.MessageBox.Show("No Git repository is currently open.", "Finish Feature",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var gf = new VsGitFlowWrapper(activeRepo.RepositoryPath, outputWindow);
                if (!gf.IsInitialized)
                {
                    System.Windows.MessageBox.Show("Repository is not initialized for GitFlow.",
                        "Finish Feature", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var features = gf.AllFeatures;
                if (features == null || !features.Any())
                {
                    System.Windows.MessageBox.Show("No active features found.",
                        "Finish Feature", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    return;
                }

                var dialog = new FinishFeatureDialog(features);
                if (dialog.ShowDialog() == true)
                {
                    var result = gf.FinishFeature(dialog.SelectedFeature);
                    
                    if (result.Success)
                    {
                        System.Windows.MessageBox.Show($"Feature '{dialog.SelectedFeature}' finished successfully!",
                            "Finish Feature", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        RefreshToolWindow();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show($"Failed to finish feature:\n{result.CommandOutput}",
                            "Finish Feature", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                System.Windows.MessageBox.Show($"Error finishing feature: {ex.Message}", "Finish Feature",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async void StartRelease(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            try
            {
                var gitService = await ServiceProvider.GetServiceAsync(typeof(IGitExt)) as IGitExt;
                var activeRepo = gitService?.ActiveRepositories?.FirstOrDefault();
                
                if (activeRepo == null)
                {
                    System.Windows.MessageBox.Show("No Git repository is currently open.", "Start Release",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var gf = new VsGitFlowWrapper(activeRepo.RepositoryPath, outputWindow);
                if (!gf.IsInitialized)
                {
                    System.Windows.MessageBox.Show("Repository is not initialized for GitFlow. Please initialize GitFlow first.",
                        "Start Release", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var dialog = new StartReleaseDialog();
                if (dialog.ShowDialog() == true)
                {
                    var result = gf.StartRelease(dialog.ReleaseVersion);
                    
                    if (result.Success)
                    {
                        System.Windows.MessageBox.Show($"Release '{dialog.ReleaseVersion}' started successfully!",
                            "Start Release", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        RefreshToolWindow();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show($"Failed to start release:\n{result.CommandOutput}",
                            "Start Release", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                System.Windows.MessageBox.Show($"Error starting release: {ex.Message}", "Start Release",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async void FinishRelease(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            try
            {
                var gitService = await ServiceProvider.GetServiceAsync(typeof(IGitExt)) as IGitExt;
                var activeRepo = gitService?.ActiveRepositories?.FirstOrDefault();
                
                if (activeRepo == null)
                {
                    System.Windows.MessageBox.Show("No Git repository is currently open.", "Finish Release",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var gf = new VsGitFlowWrapper(activeRepo.RepositoryPath, outputWindow);
                if (!gf.IsInitialized)
                {
                    System.Windows.MessageBox.Show("Repository is not initialized for GitFlow.",
                        "Finish Release", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var releases = gf.AllReleases;
                if (releases == null || !releases.Any())
                {
                    System.Windows.MessageBox.Show("No active releases found.",
                        "Finish Release", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    return;
                }

                var dialog = new FinishReleaseDialog(releases);
                if (dialog.ShowDialog() == true)
                {
                    var result = gf.FinishRelease(dialog.SelectedRelease);
                    
                    if (result.Success)
                    {
                        System.Windows.MessageBox.Show($"Release '{dialog.SelectedRelease}' finished successfully!",
                            "Finish Release", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        RefreshToolWindow();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show($"Failed to finish release:\n{result.CommandOutput}",
                            "Finish Release", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                System.Windows.MessageBox.Show($"Error finishing release: {ex.Message}", "Finish Release",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async void StartHotfix(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            try
            {
                var gitService = await ServiceProvider.GetServiceAsync(typeof(IGitExt)) as IGitExt;
                var activeRepo = gitService?.ActiveRepositories?.FirstOrDefault();
                
                if (activeRepo == null)
                {
                    System.Windows.MessageBox.Show("No Git repository is currently open.", "Start Hotfix",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var gf = new VsGitFlowWrapper(activeRepo.RepositoryPath, outputWindow);
                if (!gf.IsInitialized)
                {
                    System.Windows.MessageBox.Show("Repository is not initialized for GitFlow. Please initialize GitFlow first.",
                        "Start Hotfix", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var dialog = new StartHotfixDialog();
                if (dialog.ShowDialog() == true)
                {
                    var result = gf.StartHotfix(dialog.HotfixVersion);
                    
                    if (result.Success)
                    {
                        System.Windows.MessageBox.Show($"Hotfix '{dialog.HotfixVersion}' started successfully!",
                            "Start Hotfix", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        RefreshToolWindow();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show($"Failed to start hotfix:\n{result.CommandOutput}",
                            "Start Hotfix", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                System.Windows.MessageBox.Show($"Error starting hotfix: {ex.Message}", "Start Hotfix",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async void FinishHotfix(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            try
            {
                var gitService = await ServiceProvider.GetServiceAsync(typeof(IGitExt)) as IGitExt;
                var activeRepo = gitService?.ActiveRepositories?.FirstOrDefault();
                
                if (activeRepo == null)
                {
                    System.Windows.MessageBox.Show("No Git repository is currently open.", "Finish Hotfix",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var gf = new VsGitFlowWrapper(activeRepo.RepositoryPath, outputWindow);
                if (!gf.IsInitialized)
                {
                    System.Windows.MessageBox.Show("Repository is not initialized for GitFlow.",
                        "Finish Hotfix", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var hotfixes = gf.AllHotfixes;
                if (hotfixes == null || !hotfixes.Any())
                {
                    System.Windows.MessageBox.Show("No active hotfixes found.",
                        "Finish Hotfix", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    return;
                }

                var dialog = new FinishHotfixDialog(hotfixes);
                if (dialog.ShowDialog() == true)
                {
                    var result = gf.FinishHotfix(dialog.SelectedHotfix);
                    
                    if (result.Success)
                    {
                        System.Windows.MessageBox.Show($"Hotfix '{dialog.SelectedHotfix}' finished successfully!",
                            "Finish Hotfix", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        RefreshToolWindow();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show($"Failed to finish hotfix:\n{result.CommandOutput}",
                            "Finish Hotfix", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                System.Windows.MessageBox.Show($"Error finishing hotfix: {ex.Message}", "Finish Hotfix",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void RefreshToolWindow()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            try
            {
                ToolWindowPane window = this.package.FindToolWindow(typeof(GitFlowToolWindow), 0, false);
                if (window != null && window.Content is GitFlowToolWindowControl control)
                {
                    // Trigger a refresh by calling the control's refresh method if it exists
                    var method = control.GetType().GetMethod("RefreshContent", 
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    method?.Invoke(control, null);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        private void ShowDialogForAction(string action)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            // This method is no longer used, but keeping it for compatibility
            ShowToolWindow(this, EventArgs.Empty);
        }
    }
}
