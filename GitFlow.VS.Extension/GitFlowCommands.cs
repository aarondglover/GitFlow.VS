using System;
using System.ComponentModel.Design;
using System.Linq;
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

        private void InitGitFlow(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            // For initialization, we direct users to Team Explorer where the full UI exists
            // In the future, this could open a dedicated init dialog
            var message = "To initialize GitFlow in your repository:\n\n" +
                         "Option 1: Open Team Explorer > GitFlow page (click the GitFlow link in Team Explorer)\n" +
                         "Option 2: Use the GitFlow command-line tool\n\n" +
                         "Would you like to open Team Explorer now?";
            
            var result = System.Windows.MessageBox.Show(
                message,
                "Initialize GitFlow",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Information);
            
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                // Try to navigate to Team Explorer
                try
                {
                    var dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
                    if (dte != null)
                    {
                        // This will open Team Explorer - users can then click the GitFlow navigation item
                        dte.ExecuteCommand("View.TeamExplorer");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex);
                }
            }
        }

        private void StartFeature(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ShowDialogForAction("Start Feature");
        }

        private void FinishFeature(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ShowDialogForAction("Finish Feature");
        }

        private void StartRelease(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ShowDialogForAction("Start Release");
        }

        private void FinishRelease(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ShowDialogForAction("Finish Release");
        }

        private void StartHotfix(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ShowDialogForAction("Start Hotfix");
        }

        private void FinishHotfix(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ShowDialogForAction("Finish Hotfix");
        }

        private void ShowDialogForAction(string action)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            // For now, just show the tool window. In the future, we can implement
            // direct dialogs for each action
            ShowToolWindow(this, EventArgs.Empty);
        }
    }
}
