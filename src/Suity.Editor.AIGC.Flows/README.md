# Suity.Editor.AIGC.Flows

Suity.Editor.AIGC.Flows is the AIGC workflow module providing visual flowchart-based AI workflow design and execution. It supports LLM call nodes, article processing, prompt building, XML parsing, web browsing, hierarchical task management with sub-tasks, model selection, function calling, output validation, retry logic, and canvas document types.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.Editor.AIGC.Flows extends the Suity.Editor.Flows framework with AI-specific workflow nodes and execution capabilities. It enables users to visually design complex AI workflows by connecting specialized nodes for LLM calls, article manipulation, prompt engineering, XML processing, web browsing, and task management. The module also includes a comprehensive task page system for hierarchical AI task orchestration.

## Features

### Workflow Core

- **AIGC Flow Plugin** - Plugin implementing `IAigcWorkflowRunner` for workflow execution and chat service provision
- **AIGC Flow Document** - Document type for creating and editing AI workflow charts with format and asset type definitions
- **AIGC Flow Node** - Base class for all AIGC flow nodes with async computation support and workflow configuration access
- **AIGC Flow Computation** - Computation engine handling node execution and session integration
- **Flow Selection List** - Node factory panel containing all available AIGC node types
- **Flow Plugin** - Configuration plugin managing task decomposition, function calling prompts, AI call pause settings, and more
- **Data Types** - Custom diagram data types including ConversationThreadDataType with magenta connector styling

### Workflow Nodes

- **Workflow Node** - Starting node for AIGC workflow execution with default language model configuration
- **Workflow Chat** - Workflow-based chat interaction managing workflow execution, dialogue processing, and runner lifecycle
- **Call LLM** - Core LLM call node supporting model selection, function calling, output validation, and retry logic with output verification
- **LLM Classifier** - LLM classifier node calling LLM to categorize input text into predefined categories

### Article Processing Nodes

- **Article Nodes** - Comprehensive article node collection including GetArticle, GetArticleByUrl, GetOrCreateArticle, SetArticle, GetArticleInformation, ForeachArticles, CloneArticleOutline, CloneArticle, ManualSelectArticle, ArticleGuideToContent, and ArticleFullContent
- **AIGC Data Row Node** - Base class for data row-related AIGC flow nodes with async computation support

### Prompt Processing Nodes

- **ReplaceOnePrompt** - Single keyword replacement in prompts
- **BuildPrompt** - Building prompts from templates
- **ReplaceMultiplePrompts** - Multi-keyword replacement in prompts
- **GetSpeechLanguage** - Getting local language name

### XML Tag Processing Nodes

- **Extract XML Tags** - Extracting XML tags from text
- **Extract XML Tags with Routing** - Extracting XML tags and routing execution
- **Get/Set XML Attributes** - Getting and setting XML tag attributes
- **Get/Set XML Content** - Getting and setting XML tag content

### Additional Nodes

- **Workflow Log** - Outputting workflow log messages with forced pause support
- **Browse Web** - Browsing web pages and retrieving HTML or plain text content
- **Type Nodes** - Getting struct JSON Schema definitions, multiple struct definitions, and manual asset selection
- **Extract Code Block** - Extracting the first code block from Markdown text

### Page System

- **Page Definition Nodes** - AIGC page operation node base classes including runtime nodes, design-time definition nodes, and type definition nodes
- **Page Function Node** - Referencing page definition assets and executing pages in workflows with caller context
- **Page Canvas Node** - Page canvas node for visual page editing
- **Rect Tree Builder** - Rectangle tree builder for page element spatial layout
- **Page Elements** - Begin, end, parameter, task, workspace, tool, sub-page, result, group, message, file output, article output, prompt parameter, and sub-task output elements
- **AIGC preset Document** - Preset document definition
- **Knowledge Article List** - Knowledge article list management

### Task Page System

- **Task Page Document** - Managing task page creation, configuration, and execution orchestration with startup page, workspace, tool list, and knowledge article configuration
- **Task Page** - Managing AI generation content tasks with task hierarchy, prompt management, sub-task addition, and chat history collection
- **Task Page Runner** - Coordinating task page execution handling task creation, execution, and parent reporting
- **Page Instance** - Managing page elements, parameters, and computation context with page building, data sync, and flow call functionality
- **Page Elements** - Base page element class and specialized element types for parameters, outputs, messages, and control flow

### Workspace Flow Nodes

- **Workspace Node Base** - Base class for workspace-related flow nodes
- **Workspace File Operations** - ListWorkSpaceFiles, ReadWorkSpaceFile, WriteWorkSpaceFile, DeleteWorkSpaceFile, and IsWorkSpaceFileExist nodes

### Canvas System

- **Canvas Document** - Managing flow-based visual editing and asset nodes with node creation, drag-drop, and computation
- **Canvas Asset Node Resolver** - Resolving asset types to corresponding canvas asset nodes
- **Canvas Data Table Node** - Displaying and managing data table assets in tree view
- **Canvas Selection List** - Providing available tool node selection for canvas documents

### AI Startup

- **AIGC Startup Window** - User input prompt interface for selecting startup Agent and creating/executing task pages

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  AIGC Workflow Engine                     │
│         (Computation, Session Integration)               │
├──────────────┬──────────────┬───────────────────────────┤
│   LLM Call   │  Article     │   Prompt & XML            │
│   Nodes      │  Processing  │   Processing Nodes        │
├──────────────┼──────────────┼───────────────────────────┤
│   Web        │  Type        │   Task Page System        │
│   Browsing   │  Nodes       │   (Hierarchy, Execution)  │
├──────────────┴──────────────┴───────────────────────────┤
│              Canvas & Page Definition                    │
└─────────────────────────────────────────────────────────┘
```

## Dependencies

- **Suity** - Core framework library
- **Suity.Rex** - Reactive data management and DI
- **Suity.DataSync** - Data synchronization and serialization
- **Suity.Editor** - Editor framework interfaces
- **Suity.Editor.Flows** - Visual flowchart editing and execution
- **Suity.Editor.AIGC** - AIGC core framework

## Documentation

For detailed implementation information, please refer to the [source code](./).

## License

This project is part of the Suity ecosystem. See the main [LICENSE](../../LICENSE) file for details.
