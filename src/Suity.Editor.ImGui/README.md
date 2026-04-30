# Suity.Editor.ImGui

Suity.Editor.ImGui is an ImGui-based editor UI component library providing tree view systems, property editing, flowchart rendering, log console, and document management for the Suity editor framework.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.Editor.ImGui bridges the Suity.Editor framework with the Suity.ImGui rendering system, providing concrete UI implementations for editor components. It includes tree view systems with multiple layout options, a comprehensive property grid, flowchart rendering support, and document view management. The library uses an external abstraction layer (Externals) pattern to decouple UI rendering logic from specific implementations.

## Features

### Tree View System

- **Column Templates** - Configurable multi-column rendering with adjustable column widths
- **Three-Column Template** - Predefined template with name, description, and preview columns
- **Tree View Base** - Abstract tree view with selection, menus, keyboard navigation, and scrolling
- **Path Tree View** - Path-based tree view with drag-drop support and simple implementation
- **Headerless Path Tree** - Path tree without header row, supporting inline editing and open/delete requests
- **Column Path Tree** - Column-layout path tree with name, description, and preview columns
- **Path Tree Model** - Bridge between PathTreeModel and ImGui tree view system
- **Tree Theme** - Visual theme configuration for background, selection, expand buttons, and column resizers
- **Drag & Drop** - Drag data encapsulation for virtual and path tree views
- **Template Interface** - Rendering template interface for tree view, header, row, and edit modes

### Property Editing System

- **Property Grid Interface** - Object property inspection and editing UI
- **Property Target** - Abstract property target managing hierarchy, expansion, styling, read/write, and field operations
- **Array Target** - Array-based property editing with element management and array operations
- **Array Handler** - Generic and non-generic array collection operations
- **Editor Templates** - UI templates for boolean, string, numeric, enum, color, datetime, and selector editors
- **SValue Editor Templates** - Specialized editors for SKey, SAssetKey, SEnum, SBoolean, SString, and other SValue types
- **Provider System** - Extensible provider interface for custom populate, row, and editor functions
- **Property Grid Theme** - CSS class constants and theme helper methods
- **Action Setter** - Value operation extensions for set, array add/remove/modify, and clone/swap
- **Property Target Utility** - Tools for creating and manipulating property targets with path filling and preview conversion
- **Property Editor Base** - Abstract base classes for property editors and property fields with grouped editor support
- **Row Drawer** - Abstract base for property row drawing
- **Axis Editor** - Numeric input field axis style configuration

### Flowchart Rendering

- **Node Graph Extensions** - Flowchart node rendering operations including node frames, connectors, titles, and blink effects
- **Node Graph External** - External abstract class for node graph rendering operations
- **Canvas Switchable Node** - Mode-switchable canvas node supporting data transfer and knowledge base modes

### Log Console

- **Logging Model** - Log tree model with ImGui support, inheriting from MonitorPathTreeModel
- **Console Model** - Runtime log console model with log level filtering and log node rendering configuration
- **Console Menus** - Console view menu commands including copy log item functionality

### Document Management

- **Article View Collection** - Article view item collection with article management, content access, and XML tag generation
- **Article View Item** - Article field view item with content access, GUI interaction, and undo/redo support

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                 Editor ImGui Components                   │
│  ┌──────────┬──────────────┬──────────────────────┐     │
│  │Tree Views│Property Grid │  Flowchart Rendering │     │
│  │(Multi-   │(Multi-Object │  (Node Frames,       │     │
│  │ Column,  │ Editing,     │   Connectors,        │     │
│  │ Path)    │ Templates)   │   Blink Effects)     │     │
│  └──────────┴──────────────┴──────────────────────┘     │
├─────────────────────────────────────────────────────────┤
│              Log Console & Document Views                │
├─────────────────────────────────────────────────────────┤
│              External Abstraction Layer                  │
│         (Decoupled from specific implementations)        │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library
- **Suity.DataSync** - Data synchronization and serialization
- **Suity.Graphics** - Graphics abstraction layer
- **Suity.ImGui** - ImGui framework
- **Suity.ImGui.BuildIn** - ImGui built-in implementations
- **Suity.Editor** - Editor framework interfaces

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
