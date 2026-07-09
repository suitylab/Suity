System Integration & Runtime Contract (SIRC)

## Unified Logging & Telemetry Standard
- Define the global Logging Manager and logging format `[Level] [TraceID] [Module] [EventName] Message | {ContextJSON}` that mandates `TraceID` correlation across all log levels.
- Define the logging points/types for all modules and entities.

## Module Internal State Management
- Define the internal states for each module, ensuring robust state management with atomic mutations and lifecycle hooks.
- Define logging mechanism when state is changed.

## Event Bus & Event-Driven Architecture
- Define the global Event Manager。
- Define event types for all modules and entities.

## Unified State Snapshot & Inspection Mechanism
- Define the snapshot mechanism.
- Specify the snapshot properties for all modules and entities.

## System Bootstrapping & Initialization Sequence
- Define the deterministic initialization flow for all modules, including specific execution steps, triggered events
- Define expected log outputs during initialization.

## Inter-Module Activation & Triggering Flow
- Define expected triggering mechanism between all modules and entities (triggering event -> expected behaviour).

**Do NOT output specific language code, output markdown text only**