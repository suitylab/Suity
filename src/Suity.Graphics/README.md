# Suity.Graphics

Suity.Graphics is a foundational UI rendering and interaction abstraction layer providing platform-independent interfaces for graphics rendering, color configuration, context management, drawing operations, drag-and-drop, and hierarchical menu command systems.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.Graphics serves as the graphics abstraction layer in the Suity ecosystem, defining the core interfaces and contracts for UI rendering and interaction. By providing platform-independent abstractions, it enables the upper-level UI frameworks (such as Suity.ImGui) to operate without direct dependencies on specific rendering backends.

## Features

### Graphics Context & I/O

- **IGraphicContext** - Graphics device context interface managing input/output, draw regions, and refresh requests
- **IGraphicInput** - Graphics input interface encapsulating mouse, keyboard, and drag-drop events with predefined input instances
- **IGraphicOutput** - Graphics output interface providing rendering operations for lines, rectangles, ellipses, text, and images
- **IGraphicObject** - Object interface for handling both graphic input and output operations

### Color Configuration

- **IColorConfig** - Color configuration interface with default implementation providing predefined UI theme color schemes
- **ColorHelper** - Color manipulation and conversion utilities including interpolation, opacity adjustment, and HTML color parsing

### Menu Command System

- **MenuCommand** - Abstract base class for menu commands with sub-command management, type acceptance, selector application, and view binding
- **RootMenuCommand** - Top-level menu container with static creation methods
- **MainMenuCommand** - Main menu bar with automatic expanded state checking
- **SimpleMenuCommand** - Simple menu command implementation with action callback and popup state checking
- **MenuSeparator** - Menu separator with visibility control
- **IMenuItemView** - Menu item view interface defining display properties and behavior with empty implementation

### Drag & Drop

- **IDragEvent** - Drag-drop event interface and data class with effect control and data format definitions
- **IDropTarget** - Interface for receiving drag-drop operations with `DragOver` and `DragDrop` methods

### Extension Interfaces

- **Context Menu** - Interface for context menu display and management
- **Custom Controls** - Interface for custom control integration
- **Drop-down Edit** - Interface for drop-down editing functionality
- **Text Box Edit** - Interface for text box editing
- **Tool Tips** - Interface for tooltip display
- **Color Picker** - Interface for color selection

### Utilities

- **MathHelper** - Mathematical operations and geometric calculations including clamping, linear interpolation, rectangle scaling and offsetting
- **CommonTypeHelper** - Utility for finding common base types in type collections
- **DerivedTypeHelper** - Derived type caching utility for retrieving all derived types by base type
- **StringExtensions** - String operation extensions including formatting, prefix/suffix removal, and length limiting

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   IGraphicContext                        │
│              (Graphics Device Context)                   │
├───────────────────────────┬─────────────────────────────┤
│      IGraphicInput        │       IGraphicOutput        │
│   (Mouse/Keyboard/Drag)   │   (Lines/Rects/Text/Image)  │
├───────────────────────────┴─────────────────────────────┤
│                    IColorConfig                          │
│              (Theme Color Schemes)                       │
├─────────────────────────────────────────────────────────┤
│                  Menu Command System                     │
│     (Root -> Main -> Commands -> Separators)             │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library for base utilities and type reflection

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
