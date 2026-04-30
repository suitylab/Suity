# Suity.Editor.VirtualTree

Suity.Editor.VirtualTree is a flexible, extensible virtual tree component library for property tree editing. It features an adapter-based node system mapping various data sources to virtual tree nodes, priority-driven node creation, complete undo/redo support, and ImGui rendering customization.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.Editor.VirtualTree provides a virtual node architecture for building property tree editors. Through `VirtualTreeModel` and `VirtualNode`, it manages tree display and interaction while decoupling data sources from presentation through an adapter pattern. The system automatically selects appropriate node types based on target types and context, enabling extensible node creation through user-defined providers.

## Features

### Core Model

- **VirtualTreeModel** - Core tree structure managing nodes, display objects, interaction events, preview paths, and undo/redo transactions
- **VirtualNode** - Abstract base class defining tree structure, lifecycle management, value get/set, expand state, and context menus
- **Node Collection** - Partial class implementing child node collection with add/remove/modify and index management
- **Node Display** - Partial class handling text, icon, color, status, and preview text updates
- **Node ImGui** - Partial class implementing ImGui rendering interface with analysis result visualization
- **Setter Action** - Abstract operation for setting virtual node values with undo/redo support

### Node Types

- **RootNode** - Tree root node as top-level container with default context menu behavior
- **EmptyNode** - Internal placeholder node for fallback or marker purposes
- **SimpleTextNode** - Static text node without value binding for fixed text display
- **StringNode** - String value node with editable preview text
- **ToStringNode** - Object string representation node via `ToString()` with text editing support
- **BaseObjectNode** - Abstract base for objects with properties, providing property display, editing, and sync infrastructure
- **SyncObjectNode** - Syncable object node with view object setup, value setting, and advanced editing (JSON serialization)
- **ListVirtualNode** - Generic list node based on `ListAdapter` with item management, drag-drop, and GUI creation
- **IListVirtualNode** - Collection node based on `IList` interface with element type resolution

### SValue Nodes

- **SObjectNode** - `SObject` value node with property display, editing, and XML/JSON serialization
- **SArrayNode** - `SArray` value node with list add/remove/modify and drag-drop operations
- **Advanced Edit Helper** - Extension methods for advanced editing operations on SItem-backed nodes
- **Setter Actions** - Undo/redo operations for SArrayNode including value set, insert, and delete

### Adapter System

- **VirtualNodeAdapter** - Abstract base for node adapters providing value access, parent query, service resolution, and display properties
- **ListAdapter** - Abstract base for list adapters defining item count, get/set, insert, delete, and GUI creation
- **IListAdapter** - `IList` list adapter base with element type resolution and new item creation
- **ViewNodeAdapter** - List adapter for `IViewNode` interface with sync-based list operations
- **ViewListAdapter** - List adapter for `IViewList` and `ISyncList` interfaces with sync-based operations

### Undo/Redo Actions

- **Comment Action** - Reversible operation for setting/clearing comment state on view objects
- **Object JSON Setter** - Reversible operation parsing JSON data into target object
- **Object Setter Actions** - Reversible operations for object property setting including sync and SObject variants
- **List Setter Actions** - Reversible operations for list nodes including edit, insert, and delete
- **IList Setter Actions** - Reversible operations for basic IList nodes including value set, insert, and delete

### Configuration & State

- **Node Provider** - `IVirtualNodeProvider` interface and `ProviderContext` for type-based and priority-driven node creation
- **Usage Attribute** - `VirtualNodeUsageAttribute` for matching node types to target edit types with static and dynamic priority
- **Tree Asset Config** - Single tree asset configuration with selected node path, expand state, preview preset, and custom user data
- **Expand State** - Virtual node expand state backup and restoration with path-level persistence
- **Virtual Path** - Tree path representation composed of property name segments with comparison and batch creation
- **Operation State** - Enumeration for virtual node operation results (success, multi-select, unsupported, null reference, creation failed)

### Event System

- **Context Menu Events** - Context menu request and menu action handling
- **Selection Events** - Selection change notifications
- **Node Events** - Node-specific event notifications
- **Value Edit Events** - Value editing and list editing event parameters

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   VirtualTreeModel                       │
│              (Tree Management & Interaction)             │
├─────────────────────────────────────────────────────────┤
│                     VirtualNode                          │
│         (Display, Value, Collection, ImGui)              │
├──────────────┬──────────────┬───────────────────────────┤
│   Adapters   │  Node Types  │    Setter Actions         │
│ (Data Source │ (Object,     │   (Undo/Redo for          │
│  Mapping)    │  List, SValue│    Value Modifications)   │
├──────────────┴──────────────┴───────────────────────────┤
│              Provider System & State Persistence         │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library
- **Suity.DataSync** - Data synchronization and serialization
- **Suity.Editor** - Editor framework interfaces

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
