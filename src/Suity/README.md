# Suity

Suity is a comprehensive C# foundational framework library that provides a robust infrastructure for application development. It offers a wide range of core utilities, collection extensions, reflection helpers, logging systems, networking support, localization, conversation systems, and more, serving as the base layer for all other Suity components.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity serves as the foundational layer of the Suity ecosystem, providing essential building blocks for enterprise-grade C# applications. The framework is designed with a focus on performance, thread safety, and extensibility, making it suitable for medium to large-scale applications.

At its core, Suity provides a unified object lifecycle management system through the `Object` and `Device` abstractions, enabling consistent object creation, destruction, and environment abstraction across the entire framework.

## Features

### Object Lifecycle Management

- **Object Base Class** - Unified base class with lifecycle management (`OnDestroy`), name management, and static destruction methods
- **Device Abstraction** - Abstract device layer providing object creation, action queues, logging, and service resolution
- **SystemObject** - Startable/stoppable object base class for system-level components

### High-Performance Collections & Data Structures

- **Object Pool** - Stack-based object pooling (`Pool`) and thread-safe concurrent pooling (`ConcurrentPool`) for reduced GC pressure
- **CappedArray** - Fixed-size circular array with head/tail insertion and automatic cursor cycling
- **WordTree** - Trie-based data structure for efficient word matching and sensitive word filtering
- **UniqueMultiDictionary** - Multi-value dictionary allowing one key to map to multiple unique values
- **RangeCollection** - Range-based collection managing groups with high/low boundaries and binary search support
- **ActionQueue** - Thread-safe action queue with batch execution and exception handling

### Reflection & Type System

- **Type Resolution** - String-based type parsing supporting generics, arrays, nested types, and complex type expressions with caching
- **Type Description** - Type name generation, member ID resolution, and numeric type detection
- **Derived Type Discovery** - Assembly scanning for derived type discovery with caching and exclusion support
- **Instantiation Helpers** - Expression tree-based constructor caching and uninitialized object creation

### Logging & Monitoring

- **Multi-Level Logging** - Runtime, network, resource, entity, and operation logging interfaces with empty object implementations
- **Exception Logging** - Extension methods for direct exception logging with structured log item types
- **Action Logging** - Operation tracking with structured log items for debugging and auditing

### Networking Abstractions

- **Network Direction** - Traffic direction enumeration (none, upload, download)
- **Delivery Methods** - Multiple transmission modes including unreliable, reliable, ordered, and sequenced
- **Network Logging** - Network event logging interface for monitoring network activity

### Localization & Internationalization

- **ILocalizer Interface** - Localization interface with global static access
- **String Localization** - Support for plain strings, interpolated strings, and context-aware localization

### Conversation System

- **Conversation Framework** - Flexible conversation system with roles, message types, and button interactions
- **Async Input Support** - Asynchronous text and button input waiting for interactive scenarios
- **Message Types** - System, user, debug, warning, and error message support

### Utility Libraries

- **Random Number Generation** - WELL algorithm-based high-quality random number generator with seeding support
- **String Operations** - Prefix/suffix removal, length limiting, split finding, and escape/unescape utilities
- **Stream Operations** - Stream-to-byte-array conversion, file I/O, stream copying, and UTF-8 text conversion
- **Byte Operations** - UTF-8 encoding/decoding, Base64 conversion, hex conversion, and array concatenation
- **Path & File Utilities** - Relative/absolute path conversion, path normalization, URL拼接, and file/directory operations
- **Time Synchronization** - Remote UTC time synchronization with latency compensation and remaining time calculation

### Value Storage & Services

- **ValueStore** - Generic value management with get/set/pick/swap operations and value transformation
- **ServiceStore** - Lazy-loaded service resolution with deferred service acquisition

### Metadata & Attributes

- **Display Attributes** - Display text, preview text, tooltips, category, and ordering attributes
- **Thread Safety Markers** - Multi-threading safety mode enumeration for lock-safe, concurrent-safe, per-thread-safe, read-only, single-thread-restricted, and unsafe patterns
- **Type Descriptors** - Type and field metadata descriptors for runtime type information

## Architecture

Suity follows a modular, interface-driven architecture:

```
┌─────────────────────────────────────────────────────────┐
│                      Environment                         │
│              (Global Static Entry Point)                 │
├─────────────────────────────────────────────────────────┤
│                        Device                            │
│          (Runtime Abstraction Layer)                     │
├──────────┬──────────┬──────────┬──────────┬─────────────┤
│  Object  │  Logging │Collections│Reflection│ Networking  │
│ Lifecycle│  System  │ & Data   │ & Type   │  Abstraction│
│          │          │Structure │  System  │             │
├──────────┴──────────┴──────────┴──────────┴─────────────┤
│                   Utility Libraries                      │
│         (Random, String, Stream, Path, Time)             │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

Suity is designed to be self-contained with minimal external dependencies, making it suitable as a foundational library for any .NET project.

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
