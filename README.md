# GitFlow for Visual Studio

[![Build Status](https://dev.azure.com/gitflowvs/GitFlowVS/_apis/build/status/GitFlow.VS)](https://dev.azure.com/gitflowvs/GitFlowVS/_build/latest?definitionId=1)

[![Release Status](https://vsrm.dev.azure.com/gitflowvs/_apis/public/Release/badge/5a9a320c-6abf-47d7-9c95-3befcfc93113/1/1)](https://dev.azure.com/gitflowvs/GitFlowVS/_release?definitionId=1)




### Features 
This extension integrates GitFlow into your Visual Studio development workflow. It provides a dedicated menu under **Extensions > GitFlow** with commands to easily create and finish feature, release and hotfix branches.

The extension also integrates with Team Explorer for those who prefer that interface (available in VS2022 and earlier).

It exposes the most common GitFlow options when finishing branches, such as options to delete branches, rebase on development branch and tagging of release branches.

Read more about this extension here:
http://blog.ehn.nu/2015/02/introducing-gitflow-for-visual-studio/

 

### Prerequisites
The extension requires Visual Studio 2022 or 2026 (it's also available for VS2019, VS2017 and VS2015 in older versions). It will install GitFlow for you if it is not found on the machine. Since GitFlow depends on Git for Windows, this must be installed before using the extension.

### Using the Extension

#### Via Extensions Menu (VS2026+)
1. Open a Git repository in Visual Studio
2. Navigate to **Extensions > GitFlow**
3. Use the menu commands to:
   - Initialize GitFlow in your repository
   - Start/Finish features, releases, and hotfixes
   - View the GitFlow tool window for an overview of your repository state

#### Via Team Explorer (VS2022 and earlier)
The extension also integrates with Team Explorer, providing a GitFlow page accessible from the Team Explorer navigation.
 

## Screenshots

### GitFlow Menu
The new Extensions > GitFlow menu provides quick access to all GitFlow operations.

### GitFlow Tool Window
The tool window shows the current state of your repository and active features/releases.

### Initialize repo for GitFlow

![Initialize](Images/gf_init.png)

### Start New Feature

![Start feature](Images/gf_startfeature.png)

### Finish Feature

![Finish feature](Images/gf_finishfeature.png)

### List of current features

![List features](Images/gf_features.png)
