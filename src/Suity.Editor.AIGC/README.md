# Suity.Editor.AIGC

Suity.Editor.AIGC is the core AI Generated Content (AIGC) framework for the Suity editor, providing LLM service abstraction, multi-type AI assistants, RAG knowledge base with vector and graph retrieval, tool calling system, workflow-based AI task orchestration, and comprehensive prompt configuration.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.Editor.AIGC provides a complete AI integration layer for the Suity editor. It abstracts LLM service operations, implements multiple AI assistant types for different contexts, integrates a RAG (Retrieval-Augmented Generation) knowledge base system, and provides a tool calling mechanism for extending AI capabilities. The framework supports workflow-based AI task orchestration and a rich prompt configuration system for classification, extraction, generation, and knowledge base queries.

## Features

### LLM Service Layer

- **LLM Service** - Abstract base providing core LLM operations including chat, workflow execution, model retrieval, and function calling
- **LLM Interfaces** - Core interfaces for LLM models, calls, chats, embedding models, and image generation models
- **LLM Chats** - Chat session implementations including base, empty, and manual chat variants
- **LLM Models** - Model and model group asset definitions with type enumeration
- **LLM Calls** - Base LLM call implementation with streaming response handling
- **Embedding Models** - Embedding model assets for vector operations

### AI Assistant System

- **AI Assistant** - Abstract base for AI assistants with metadata and empty implementation
- **Assistant Service** - Abstract base providing LLM calls, prompt management, classification, task decomposition, canvas operations, and more
- **Canvas Assistant** - Canvas context-aware AI assistant base
- **Document Assistant** - Document assistant handling document creation, editing, knowledge queries, and more
- **Generative Assistant** - Base for document generation, editing, and batch operations
- **Node Graph Assistant** - Assistant providing graph rule building, type context, and node connection information
- **Canvas Context** - Managing canvas state including target document, selections, and more
- **Prompt Builder** - Building AI prompts with context-aware content

### RAG Knowledge Base

- **RAG Service** - Vector knowledge base service with embedding model, knowledge base management, vector query, and knowledge indexing
- **Retrieval Modes** - Support for both vector retrieval and graph retrieval modes
- **Canvas Switchable Node** - Interface for nodes that can switch between canvas modes

### AI Tool System

- **AI Tool** - Abstract base for AI tools with metadata and generic tool implementation
- **Tooling Assistant** - Base for handling tool calling requests
- **Tool Parameters** - Parameter definitions for data tables, documents, example data, generative operations, and type edits
- **Data Model Service** - Data model management with segmentation and specification

### Workflow System

- **Workflow Interfaces** - Interfaces for running workflows and workflow runners
- **Workflow Integration** - Integration with Suity.Editor.Flows for visual workflow design

### Task Page System

- **Task Page Interfaces** - Interfaces for pages, task pages, skills, and tool assets
- **Task Page Implementation** - Task page implementations for AI task management
- **Task Page Extensions** - Extension methods for task page operations

### Prompt Configuration

- **Configurations** - AI configuration classes for classifiers, segmenters, extractors, support configs, knowledge base configs, and data generation configs
- **Prompt Assets** - Prompt asset definitions and common prompt templates
- **AI Prompt Info** - AI prompt information structure

### Utilities

- **LLM Extensions** - LLM-related extension methods
- **AIGC Extensions** - AIGC-related extension methods
- **AIGC Colors** - AIGC color constant definitions
- **Progress Counter** - Progress tracking for AI operations
- **Retry Helper** - Retry logic for AI operations
- **Computation Helper** - Computation utilities
- **Attachment Set** - Attachment management for AI operations
- **Image Generation** - Image generation asset definitions

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    LLM Service                           │
│         (Chat, Workflow, Models, Function Call)          │
├──────────────┬──────────────┬───────────────────────────┤
│   AI         │   RAG        │    AI Tools               │
│ Assistants   │ Knowledge    │   (Tool Calling,          │
│ (Canvas,     │ Base         │    Parameters)            │
│  Document,   │ (Vector,     │                           │
│  Generative) │  Graph)      │                           │
├──────────────┴──────────────┴───────────────────────────┤
│              Workflow & Task Page System                 │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library
- **Suity.Rex** - Reactive data management and DI
- **Suity.DataSync** - Data synchronization and serialization
- **Suity.Editor** - Editor framework interfaces
- **Suity.Editor.Flows** - Visual flowchart editing and execution

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
