# Suity.Editor

Suity.Editor is the core visual editor framework providing asset management, workspace management, flowchart editing, code rendering, type design, document management, undo/redo, and plugin extensibility. It serves as the foundation for building professional-grade editor applications.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.Editor provides a comprehensive framework for building visual editor applications. It defines the core abstractions and services for asset lifecycle management, workspace organization, visual programming through flowcharts, code generation from visual designs, type system design, document management, and a service-based architecture for extensibility. The framework is designed to be platform-agnostic, with UI implementations provided by separate modules.

## Features

### Asset Management

- **Asset Lifecycle** - Complete resource lifecycle management with metadata tracking and GUID-based identification
- **Asset Types** - Support for data tables, images, libraries, values, and groups
- **Reference Tracking** - Object-to-object reference management and resolution
- **Filtering** - Asset filtering by data type, instance type, and user code
- **Metadata** - Rich metadata storage with file-based persistence

### Workspace Management

- **Multi-Workspace** - Multiple workspace support with independent code repositories, references, and render targets
- **Controllers** - Pluggable workspace controllers for different workspace types
- **Render Tracking** - File render state tracking with modification type classification
- **Framework Configuration** - Target framework configuration for compilation

### Flowchart Editing

- **Node Graph** - Visual node-based editing with connectors, grouping, and annotations
- **Data & Action Flow** - Support for both data flow and action flow execution paradigms
- **Canvas Operations** - Zoom, pan, selection, and clipboard operations
- **Node Styles** - Customizable node visual appearance

### Code Rendering Engine

- **Render Targets** - Code file generation with naming rules and path management
- **Render Config** - Code generation parameters and configuration
- **Incremental Rendering** - File state tracking for incremental updates
- **Code Segments** - User code protection through segment-based parsing
- **Material System** - Code template processing and material utilities

### Type Design System

- **Type Definitions** - Visual design of structs, enums, abstract types, events, delegates, and logic modules
- **Field Design** - Field-level configuration with type, default value, and attributes
- **Function Design** - Function parameter and return type design
- **Type Manager** - Type registration, lookup, and native type integration
- **Design Documents** - Persistent design documents with `.stype` format

### Document System

- **Document Lifecycle** - Document open, close, and management through DocumentManager
- **Format Resolution** - Pluggable document format resolution
- **View Management** - Document view hosting and sub-view support
- **Text Documents** - Base text document handling with format serialization
- **Linked Documents** - Asset-linked documents with named item hierarchies

### Value System

- **SValue Types** - Flexible data value management including objects, arrays, delegates, dynamics, enums, and keys
- **Data Items** - Hierarchical data structure with SItem base class
- **Object Controller** - Object value lifecycle management
- **Sync Integration** - Integration with Suity.DataSync synchronization system

### Service Architecture

- **Editor Services** - Centralized service collection managing all editor services
- **Dialog Service** - User interaction dialog handling
- **Clipboard Service** - Copy/paste operations
- **Navigation Service** - Editor-internal navigation
- **Analysis Service** - Code and problem analysis
- **Localization Service** - Internationalization support
- **Menu Service** - Menu system management
- **Progress Service** - Loading progress display
- **Selection Service** - GUI selection dialogs
- **Tool Window Service** - Tool window management
- **Icon Service** - Icon resolution by file path or asset ID

### Undo/Redo System

- **Action History** - Complete operation history management
- **Macro Actions** - Grouping multiple operations into single undoable actions
- **Value Change Tracking** - Tracking value modifications for undo/redo
- **Sync Presets** - Preset state management for synchronization

### Plugin Architecture

- **Plugin Base** - Extensible plugin system for custom features
- **Service Registration** - Plugin-based service registration and initialization
- **GUI State** - UI state persistence through plugins

### Path Tree Views

- **Path Tree Model** - File system tree structure management
- **Monitor Model** - File change listening with automatic updates
- **Node Types** - Directory, file, root directory, text, and log node types
- **Lazy Loading** - Dummy nodes for deferred content loading

### Selection System

- **Selection Lists** - Single and multiple selection list management
- **Asset Selection** - Asset type selection with tree item representation
- **Member Selection** - Type member selection
- **Editor Object Selection** - Editor object selection handling

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   Editor Services                        │
│         (Dialog, Clipboard, Navigation, etc.)            │
├──────────────┬──────────────┬───────────────────────────┤
│   Asset      │  Workspace   │    Document               │
│  Manager     │  Manager     │    Manager                │
├──────────────┼──────────────┼───────────────────────────┤
│  Flowchart   │  Code Render │    Type Design            │
│  Editing     │  Engine      │    System                 │
├──────────────┴──────────────┴───────────────────────────┤
│              Value System & Undo/Redo                    │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library
- **Suity.Rex** - Reactive data management and DI
- **Suity.DataSync** - Data synchronization and serialization
- **Suity.Graphics** - Graphics abstraction layer
- **Suity.ImGui** - ImGui framework (for UI implementations)
- **Newtonsoft.Json** - JSON serialization
- **Pathoschild.Http.FluentClient** - HTTP client for network operations
- **MarkedNet** - Markdown parsing

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
