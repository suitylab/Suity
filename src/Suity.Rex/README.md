# Suity.Rex

Suity.Rex is a reactive data management and dependency injection framework built on a virtual DOM-like tree structure. It provides hierarchical data storage with path-based navigation, computed properties, change notifications, and a full DI container supporting multiple resolution patterns.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.Rex introduces a reactive programming paradigm centered around a tree-based data structure (`RexTree`/`RexNode`). Through a path system (`RexPath`), developers can precisely locate and observe data changes anywhere in the tree hierarchy. The framework combines reactive state management with a flexible dependency injection container, enabling clean separation of concerns and testable architecture.

## Features

### Reactive Data Tree

- **RexTree** - Core hierarchical tree structure managing nodes with data storage, action dispatch, listener management, and path mapping
- **RexNode** - Tree node class containing data, children, computed data, and listeners with before/after notification mechanisms
- **Path Navigation** - Dot-separated path syntax (e.g., `foo.bar[0].baz`) for precise node location and manipulation
- **Computed Properties** - Lazy-evaluated computed data with getter/setter support and automatic invalidation

### Dependency Injection Container

- **RexMapper** - Main mapper class managing type registration, resolution, and dependency injection
- **Multiple Resolution Patterns** - Support for Handler, Producer, Recycler, Assembler, Reducer, and Mediator patterns
- **Singleton & Transient** - Flexible lifetime management for resolved instances
- **External Service Integration** - Support for external service provider integration

### Reactive Programming Operators

- **Filtering** - `Where`, `NotNull`, `OfType` operators for value filtering
- **Transformation** - `Select`, `SelectMany`, `SelectIf`, `Format` operators for value transformation
- **Combination** - `Combine`, `And`, `Or` operators for merging multiple reactive sources
- **Conditional** - `If`, `IfHasValue` operators for conditional execution
- **Collection** - `First`, `FirstOrDefault`, `Each`, `Take`, `Skip` operators for enumerable processing
- **Side Effects** - `MapUpdateTo`, `MapActionTo`, `SetDataTo` operators for reactive side effects
- **Queueing** - `Queued` operator for deferred processing of reactive emissions

### Event System

- **RexEvent** - Generic event system supporting 0 to 4 parameters
- **Event Listeners** - Event-to-listener adaptation with operator chain support
- **Value System** - `RexValue` and `RexReadonlyValue` with change notification to listeners

### Infrastructure

- **Action System** - Action argument packaging (0-4 parameters) with listener infrastructure
- **Disposable Management** - `RexDisposeCollector` for batch resource management
- **Concurrent Pool** - Thread-safe object pooling for high-performance scenarios

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                      RexTree                             │
│              (Hierarchical Data Structure)               │
├─────────────────────────────────────────────────────────┤
│                      RexNode                             │
│         (Data, Children, Computed, Listeners)            │
├──────────────┬──────────────┬───────────────────────────┤
│   RexPath    │  RexMapper   │    RexOperators           │
│ (Navigation) │     (DI)     │  (Select, Where, etc.)    │
├──────────────┴──────────────┴───────────────────────────┤
│                   RexEvent / RexValue                    │
│              (Event & Reactive Value System)             │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library for base utilities and collections

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
