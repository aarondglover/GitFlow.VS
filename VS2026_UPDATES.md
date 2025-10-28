# GitFlow.VS - VS2026 Update Notes

## Changes Made for VS2026 Support

### Overview
Updated the GitFlow extension to support Visual Studio 2026 and moved away from exclusive TeamExplorer integration to a more flexible menu-based approach.

### Key Modifications

#### 1. VSIX Manifest (`source.extension.vsixmanifest`)
- Updated version to 2.0.0.0
- Changed display name to "GitFlow for Visual Studio 2026"
- Updated installation target to VS version [18.0,19.0)
- Removed VS-specific MPF dependency
- Removed "TeamExplorer" from tags
- Updated prerequisites to require VS 18.0+

#### 2. New Menu Structure (`GitFlowVSCommands.vsct`)
Created a new command table defining:
- Main menu group under Extensions
- GitFlow submenu
- Commands for:
  - Show GitFlow Window
  - Initialize GitFlow
  - Start/Finish Feature
  - Start/Finish Release
  - Start/Finish Hotfix

#### 3. New Tool Window (`GitFlowToolWindow.cs` & `GitFlowToolWindowControl.xaml`)
- Created a dockable tool window for GitFlow
- Displays current repository state
- Shows active features and releases
- Provides initialization UI when needed
- Independent of TeamExplorer framework

#### 4. Command Handlers (`GitFlowCommands.cs`)
- Implemented command handlers for all menu items
- Uses async/await pattern for VS services
- Currently shows tool window for most operations
- Can be extended with modal dialogs for specific operations

#### 5. Helper Classes (`GitFlowHelper.cs`)
- Utility functions for checking GitFlow installation
- Reduces code duplication

#### 6. Package Updates (`GitFlow.VS.ExtensionPackage.cs`)
- Properly implements AsyncPackage pattern
- Registers menu resources
- Registers tool window
- Initializes commands asynchronously
- Follows VS2022+ best practices

### Backward Compatibility

The extension maintains backward compatibility with existing TeamExplorer integration:
- All existing TeamExplorer sections remain functional
- GitFlowPage, GitFlowNavigationItem still work
- ViewModels and UI components unchanged
- Users can choose between menu-based or TeamExplorer workflows

### Architecture

```
Extensions Menu (new)
├── GitFlow
    ├── Show GitFlow Window → Opens tool window
    ├── Initialize GitFlow → Opens tool window with init UI
    ├── Start Feature... → Opens tool window (future: modal dialog)
    ├── Finish Feature... → Opens tool window (future: modal dialog)
    ├── Start Release... → Opens tool window (future: modal dialog)
    ├── Finish Release... → Opens tool window (future: modal dialog)
    ├── Start Hotfix... → Opens tool window (future: modal dialog)
    └── Finish Hotfix... → Opens tool window (future: modal dialog)

Team Explorer (existing)
└── GitFlow page
    ├── Action Section
    ├── Features Section
    ├── Releases Section
    └── Init Section
```

### Future Enhancements

1. **Modal Dialogs**: Create dedicated dialogs for start/finish operations instead of relying on tool window
2. **Remove TeamExplorer**: Once VS2026 is released and stable, consider removing TeamExplorer integration entirely
3. **Async Operations**: Add proper async support for long-running Git operations
4. **Progress Reporting**: Use VS status bar or progress windows for operations
5. **Keyboard Shortcuts**: Add default keyboard shortcuts for common operations
6. **Context Menus**: Add GitFlow commands to Solution Explorer context menu

### Testing Notes

Since this is a VS extension that requires Windows and Visual Studio to build:
- Extension requires VS2022+ SDK for development
- Target runtime is VS2026 (currently using VS2022 SDK with version 18.x manifest)
- Manual testing required in Visual Studio environment
- Linux build environment cannot compile/test the extension

### Migration Path

For users upgrading from previous versions:
1. Extension will continue to work with existing TeamExplorer workflow
2. New Extensions > GitFlow menu provides alternative access
3. Tool window offers quick overview of repository state
4. No breaking changes to existing functionality
