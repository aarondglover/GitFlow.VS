using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace GitFlowVS.Extension
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("1F9974CD-16C3-4AEF-AED2-0CE37988E2F1")]
    public class GitFlowToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitFlowToolWindow"/> class.
        /// </summary>
        public GitFlowToolWindow() : base(null)
        {
            this.Caption = "GitFlow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new GitFlowToolWindowControl();
        }
    }
}
