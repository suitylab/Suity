# Suity.ImGui

Suity.ImGui is a proprietary immediate-mode GUI framework developed in pure C#, built on a node-tree architecture with four extensible systems (Input, Layout, Fit, Render). It enables rapid UI development through its immediate-mode paradigm with a rich widget library, CSS-like styling, and virtual rendering for high-performance scenarios.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.ImGui implements an immediate-mode GUI paradigm where UI code is written declaratively without managing complex view state. The framework is built around a node-tree architecture (`ImGui` and `ImGuiNode`) with four pluggable systems: Input, Layout, Fit, and Render. This design enables highly customizable UI pipelines while maintaining the simplicity and directness of immediate-mode programming.

## Features

### Core Framework

- **ImGui Abstract Class** - Core entry point managing node tree, input/output processing, style system, and value management
- **ImGuiNode** - Abstract base class for render nodes with transform, style, layout, rendering, and input properties
- **ImGuiPath** - Hierarchical path representation for nodes in the UI tree with composition and manipulation support
- **Node Flags** - State and behavior flags including initialization, render dirty, disabled, and float layout states
- **Node References** - Strong and weak reference wrapper with automatic cleanup and refresh requests

### Four-System Architecture

- **Input System** - Processes user input (mouse, keyboard) and dispatches to nodes
- **Layout System** - Calculates node positions and sizes based on layout rules
- **Fit System** - Adapts node dimensions to available space
- **Render System** - Renders nodes to the graphics output

### Widget Library

- **Buttons** - Normal, horizontal/vertical, expand, toggle, and dropdown buttons
- **Text Display** - Single-line text, initialization text, and multi-line text areas
- **Text Input** - String input, password input, multi-line text input, numeric input, and progress bars
- **Toggles** - Checkbox, advanced tri-state checkbox, and toggle buttons
- **Panels** - Normal panels and expandable panels with header styling and content callbacks
- **Resizers** - Horizontal/vertical resizers and group resizers with min/max position constraints
- **Scroll Containers** - Scrollable frames with scroll rate control, scrollbar positioning, and auto-scroll to bottom
- **Virtual Lists** - High-performance lists rendering only visible items with data source management
- **Tree Views** - Tree controls with data setting, node title rendering, and drag-drop support

### Styling System

- **CSS-like Selectors** - Type name, class name, and ID-based style targeting
- **Pseudo-states** - Hover, active, and other pseudo-state support
- **Transition Animations** - Animated property transitions with built-in easing functions
- **Style Classes** - Color, border, header, font, corner radius, image filter, progress bar, margin/padding, and alignment styles with interpolation support

### Animation System

- **Easing Functions** - Linear, quadratic, cubic, quartic, quintic, sinusoidal, exponential, and circular easing curves (EaseIn/EaseOut/EaseInOut)
- **Value Transitions** - Animated value interpolation with custom easing

### Advanced Features

- **Virtual Rendering** - On-demand rendering strategy instantiating only visible nodes for smooth performance with thousands of items
- **External Control Embedding** - Support for hosting native controls (e.g., WinForms) within ImGui
- **Viewport Zoom/Pan** - Canvas zoom and pan operations
- **Drag & Drop** - Full drag-drop support with data transfer
- **Context Menus** - Right-click menu support
- **Tree Data Model** - `VisualTreeData` and `VisualTreeNode` for tree view data with selection modes, drag-drop modes, and visitor patterns

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                        ImGui                             │
│              (Core Entry Point)                          │
├──────┬──────────┬──────────┬────────────────────────────┤
│Input │  Layout  │   Fit    │         Render             │
│System│  System  │  System  │         System             │
├──────┴──────────┴──────────┴────────────────────────────┤
│                   ImGuiNode Tree                         │
│         (Widgets, Styles, Animations)                    │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library for base utilities
- **Suity.Graphics** - Graphics abstraction layer for rendering interfaces

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
