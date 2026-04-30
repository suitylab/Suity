# Suity.Editor.Packaging

Suity.Editor.Packaging is the package management plugin module for the Suity editor, providing complete resource package export and import functionality. It supports two package formats: standard Suity packages (`.suitypackage`) and library packages (`.suitylibrary`).

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.Editor.Packaging enables sharing and distribution of editor assets through a native package system. The module handles both export (collecting dependencies, GUID replacement, manifest generation) and import (batch loading, global ID resolution, workspace auto-creation, metadata tracking) operations through an ImGui-based preview interface with file selection and status indicators.

## Features

### Export System

- **Package Exporter** - Core export engine packaging project resource files and workspace files into `.suitypackage` or `.suitylibrary` ZIP archives
- **Dependency Collection** - Automatic collection of dependent files during export
- **GUID Replacement** - Reverse GUID replacement for render files during export
- **Manifest Generation** - XML manifest generation with MD5 checksums for library packages
- **Batch Export** - Multi-file batch export with document format-based ordering

### Import System

- **Package Importer** - Core import engine extracting ZIP archives and importing files into the project
- **Batch Loading** - Document format dependency-based batch loading
- **Global ID Resolution** - Forward global ID resolution for render files
- **Workspace Management** - Automatic workspace creation and update after import
- **Metadata Tracking** - Post-import metadata tracking

### Preview Interface

- **Package Preview** - ImGui-based tree preview dialog with file selection checkboxes
- **Directory Nodes** - Directory nodes with hierarchical enable state propagation
- **File Nodes** - File nodes with status icons and text tooltips
- **Status Indicators** - Error, warning, render target, and duplicate file status indicators
- **Keyboard Navigation** - Space key shortcut for toggling selected items

### Data Models

- **Render File** - Render file metadata
- **WorkSpace File** - Workspace file entries
- **Package Direction** - Operation direction enumeration (export/import)
- **Package Types** - Package type enumeration (SuityPackage/SuityLibrary)
- **File Locations** - File location enumeration (assets/workspaces)

### Plugin Entry

- **PackagerPlugin** - Editor plugin entry implementing `IPackageExport` and `IPackageImport` service interfaces for export and import dispatch

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   PackagerPlugin                         │
│         (IPackageExport / IPackageImport)                │
├───────────────────────────┬─────────────────────────────┤
│    Package Exporter       │     Package Importer        │
│  (ZIP Archive, GUID       │   (ZIP Extract, ID          │
│   Replace, Manifest)      │    Resolve, Workspace)      │
├───────────────────────────┴─────────────────────────────┤
│                 Package Preview ImGui                    │
│     (Tree View, Checkboxes, Status Indicators)           │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library
- **Suity.Editor** - Editor framework interfaces
- **Suity.ImGui** - ImGui framework
- **Suity.ImGui.BuildIn** - ImGui built-in implementations

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
