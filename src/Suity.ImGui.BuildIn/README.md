# Suity.ImGui.BuildIn

Suity.ImGui.BuildIn is the built-in implementation library for the Suity.ImGui framework, providing complete backend implementations for the input, layout, fit, and render systems, along with professional-grade UI components including property grids, tree views, and node graph editors.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.ImGui.BuildIn provides the concrete implementations that power the Suity.ImGui framework. It includes backend implementations for all four core systems (Input, Layout, Fit, Render), as well as a comprehensive set of UI components designed for professional editor applications. The library follows a layered architecture with clear separation between core framework implementations and higher-level UI components.

## Features

### Core Framework Implementation

- **ImGuiBK** - Core ImGui implementation managing node tree, input processing, layout, and rendering
- **System Backends** - Complete backend implementations for input (`ImGuiInputSystemBK`), layout (`ImGuiLayoutSystemBK`), fit (`ImGuiFitSystemBK`), and render (`ImGuiRenderSystemBK`) systems
- **Node Backend** - Low-level node implementation handling lifecycle and content management
- **Factory System** - Abstract factory for creating and recycling ImGui nodes
- **Style Collection** - Named style sets with raw values, pseudo-class values, and transitions
- **Value Collection** - GUI value cache collection with typed value storage and inheritance

### Animation System

- **EaseAnimation** - Base easing animation class with time interpolation and multiple easing functions
- **ScrollAnimation** - Smooth scrolling animation to target position
- **ExpandAnimation** - Smooth expand/collapse animation for nodes
- **Value Source Animation** - Easing animation based on value sources

### Tree View Components

- **Virtual Tree View** - High-performance virtual tree view with selection, clipboard, comments, and preview path support
- **Column Tree View** - Multi-column virtual tree view with name, description, and preview columns
- **Headerless Tree View** - Tree view without header row
- **Tree Model** - Tree data management with node operations
- **Drag & Drop** - Drag data transfer objects for tree operations
- **Menu Commands** - Tree view menu command definitions

### Property Grid System

- **Property Grid** - Full-featured property grid for inspecting and editing object properties
- **Editor Templates** - Built-in editors for boolean, string, numeric, enum, color, datetime, and selector types
- **Multi-Object Editing** - Support for editing multiple objects simultaneously
- **Undo/Redo Integration** - Property changes integrated with undo/redo system
- **Custom Editor Support** - Extensible editor provider system for custom type editors
- **Property Targets** - Root, column, converted, and array property target implementations

### Node Graph / Flowchart Editor

- **Graph Control** - Main node graph rendering and interaction control
- **Graph Node** - Base graph node with position, size, connectors, and rendering
- **Group/Comment Nodes** - Grouping and annotation node types
- **Viewport** - Viewport management with pan and zoom operations
- **Selection/Link Managers** - Node selection and link operation management
- **Theme System** - Complete theme definitions for nodes, connectors, and UI elements
- **Sub-Property Grid** - Embedded property grid for node properties

### Style & Theme System

- **Node Styles** - Node-specific style collection with parent style and theme inheritance
- **Theme Backend** - Theme configuration and style management
- **Function Chain** - Interface for chaining input, layout, fit, and render functions

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    ImGuiBK                               │
│              (Core Implementation)                       │
├──────┬──────────┬──────────┬────────────────────────────┤
│Input │  Layout  │   Fit    │         Render             │
│ BK   │   BK     │   BK     │         BK                 │
├──────┴──────────┴──────────┴────────────────────────────┤
│                  UI Components                           │
│  ┌──────────┬──────────────┬──────────────────────┐     │
│  │Tree Views│Property Grid │   Node Graph Editor  │     │
│  └──────────┴──────────────┴──────────────────────┘     │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library
- **Suity.Graphics** - Graphics abstraction layer
- **Suity.ImGui** - ImGui framework interfaces and base classes

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
