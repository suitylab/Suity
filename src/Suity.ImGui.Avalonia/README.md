# Suity.ImGui.Avalonia

Suity.ImGui.Avalonia is a cross-platform graphics rendering library that integrates the Suity.ImGui framework with Avalonia UI. Powered by SkiaSharp for high-performance 2D drawing, it provides double/single buffering, partial repaint optimization, rich text rendering, menu systems, drag-drop operations, and color pickers for seamless cross-platform deployment.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.ImGui.Avalonia bridges the Suity.ImGui immediate-mode GUI framework with the Avalonia UI cross-platform framework. It provides Avalonia-specific implementations of graphics contexts, input/output handling, and UI controls, enabling Suity.ImGui-based applications to run on Windows, Linux, and macOS with consistent rendering quality and performance.

## Features

### Core Controls

- **AvaImGuiControl** - Avalonia control for rendering ImGui UI with theme and background color configuration
- **AvaSKGraphicControl** - Double-buffered SkiaSharp rendering surface with complete input handling
- **AvaSKControl** - Base Avalonia control for SkiaSharp rendering through custom draw operations
- **AvaSKCompositionControl** - Efficient SkiaSharp rendering using composition layers

### Graphics Context

- **Double Buffer Context** - Off-screen rendering with partial repaint optimization for improved performance
- **Single Buffer Context** - Direct rendering to target canvas for simpler use cases

### Input & Output

- **AvaGraphicInput** - Avalonia-specific graphic input handling mouse, keyboard, and drag-drop events
- **AvaGraphicOutput** - Avalonia-specific graphic output providing primitive drawing, text rendering, and snapshot capabilities

### Editing Controls

- **TextBox Overlay** - Overlay text editing with single/multi-line, password mode, and auto-width adjustment
- **DropDown Adorner** - Decorator-based dropdown editing with precise positioning
- **DropDown Popup** - Context menu-based dropdown editing
- **Color Picker** - Color selection using Avalonia Flyout and ColorView

### Menu System

- **Menu Binders** - Binds root menu commands to Avalonia Menu, ContextMenu, and MenuFlyout controls
- **Menu Item View** - Avalonia menu item view with icon, shortcut key, and submenu support

### Drag & Drop

- **AvaDragEvent** - Avalonia-specific drag-drop event handling with effect control and internal data transfer

### Drawing Utilities

- **Rich Text Drawing** - TextLayout caching and dynamic color rendering support
- **SkiaSharp Extensions** - System.Drawing to SkiaSharp type conversion with font and image caching
- **SkiaSharp Utils** - Point, rectangle, size, color, and image type conversion utilities

### Helper Utilities

- **Conversion Helper** - Type conversion between Avalonia, System.Drawing, and SkiaSharp
- **ID Generator** - Random string identifier generation
- **LRU Cache** - High-performance Least Recently Used cache with capacity management and release callbacks

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  Avalonia UI Framework                    │
├─────────────────────────────────────────────────────────┤
│               Suity.ImGui.Avalonia                        │
│  ┌────────────┬─────────────┬──────────────────────┐    │
│  │ Controls   │  Graphics   │    Edit Controls      │    │
│  │ (ImGui,    │  Context    │   (TextBox, DropDown, │    │
│  │  Skia)     │  (Input/    │    ColorPicker)       │    │
│  │            │   Output)   │                       │    │
│  └────────────┴─────────────┴──────────────────────┘    │
├─────────────────────────────────────────────────────────┤
│                    SkiaSharp                             │
│              (2D Graphics Rendering)                     │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library
- **Suity.Graphics** - Graphics abstraction layer
- **Suity.ImGui** - ImGui framework
- **Avalonia UI** - Cross-platform UI framework
- **SkiaSharp** - 2D graphics rendering library

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
