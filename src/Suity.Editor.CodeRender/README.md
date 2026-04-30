# Suity.Editor.CodeRender

Suity.Editor.CodeRender is a code rendering and generation library featuring JavaScript parsing (full ECMAScript lexer and parser to AST), AST-based code rendering for multiple target languages, an expression system converting editor objects to expression nodes, a dynamic proxy template system, code segment parsing with user code protection, and a LiteDB-based encrypted local code database.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.Editor.CodeRender provides a complete code generation solution for the Suity editor. It includes a built-in JavaScript parser that produces AST (Abstract Syntax Tree), an AST-based code rendering system supporting multiple target languages, a dynamic proxy mechanism for template code generation, and a segment-based code replacement system that protects user-written code during incremental updates.

## Features

### JavaScript Parser

- **Full ECMAScript Support** - Complete lexer and parser supporting all standard ECMAScript syntax structures
- **Token Types** - Keyword recognition, identifier scanning, numeric/string/regex literal parsing, and comment handling
- **AST Nodes** - Comprehensive AST node definitions including expressions, statements, declarations, and program structure
- **Expression Nodes** - Array, assignment, binary, conditional, call, member, logical, unary, update, object, property, and literal expressions
- **Statement Nodes** - Block, break, continue, do-while, for, for-in, if, labeled, return, switch, throw, try, while, with, expression, empty, and debugger statements
- **Declaration Nodes** - Function declaration/expression and variable declaration/declarator
- **Position Tracking** - Source location tracking with line/column information for error reporting

### AST Code Rendering

- **RenderLanguage** - Abstract base class for rendering AST nodes to specific target languages
- **Syntax Support** - Built-in ECMA standard operator and syntax handling for assignment, arrays, block statements, binary expressions, call expressions, conditional expressions, loops, and function declarations
- **Type Resolution** - AST type resolver handling type resolution in AST nodes
- **Class Nodes** - Class-related AST nodes including class fields, methods, and native code
- **Extension Nodes** - Extension-related AST nodes

### Expression System

- **Expression Factory** - Base factory building expression nodes from objects with type caching and generic factory support
- **Expression Services** - Dedicated expression services for editor objects, components, data, DTypes, formatters, functions, initial values, and controllers
- **Expression Render Target** - Handling final expression output

### Dynamic Proxy Template System

- **DynamicProxy** - Base class capturing dynamic member access patterns into code string expressions
- **RenderProxy** - Base proxy class providing tools for wrapping editor values, resolving type names, and managing namespace context
- **SObject/SArray Proxy** - Dynamic access to editor object and array values
- **AssetId Proxy** - Dynamic access to resource identifiers
- **Render Target Proxy** - Dynamic access to render targets
- **Type Definition Proxy** - Dynamic access to type definition information
- **Error Proxy** - Error tracking during code generation

### Code Replacement & Segments

- **Segment Parser** - Parsing code strings into SegmentNode tree structures using markers (`//{#` and `//}#`)
- **Segment Document** - Segment collection for incremental storage and query
- **Segment Nodes** - Tree structure node types including root, label, and text nodes
- **Replacement Header** - Metadata storage for replacement operations

### User Code Database

- **LiteCodeLibraryDB** - LiteDB-based local code database with encrypted access
- **Code Tags** - User code metadata and content storage
- **Code Tag Collection** - Managing related code tag groups
- **Operations** - Storage, query, incremental update, and rename operations

### Code Rendering Core

- **Code Binder** - Namespace resolution and type binding
- **Code Block** - Manipulable code block with indentation support and string conversion
- **Render Results** - Code rendering result data structures
- **Render Targets** - Code rendering target classes and interfaces
- **Replacement** - Core code replacement logic
- **Code Template Extensions** - Extension methods for creating code templates
- **Default Materials** - Default material-related code rendering configuration

### Utilities

- **WordTreeEx** - Extended word tree for efficient multi-pattern string matching and lexical analysis with generic type markers
- **Type Extensions** - Type string formatting, conversion, and acquisition utilities

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  Code Generation                          │
│  ┌──────────┬──────────────┬──────────────────────┐     │
│  │JavaScript│  AST Code    │   Dynamic Proxy      │     │
│  │Parser    │  Rendering   │   Template System    │     │
│  │(ECMAScript)│(Multi-     │   (SObject, SArray,  │     │
│  │          │  Language)   │    TypeDef Proxy)    │     │
│  └──────────┴──────────────┴──────────────────────┘     │
├─────────────────────────────────────────────────────────┤
│              Code Replacement & User Code DB             │
│     (Segment Parsing, LiteDB Storage, Protection)        │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library
- **Suity.DataSync** - Data synchronization and serialization
- **Suity.Editor** - Editor framework interfaces
- **LiteDB** - Embedded NoSQL database for code storage
- **Newtonsoft.Json** - JSON serialization
- **Pathoschild.Http.FluentClient** - HTTP client for network operations

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
