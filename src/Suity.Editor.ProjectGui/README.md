# Suity.Editor.ProjectGui

Suity.Editor.ProjectGui is the project view module providing an ImGui-based tree file browser for managing and navigating project assets, workspaces, and publish directories. It supports rich context menu operations including file creation, deletion, renaming, package import/export, reference finding, and asset publishing.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.Editor.ProjectGui provides the primary file browser interface for the Suity editor. It uses a PathNode architecture to abstract different file and directory types into specialized node classes, each responsible for its own display, drag-drop, and double-click behavior. The command system provides an extensible menu operation framework with context-aware menu item visibility.

## Features

### Core Components

- **ProjectGui** - Main project view GUI component displaying project tree and handling file/workspace navigation
- **ProjectViewPlugin** - Plugin providing project tree browser and related services
- **ProjectPathTreeView** - Specialized headerless path tree view for project asset browser
- **ProjectRootCommand** - Root context menu command aggregating all available menu operations
- **ProjectViewConfig** - Persistent configuration state storing expanded tree node paths
- **IProjectGui** - Interface providing access to project view GUI and operations

### Tree Node Types

- **Asset Root Node** - Assets root directory node handling file drag-drop and file bunch creation
- **Asset Directory Node** - Resource directory node with drag-drop support
- **Asset File Node** - Resource file node handling asset association, attachment files, and render targets
- **Asset Element Node** - Asset element (sub-asset) node displaying render status and error/warning indicators
- **Asset Field Node** - Field node within asset elements
- **Publish Root Node** - Local publish folder root node
- **Render Target Node** - Render target node in project tree
- **User Code File Node** - User code file node in code library
- **Bunch Inner File Node** - File node within file bunch assets
- **Workspace File System Nodes** - Workspace manager, root, directory, and file nodes
- **Workspace Reference Nodes** - Code generation reference groups and assembly reference groups

### File Operations

- **Create File Commands** - File creation by category and folder creation
- **Clone File** - Cloning selected file nodes with incrementing name
- **Delete File/Directory** - Deleting selected directory, file, or workspace nodes
- **Rename** - Renaming selected file or directory nodes
- **Explore** - Opening selected node location in system file explorer
- **Open File** - Opening selected file in appropriate editor or external program

### Package Operations

- **Import Package** - Importing packages into the project
- **Export** - Exporting selected assets, directories, or workspaces as packages

### Asset Operations

- **Find Reference** - Finding all references to selected asset or element
- **Publish** - Publishing selected asset file
- **Show Problem** - Displaying analysis problems for selected asset file
- **File Bunch Commands** - Optimizing file bunch asset storage

### Workspace Management

- **Assembly References** - Adding assembly references, system references, and disabled references
- **Model References** - Adding model references, render flow references, file bunch references, and user code references
- **Build Commands** - Executing workspace full render, incremental render, and file render
- **Controller Management** - Changing workspace controller type and creating new workspaces
- **Navigation** - Navigating to workspace file, reference, or assembly definitions
- **Solution Commands** - Compiling/building solution and opening solution in IDE
- **User Code Commands** - User code operations including commit and restore
- **Render File Binding** - Binding/unbinding user-occupied files to render output
- **Workspace Configuration** - Setting external main path and workspace configuration settings

### Icon Cache

- **Icon Cache** - Caching icons used by the project for efficient display

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                      ProjectGui                          │
│              (Main Project View GUI)                     │
├─────────────────────────────────────────────────────────┤
│                    PathNode Tree                         │
│  ┌──────────┬──────────────┬──────────────────────┐     │
│  │Asset     │  Workspace   │   Publish            │     │
│  │Nodes     │  Nodes       │   Nodes              │     │
│  └──────────┴──────────────┴──────────────────────┘     │
├─────────────────────────────────────────────────────────┤
│                  Command System                          │
│     (File, Package, Asset, Workspace Operations)         │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library
- **Suity.Editor** - Editor framework interfaces
- **Suity.ImGui** - ImGui framework
- **Suity.ImGui.BuildIn** - ImGui built-in implementations

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
