# GitFlow for Visual Studio

[![Build Status](https://dev.azure.com/gitflowvs/GitFlowVS/_apis/build/status/GitFlow.VS)](https://dev.azure.com/gitflowvs/GitFlowVS/_build/latest?definitionId=1)

[![Release Status](https://vsrm.dev.azure.com/gitflowvs/_apis/public/Release/badge/5a9a320c-6abf-47d7-9c95-3befcfc93113/1/1)](https://dev.azure.com/gitflowvs/GitFlowVS/_release?definitionId=1)


### Features 
This extension integrates GitFlow into your Visual Studio 2026 development workflow. It provides a dedicated menu under **Extensions > GitFlow** with commands to manage feature, release and hotfix branches.

The extension exposes common GitFlow operations through menu commands and a tool window that displays the current state of your repository.

Read more about GitFlow here:
http://nvie.com/posts/a-successful-git-branching-model/

### Prerequisites
The extension requires Visual Studio 2026. It will install GitFlow for you if it is not found on the machine. Since GitFlow depends on Git for Windows, this must be installed before using the extension.

### Using the Extension

1. Open a Git repository in Visual Studio 2026
2. Navigate to **Extensions > GitFlow**
3. Use the menu commands to:
   - **Show GitFlow Window** - View repository state and active branches
   - **Initialize GitFlow** - Set up GitFlow in your repository (currently via command line)
   - **Start/Finish Feature** - Manage feature branches
   - **Start/Finish Release** - Manage release branches
   - **Start/Finish Hotfix** - Manage hotfix branches

### GitFlow Tool Window
The tool window shows:
- Current branch
- Active features
- Active releases
- Repository initialization status

## Screenshots

### GitFlow Menu
Access all GitFlow operations from the Extensions > GitFlow menu.

### GitFlow Tool Window
View your repository's GitFlow state at a glance.
