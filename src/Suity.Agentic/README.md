# Suity.Agentic

Suity.Agentic is a cross-platform editor application built on Avalonia UI, implementing a full-featured Agentic Workflow Editor. It features a dockable interface system, document management with syntax highlighting and code folding, ImGui-style startup screen, unified service architecture, complete menu system, and cross-platform support for Windows, Linux, and macOS.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.Agentic is the main application of the Suity ecosystem, bringing together all editor components into a cohesive cross-platform desktop application. Built on Avalonia UI with .NET 10, it provides a professional-grade IDE experience with dockable panels, multiple document types, ImGui-based custom views, and a comprehensive service architecture. The application follows the MVVM pattern and supports fuzzy search for efficient resource navigation.

## Features

### Application Core

- **Program Entry** - Application entry point building and starting the Avalonia desktop application
- **SuityApp** - Main application class managing window creation, configuration loading/saving, and project open/close workflows
- **AvaEditorDevice** - Core editor device singleton initializing and registering all editor services, handling Rex events and navigation requests
- **View Locator** - Avalonia view locator providing automatic ViewModel to View mapping

### Configuration Management

- **Editor App Config** - Application configuration model managing language settings and recently opened project records
- **Editor Official Config** - Official server configuration defining template, extension, and remote resource URLs with local directories
- **App Config Service** - Application configuration reading service based on Microsoft.Extensions.Configuration

### Dockable Interface

- **Editor Dock Container** - Main dock container managing tool panels, document area, layout serialization and restoration
- **Editor Document Content** - Document content host packaging document views into dockable panels with close and save logic
- **Editor Tool Content** - Tool window content host providing ImGui or native control rendering for dockable tool panels
- **Dock Extensions** - Extension methods converting DockHint enum to DockModel DockMode
- **Window State Manager** - Saving and restoring window position, size, and state through JSON serialization

### Main Window Views

- **Main Window** - Main editor window managing dock layout, unsaved document prompts, and window setting persistence
- **Main View** - Main view user control containing project title button and navigation search button
- **Menu View** - Menu bar view dynamically building main menu structure
- **ImGui Dialog Window** - ImGui dialog window wrapper supporting modal and non-modal display
- **Splash Window** - Startup/progress window displaying loading operations and progress bar

### Startup Interface

- **Startup Window** - Startup window hosting project selection ImGui interface
- **Startup Project GUI** - ImGui-based startup GUI supporting open recent projects, create new projects, and language switching
- **Startup Styles** - Startup window theme and style definitions including navigation buttons and project list styles
- **Startup Helper** - Startup utility providing file download and image caching functionality

### Text Editing

- **Avalonia Text Document View** - Text document view based on AvaloniaEdit with syntax highlighting, code folding, undo/redo, and clipboard operations
- **Brace Folding Strategy** - Brace-based code folding strategy for C#, Java, JS, TS, and other languages
- **HTML Folding Strategy** - HTML/XML document folding strategy supporting element and comment folding
- **JSON Folding Strategy** - JSON document folding strategy supporting brace and bracket folding

### Selection System

- **Selection Window** - Modal selection dialog supporting search filtering and single/multi-select
- **Selection Model** - Selection item tree model with fuzzy filtering and hierarchical navigation
- **Selection Tree View** - Selection item tree view control displaying name and preview columns
- **Filtered Selection List** - Fuzzy selection filtering interface with three implementations (ScoreSharp, FastFuzzy, FuzzySharp)

### ViewModel Layer

- **ViewModel Base** - ViewModel base class based on CommunityToolkit.Mvvm
- **Main Window ViewModel** - Main window ViewModel containing file operations, edit operations, and other command methods

### Services

- **Clipboard Service** - System clipboard service providing async text read/write
- **Dialog Service** - Dialog service implementing file open and other operations
- **Async Dialog Service** - Async dialog service providing message box, input box, file/folder selectors, and more
- **Document View Manager** - Document view manager resolving active document and opened document list
- **Icon Service** - Icon service getting corresponding icons by file path or asset ID
- **ImGui Service** - ImGui service creating dialogs, tree views, and draw items
- **License Service** - License service implementing community edition feature permissions
- **Progress Service** - Progress service displaying loading progress in startup window or status bar
- **Selection Service** - Selection service showing single/multi-select GUI dialogs
- **Tool Window Service** - Tool window service auto-scanning and registering all tool windows
- **Sync Type Resolver** - Sync type resolver supporting type aliases and serialization name mapping
- **System Service** - System service providing file monitoring, type resolution, RSA encryption, CRC32 calculation, and more
- **Platform OS** - Cross-platform abstraction layer handling recycle bin operations for Windows/Linux/macOS

### Menu Commands

- **Root Main Menus** - Root menu structure definition with File, Edit, View, Tool, Help main menus
- **File Menus** - File menu commands (save, save all, exit)
- **Edit Menus** - Edit menu commands (undo, redo, copy, cut, paste, navigate, find references, etc.)
- **View Menus** - View menu commands (tool window toggle, reset layout, toggle description display)
- **Tool Menus** - Tool menu commands (project settings)
- **Help Menus** - Help menu commands (about dialog)
- **Locate Menus** - Navigation, jump, and search menu commands
- **Active Document Menu** - Menu commands targeting active documents
- **Dock Menus** - Right-click context menus for document and tool dock tabs
- **Text Editor Menus** - Text editor right-click menus (copy, cut, paste, select all)

### Utilities

- **Windows Native Methods** - Windows P/Invoke declarations for file operations and icon handling
- **Live Stream Appender** - LLM streaming output appender for real-time dialogue interface updates
- **Full Length Property Row Drawer** - Property grid row drawer implementing full-width layout for property rows and groups

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     SuityApp                             │
│              (Main Application Class)                    │
├─────────────────────────────────────────────────────────┤
│                  AvaEditorDevice                         │
│         (Service Registry & Initialization)              │
├──────────────┬──────────────┬───────────────────────────┤
│   Dock       │  Document    │   Services                │
│  Container   │  Management  │   (Clipboard, Dialog,     │
│              │              │    Icon, ImGui, etc.)     │
├──────────────┼──────────────┼───────────────────────────┤
│   Menu       │  Selection   │   Startup                 │
│  Commands    │  System      │   Interface               │
├──────────────┴──────────────┴───────────────────────────┤
│              Avalonia UI Framework                       │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library
- **Suity.Rex** - Reactive data management and DI
- **Suity.DataSync** - Data synchronization and serialization
- **Suity.Graphics** - Graphics abstraction layer
- **Suity.ImGui** - ImGui framework
- **Suity.ImGui.BuildIn** - ImGui built-in implementations
- **Suity.ImGui.Avalonia** - Avalonia ImGui integration
- **Suity.Editor** - Editor framework interfaces
- **Suity.Editor.Internal** - Editor internal implementations
- **Suity.Editor.ImGui** - Editor ImGui components
- **Suity.Editor.AIGC** - AIGC core framework
- **Avalonia UI** - Cross-platform UI framework
- **Dock.Avalonia** - Docking library for Avalonia
- **AvaloniaEdit** - Text editor control for Avalonia
- **SkiaSharp** - 2D graphics rendering
- **CommunityToolkit.Mvvm** - MVVM toolkit
- **Microsoft.Extensions.Configuration** - Configuration framework

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
