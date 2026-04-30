# Suity.Editor.Documents

Suity.Editor.Documents is the document management module providing visual data structure design, refactoring tools, article management with Markdown import, external document parsing (Excel, Word, PDF), and AI prompt template documents.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.Editor.Documents provides a comprehensive document editing system for the Suity editor. It supports visual design of data structures (structs, enums, abstract types, events, logic modules), powerful refactoring operations (field extraction, structure folding), hierarchical article management, and parsing of external document formats. The module is a core component for data design and content management in the editor.

## Features

### Type Design System

- **Type Design Document** - Main class for designing data structures (structs, enums, abstract types, logic modules) with `.stype` file format
- **Type Design Plugin** - Plugin managing available design item types sorted by display order
- **Type Design Group** - Grouping container for organizing type items

### Struct Types

- **Struct Type** - Structure type with field list and brief description, supporting inheritance and value type settings
- **Struct Type Base** - Abstract base providing field management, inheritance support, and data row interface
- **Struct Field** - Field in struct or abstract type with type info, default value, optionality, and display options
- **Struct Field List** - Field list container supporting parameter and label item creation
- **Struct Field Item/Label** - Field list items with description support and sibling navigation

### Enum Types

- **Enum Type** - Enum type with item list and ID automation mode settings
- **Enum Item** - Single enum item with name, value, and description
- **Enum Item List** - Enum item field list supporting enum item and label creation

### Abstract & Event Types

- **Abstract Type** - Abstract struct type implementable by other structs
- **Event Type** - Event type for type design document event definitions
- **Event Argument Type** - Event parameter type definition
- **Class Function** - Function definition in type design documents with access and action modes
- **Logic Module** - Logic module type with component reference list

### Enum-to-Struct Generation

- **EnumToStruct Type** - Automatically generated struct type from enum with field type and value configuration
- **DEnumToStruct Asset** - Asset representation of enum-to-struct conversion
- **EnumToStruct Builder** - Builder for creating enum-to-struct conversions

### Refactoring Tools

- **Refactor Command** - Refactoring menu command entry aggregating extract/collapse struct and array commands
- **Base Refactor Action** - Base class for refactoring undo/redo operations with document management and value migration
- **Extract Struct** - Extract selected struct fields into new struct type with undo/redo
- **Collapse Struct** - Expand struct fields into their constituent fields with undo/redo
- **Extract Array** - Extract multiple same-type fields into array field with undo/redo
- **Collapse Array** - Collapse array field into multiple independent fields with undo/redo
- **Find Implement** - Menu command for finding abstract type implementations

### Article Management

- **Article Document** - Article document management with article collection and `article://` URI resolver, supporting `.sarticle` file format
- **Article** - Article node supporting nested child articles, title, overview, content, type, writing guide, and annotations
- **Article Document View** - Tree-inspector layout editing interface for article documents

### Text Documents

- **Prompt Document** - AI prompt template document for AIGC workflows with `.sprompt` file format
- **Canvas Text Node** - Text node displayable on canvas

### External Document Support

- **Excel Text Asset** - Excel document asset extracting text content from `.xls` and `.xlsx` files
- **Word Text Asset** - Word document asset extracting text content from `.doc` and `.docx` files
- **PDF Text Asset** - PDF document asset extracting text content from `.pdf` files
- **Word to Markdown Converter** - Static utility converting Word (.docx) documents to Markdown format

### PDF Extraction

- **PDFsharp Extractor** - Single-threaded PDF text extractor based on PDFsharp with font parsing and Unicode mapping

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  Document Management                     │
│  ┌──────────┬──────────────┬──────────────────────┐     │
│  │Type      │  Article     │   External           │     │
│  │Design    │  Management  │   Document Parsing   │     │
│  │(Struct,  │  (Hierarchy, │   (Excel, Word,      │     │
│  │ Enum,    │   Markdown,  │    PDF)              │     │
│  │ Abstract)│   article://)│                      │     │
│  └──────────┴──────────────┴──────────────────────┘     │
├─────────────────────────────────────────────────────────┤
│              Refactoring Tools                           │
│     (Extract/Collapse Struct & Array with Undo/Redo)     │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library
- **Suity.DataSync** - Data synchronization and serialization
- **Suity.Editor** - Editor framework interfaces
- **PDFsharp** - PDF processing library

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
