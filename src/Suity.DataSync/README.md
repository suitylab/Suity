# Suity.DataSync

Suity.DataSync is a comprehensive data synchronization and serialization library providing object data sync, serialization/deserialization, cloning, and traversal mechanisms. Built around a unified sync mode design, it supports serialization, cloning, view presentation, and object tree traversal through `ISyncObject` and `ISyncList` interfaces.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.DataSync provides a unified approach to data synchronization through its core `ISyncObject` and `ISyncList` interfaces. The sync mechanism operates with different modes (Get, Set, GetAll, SetAll, Insert, RemoveAt) and intents (Serialize, Clone, View, Visit), enabling a single implementation to serve multiple purposes. This design eliminates the need for separate serialization, cloning, and UI binding code.

## Features

### Binary Data I/O

- **BinaryDataReader/Writer** - Efficient binary reading and writing of primitive types
- **ByteArray** - Encapsulated byte array with combined read/write functionality
- **Endianness Support** - Little-endian and big-endian byte order converters

### JSON & BSON Processing

- **JSON Parser** - Lightweight JSON string parser producing `JsonObject` and `JsonArray`
- **JSON Serializer** - Bidirectional object-to-JSON conversion with formatting support
- **BSON Support** - BSON value types, objects, arrays, and simple BSON encoder/decoder
- **Data Readers/Writers** - JSON and BSON implementations of `IDataReader`/`IDataWriter` interfaces

### LZ4 Compression

- **LZ4 Codec** - Fast compression algorithm with multiple platform implementations
- **Safe/Unsafe Modes** - Platform-agnostic safe mode and pointer-optimized unsafe mode
- **32/64-bit Variants** - Architecture-specific implementations for optimal performance
- **Pickler** - Data packaging/unpackaging utility for compressed data

### Object Synchronization Framework

- **ISyncObject** - Interface for object property synchronization operations
- **ISyncList** - Interface for index-based list synchronization
- **Sync Modes** - Get, Set, GetAll, SetAll, Insert, RemoveAt, and more
- **Sync Intents** - Serialize, Clone, View, Visit for different operation purposes
- **Sync Flags** - AttributeMode, ByRef, NoSerialize, PathHidden for fine-grained control
- **Proxy System** - Abstract base classes for types without native sync interface support

### Serialization & Cloning

- **Serializer** - Object tree serialization based on `INodeWriter`/`INodeReader`
- **Cloner** - Deep copy through sync operations with complex object graph support
- **XML Serialization** - XML format serialization support

### Object Tree Traversal

- **Visitor Pattern** - Depth-first traversal with predicate/action execution
- **Path Context** - Path tracking during tree traversal for precise location
- **SyncPath** - Path chain composed of strings, integers, Guids, or Loids with composition and matching

### View System Integration

- **IViewObject** - View object interface extending `ISyncObject` with `SetupView` method
- **View Properties** - Name, description, icon, flags (expand, readonly, disabled), and styling
- **Edit Interfaces** - Text edit, preview edit, preview display, and list text display interfaces

### Type Resolution

- **SyncTypes** - Type registration and management for value type parsing and proxy registration
- **SyncTypeResolver** - Interface for resolving types by name with default and custom implementations
- **Validation** - `IValidate` interface and `Validator` implementation for data validation

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   Sync Operations                        │
│         (ISyncObject / ISyncList / ISyncContext)         │
├──────────────┬──────────────┬───────────────────────────┤
│  Serializer  │   Cloner     │     Visitor               │
│ (Serialize)  │  (Clone)     │   (Traverse)              │
├──────────────┴──────────────┴───────────────────────────┤
│              Data Format Handlers                        │
│    (Binary / JSON / BSON / XML / LZ4 Compression)        │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library for base utilities and `Loid` identifier

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
