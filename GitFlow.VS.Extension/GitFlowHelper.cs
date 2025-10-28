using GitFlow.VS;
using System;
using System.IO;
using System.Reflection;

namespace GitFlowVS.Extension
{
    /// <summary>
    /// Helper class for GitFlow operations
    /// </summary>
    public static class GitFlowHelper
    {
        public static bool IsGitFlowInstalled()
        {
            try
            {
                // Check if extension has been configured
                string binariesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Dependencies\\binaries");
                if (!Directory.Exists(binariesPath))
                    return false;

                var gitBinPath = GitHelper.GetGitBinPath();
                if (gitBinPath == null)
                    return false;

                string gitFlowFile = Path.Combine(gitBinPath, "git-flow");
                if (!File.Exists(gitFlowFile))
                    return false;
                    
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
