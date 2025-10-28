using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using TeamExplorer.Common;
using System;
using System.Threading;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using Microsoft.TeamFoundation.Controls;
using System.Linq;

namespace GitFlowVS.Extension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.GuidGitFlowVsExtensionPkgString)]
    public sealed class GitFlowVSExtension : AsyncPackage
    {
        public GitFlowVSExtension()
        {
        }

        protected override System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            UserSettings.ServiceProvider = this;
            
            // Initialize commands on UI thread
            JoinableTaskFactory.RunAsync(async () =>
            {
                await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                
                var commandService = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
                if (commandService != null)
                {
                    // Add command for Extensions menu
                    AddMenuCommand(commandService, (int)GuidList.CmdidGitFlowCommand);
                    
                    // Add command for Git menu
                    AddMenuCommand(commandService, (int)GuidList.CmdidGitFlowGitMenuCommand);
                }
            });
            
            return System.Threading.Tasks.Task.FromResult<object>(null);
        }

        private void AddMenuCommand(OleMenuCommandService commandService, int commandId)
        {
            var menuCommandID = new CommandID(GuidList.GuidGitFlowVsExtensionCmdSet, commandId);
            var menuItem = new OleMenuCommand(ShowGitFlowPage, menuCommandID);
            menuItem.BeforeQueryStatus += OnBeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            if (sender is OleMenuCommand menuCommand)
            {
                // Show command only when a Git repository is active
                var gitService = GetService(typeof(IGitExt)) as IGitExt;
                menuCommand.Visible = gitService != null && gitService.ActiveRepositories.Any();
            }
        }

        private void ShowGitFlowPage(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            try
            {
                var teamExplorer = GetService(typeof(ITeamExplorer)) as ITeamExplorer;
                if (teamExplorer != null)
                {
                    teamExplorer.NavigateToPage(new Guid(GuidList.GitFlowPage), null);
                }
            }
            catch (Exception ex)
            {
                // Log error if available
                var outputWindow = GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                if (outputWindow != null)
                {
                    Guid customGuid = new Guid("B85225F6-B15E-4A8A-AF6E-2BE96A4FE672");
                    IVsOutputWindowPane pane;
                    outputWindow.GetPane(ref customGuid, out pane);
                    if (pane != null)
                    {
                        pane.OutputString($"Error opening GitFlow page: {ex.Message}\n");
                    }
                }
            }
        }
    }
}
