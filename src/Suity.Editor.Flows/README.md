# Suity.Editor.Flows

Suity.Editor.Flows is a visual flowchart editing and execution framework based on ImGui node graph architecture. It supports data flow and action flow execution modes, sync/async dual-mode computation engine, a rich built-in node library, undo/redo, sub-flow nesting, clipboard operations, and a type-safe connector system.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.Editor.Flows provides a complete visual programming experience through its node graph architecture. The framework supports two execution paradigms: data flow (values propagate through connected nodes) and action flow (control flows through action chains). The computation engine operates in both synchronous and asynchronous modes, enabling flexible workflow execution for game logic orchestration, AI dialogue flow design, data processing pipelines, and other visual programming scenarios.

## Features

### Core Engine

- **Flow Computation** - Synchronous computation engine handling node data caching, dependency resolution, and value propagation
- **Async Flow Computation** - Asynchronous computation engine supporting action node chain execution, cancellation tokens, and multi-level context management
- **Flow Document Diagram** - Diagram implementation encapsulating node and connection CRUD with view notification
- **Link Collection** - Link collection managing inter-node connections with data sync support
- **Connector Alias Manager** - Handling legacy name mapping and connection repair
- **Flow Plugin** - Plugin entry point initializing flowchart external services

### GUI Views

- **Flow View Base** - ImGui-based flowchart view with node visualization, selection management, clipboard, and drag-drop
- **Flow Document View** - Flow document view with undo/redo, sub-document navigation, and ImGui graph rendering
- **Undoable Flow View** - Flow view extension with undo/redo support
- **Node Views** - ImGui rendering for normal nodes (with expand panels and custom GUI), group nodes, and comment nodes
- **Sub-Flow View** - Sub-flowchart view rendering within expanded node regions
- **Sub-Tree View** - Tree expansion view for hierarchical node structure display
- **Sub-Document View Stack** - Multi-level document navigation and save management
- **View State Persistence** - Saving viewport position, zoom, selection, and expand state
- **Type Manager** - Caching and resolving connector data types
- **Data Types** - Action, event, unknown, and custom diagram data types with visual styles

### Undo/Redo Actions

- **Create/Delete Node** - Undoable node creation and deletion
- **Move Node** - Undoable node position changes
- **Resize Node** - Undoable node size adjustments
- **Create/Remove Link** - Undoable connection creation and deletion
- **Smart Selection** - Undoable selection change operations

### Built-in Node Library

- **Value & Input** - Input value nodes (PassValue, CloneObject, Null) and type conversion nodes
- **Math Operations** - Add, subtract, multiply, divide, power, negate, absolute, round, floor, ceil, min, max, PI constant, clamp, and linear interpolation
- **Logic Control** - Boolean condition-based value branching (BooleanSwitch)
- **String Processing** - Text input, text reference, paginated text reference, replace, escape, merge, split, whitespace check, and object-to-string
- **String Operations** - Numeric part trimming, regex replacement, empty string check, and whitespace string check
- **Variables** - Temporary variable get, set, and batch set during chart computation
- **Action & Flow Control** - Return, Foreach (array/JSON traversal), While/DoWhile loops, delay, throw error, multi-action execution, data action combine/decompose, type branch, enum branch, If condition, and cached value
- **LINQ Operations** - Select projection, Concat, Union (deduplicated union), and FirstOrDefault
- **SValue Data Operations** - SObject creation, property read/write, array element get, data table/data row operations, and data links
- **JSON Processing** - JSON extraction from text, type conversion, field get, JSON-to-text, and JSON-to-SObject parsing
- **Dialog Nodes** - OK dialog, Yes/No dialog, message output, copy message, and manual text input

### Clipboard & Menus

- **Clipboard Data** - Copy/paste data structure storing diagram items and connections
- **Context Menus** - Create, clone, delete, jump, and other operations

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   Flow Document                           │
│              (Nodes, Links, Diagram)                     │
├──────────────┬──────────────────────────────────────────┤
│   Sync       │          Async Computation               │
│ Computation  │          Engine                          │
├──────────────┴──────────────────────────────────────────┤
│                    GUI Views                             │
│    (Node Rendering, Selection, Clipboard, Sub-Flows)     │
├─────────────────────────────────────────────────────────┤
│                 Built-in Node Library                    │
│  (Math, String, Logic, Loop, JSON, SValue, Dialog, etc.) │
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

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
