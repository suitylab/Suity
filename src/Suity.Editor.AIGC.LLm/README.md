# Suity.Editor.AIGC.LLm

Suity.Editor.AIGC.LLm is the LLM integration module providing AI assistant systems for article generation, optimization, summarization, and segmentation, Mermaid diagram (flowchart, mind map) AI generation and rendering, multi-tier LLM model configuration, preset model types, and unified chat tool window interface.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.Editor.AIGC.LLm provides the concrete LLM service implementations and AI assistant systems for the Suity editor. It includes a comprehensive assistant architecture with specialized assistants for different tasks, multi-level model configuration management, Mermaid diagram generation, and a unified chat interface for AI interactions.

## Features

### Core Services

- **LLM Service Backend** - Handling chat operations, model retrieval, and LLM calls
- **LLM Model Plugin** - Managing model presets, tier configuration, and AI generation settings
- **AI Assistant Plugin** - Managing model presets, parameters, prompts, and component configuration
- **AI Assistant Service Backend** - Providing classification, task decomposition, assistant selection, and more

### Assistant System

- **Main Assistant** - Primary AI assistant routing to appropriate sub-assistants based on canvas context and request type
- **Common Chat Assistant** - Handling non-instructional user conversations
- **Image Gen Assistant** - Calling configured image generation models
- **Resume Assistant** - Resuming previously paused or interrupted AI execution
- **Assistant Chat** - Assistant chat session handling message processing and dialogue flow
- **Main Chat** - Main chat managing workflow-based LLM conversations
- **Wrapped LLM Call** - LLM call wrapper displaying model and parameter information before and after calls
- **Assistant Action** - AI assistant operation undo/redo implementations
- **Resolvers** - AI document assistant resolvers and assistant/tool information management
- **Values** - Data structures extracted from documents including segments, tasks, and assistant call chains

### Prompt Management

- **AI Prompt Manager** - Discovering and registering all available prompt types
- **Core Workflow Prompts** - Prompt definitions for data tables, flowcharts, tree diagrams, and complex field suggestions
- **General Prompts** - Prompts for minimal thinking, chapter updates, data modeling format, and article operations

### Article Operations

- **Article Edit Menu** - Article AIGC operations including generate, optimize, summarize, segment, split, and Q&A
- **Article Export Menus** - Article import/export/preview menu commands
- **Article Generate Assistants** - Collection of article-related AI assistants for generation, optimization, summarization, segmentation, splitting, and Q&A

### Mermaid Diagrams

- **Mermaid Assistant** - AI assistant for generating Mermaid diagrams supporting flowcharts and mind maps
- **Mermaid Service** - Mermaid diagram generation, caching, and rendering service

### Model & Call Types

- **Manual LLM Call** - Manual LLM model asset allowing users to provide responses directly instead of calling external LLM
- **Internal LLM Model Asset** - Base class for internal LLM model assets in the AIGC module

### UI Components

- **AIGC Chat Tool Window** - AI generation chat interface tool window
- **AIGC Menus** - AIGC workflow execution menus and attachment operation menus

### Debug Utilities

- **Debug Menus** - Debug menu commands for closing background documents

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  LLM Service Backend                     │
│         (Chat, Model Retrieval, LLM Calls)               │
├─────────────────────────────────────────────────────────┤
│                   AI Assistants                          │
│  ┌──────────┬──────────────┬──────────────────────┐     │
│  │Main      │  Common Chat │   Image Gen / Resume │     │
│  │Assistant │  Assistant   │   Assistants         │     │
│  └──────────┴──────────────┴──────────────────────┘     │
├─────────────────────────────────────────────────────────┤
│            Article Operations & Mermaid                  │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library
- **Suity.Editor** - Editor framework interfaces
- **Suity.Editor.AIGC** - AIGC core framework

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
