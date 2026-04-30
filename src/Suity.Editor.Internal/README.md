# Suity.Editor.Internal

Suity.Editor.Internal is the internal core implementation library for the Suity editor, providing the backend implementations for all editor services, project and workspace management, asset and document handling, type reflection, ImGui-based UI components, analysis services, and internationalization.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.Editor.Internal contains the concrete backend implementations (`BK` suffix) for the abstract interfaces defined in Suity.Editor. It follows a plugin-based architecture with `CorePlugin` as the entry point, managing the lifecycle of all editor components through `PluginManager`. This module is responsible for the actual execution of editor operations, bridging the abstract editor framework with concrete implementations.

## Features

### Editor Core

- **CorePlugin** - Core editor plugin registering base services (clipboard, reference manager, analysis) and defining render types
- **Project Backend** - Project lifecycle management (open/close), directory structure, settings, GUID tracking, and plugin state
- **Asset Manager Backend** - Asset registration, lookup, collection management, type binding, and metafile operations
- **Storage Manager Backend** - File and memory storage item management
- **Object Manager Backend** - Editor object entry management and ID resolution

### Workspace Management

- **Workspace Manager Backend** - Workspace lifecycle management including create, delete, rename, and file system watching
- **Workspace Backend** - Concrete workspace implementation with render target pages and record collections
- **Reference Items** - Assembly references, user file references, file bunch references, and render target library references
- **Auto Restore** - Automatic recovery collection for workspace state

### Service Implementations

- **Plugin Manager** - Plugin and assembly scanning, registration, and lifecycle management
- **Analysis Service Backend** - Object analysis, problem collection, and result generation
- **Reference Manager Backend** - Object reference tracking and management
- **Navigation Service Backend** - Editor-internal navigation implementation
- **File Asset Manager Backend** - File-based asset management
- **Menu Service** - Menu system implementation
- **Type Convert Service** - Type conversion between editor types and .NET types
- **JSON Schema Service** - JSON Schema validation and generation
- **Clipboard Service** - System clipboard integration

### Document Management

- **Document Manager Backend** - Document format registration, document lifecycle, and caching
- **Document Entry Backend** - Document metadata and state tracking
- **Text Document Format** - Plain text document format handling
- **Attributed Document Format** - Attribute-based document format resolution

### Type System

- **Native Type Reflector** - Assembly scanning and native type registration mapping .NET types to DType
- **Native Type Library** - Native type storage and lookup
- **DType Manager Backend** - DType registration and management
- **Type Definitions** - Type definition persistence and resolution

### Tree GUI & Inspector

- **TreeImGui** - ImGui-based tree view with selection, inspection, clipboard operations, and analysis
- **UndoableTreeImGui** - Tree view with undo/redo support
- **InspectorImGui** - Property grid inspector displaying object properties in a grid layout
- **Tree Document View** - Tree-based document viewing

### Analysis System

- **Project Analysis** - Project structure, settings, and workspace parsing
- **Project Loader** - Project loading and initialization
- **Plugin Loader** - Plugin discovery and loading
- **Problem Collectors** - Issue detection and collection for various editor components
- **Workspace Analysis** - Workspace structure analysis

### Internationalization

- **Loadable Localizer** - XML-based translation loading
- **Context Localizer** - Context-aware localization
- **Language Definition** - Language configuration and metadata
- **Auto-Load Localizer** - Automatic localizer loading on initialization

### Named Views

- **Named Item List** - Synchronized, displayable, and draggable named item lists
- **Named Sync List** - Named list with sync integration
- **Named Object List** - Named object usage tracking

### Node Query

- **Beacon JSON** - Hierarchical node data reading/writing using ComputerBeacon.Json
- **Newtonsoft JSON** - Node data reading/writing using Newtonsoft.Json

### Utilities

- **JSON Helper** - JSON serialization/deserialization with Newtonsoft.Json
- **AES/RSA Encryption** - Encryption helper utilities
- **CRC32 Algorithm** - CRC32 checksum calculation
- **File System Watcher** - Editor file system monitoring with soft watcher variant
- **String Validator** - String character validation
- **ID Generator** - Unique identifier generation
- **Score Sharp** - Scoring utility for fuzzy matching

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     CorePlugin                           │
│              (Entry Point & Registration)                │
├──────────────┬──────────────┬───────────────────────────┤
│   Project    │  Workspace   │    Services               │
│   Backend    │  Backend     │   (Analysis, Navigation,  │
│              │              │    Clipboard, Menu, etc.)  │
├──────────────┼──────────────┼───────────────────────────┤
│  Documents   │   Types      │    GUI                    │
│  Backend     │  Backend     │   (TreeImGui, Inspector)  │
├──────────────┴──────────────┴───────────────────────────┤
│              Analysis & I18N                             │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library
- **Suity.Rex** - Reactive data management and DI
- **Suity.DataSync** - Data synchronization and serialization
- **Suity.Graphics** - Graphics abstraction layer
- **Suity.ImGui** - ImGui framework
- **Suity.ImGui.BuildIn** - ImGui built-in implementations
- **Suity.Editor** - Editor framework interfaces
- **Newtonsoft.Json** - JSON serialization
- **BouncyCastle** - Cryptography library (for RSA PKCS#8)
- **ComputerBeacon.Json** - JSON library for node query

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
