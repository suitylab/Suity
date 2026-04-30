# Suity.Editor.AIGC.API

Suity.Editor.AIGC.API is the API plugin module for integrating third-party AI LLM and image generation models. Based on the OpenAI-compatible protocol, it supports multiple AI service providers including OpenAI, DeepSeek, Alibaba DashScope, SiliconFlow, AIHubMix, OpenRouter, and local LM Studio.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Documentation](#documentation)
- [License](#license)

## Overview

Suity.Editor.AIGC.API provides a plugin-based architecture for connecting to various AI service providers through a unified OpenAI-compatible interface. Each provider plugin implements the same base classes, ensuring consistent behavior while allowing provider-specific configurations. The system supports dynamic model list caching, streaming and non-streaming responses, tool calling, and visual plugin configuration.

## Features

### Base Classes

- **BaseOpenAIPlugin** - Abstract base for OpenAI-compatible API plugins providing plugin initialization, model list management, property sync, and view configuration
- **BaseOpenAICall** - Abstract base for OpenAI-compatible API calls handling dialogue management, message appending, tool calling, streaming output, and non-streaming requests
- **OkGoDoItHelper** - API model list management and URL formatting utility supporting model list cache loading, saving, and asynchronous remote download

### Service Provider Plugins

- **OpenAI** - Official OpenAI provider with default API endpoint `https://api.openai.com`
- **DeepSeek** - DeepSeek AI provider with default API endpoint `https://api.deepseek.com`
- **DashScope** - Alibaba Cloud DashScope (Bailian) provider with default API endpoint `https://dashscope.aliyuncs.com/compatible-mode`
- **SiliconFlow** - SiliconFlow AI provider with default API endpoint `https://api.siliconflow.cn`
- **AIHubMix** - AIHubMix AI service provider
- **OpenRouter** - OpenRouter AI provider with default API endpoint `https://openrouter.ai/api` for multi-model aggregated access
- **LM Studio** - Local AI model server with default API endpoint `http://127.0.0.1:1234` for local model invocation

### Plugin Assets

- **LLM Model Assets** - Each provider defines its own LLM model assets for text generation
- **Image Generation Assets** - Each provider defines image generation model assets where applicable

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│              BaseOpenAIPlugin / BaseOpenAICall           │
│              (OpenAI-Compatible Base Classes)            │
├──────┬───────┬───────┬───────┬───────┬───────┬──────────┤
│OpenAI│DeepSeek│DashScope│Silicon│AIHubMix│OpenRouter│LM  │
│      │        │         │Flow    │        │          │Studio│
├──────┴───────┴───────┴───────┴───────┴───────┴──────────┤
│              Model List Cache & Download                 │
│              (OkGoDoItHelper)                            │
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
