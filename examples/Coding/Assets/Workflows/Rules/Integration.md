# System Integration & Runtime Contract (SIRC)

This contract defines the strict architectural and runtime guidelines for all modules, ensuring seamless integration, deterministic behavior, and comprehensive debuggability for AI agents and human developers.

## 1. Unified Logging & Telemetry Standard
All modules MUST adhere to a structured, machine-readable logging format to enable automated parsing and agent-driven debugging.
- **Standard Format**: `[Timestamp] [Level] [TraceID] [Module] [EventName] Message | {ContextJSON}`
- **State Transition Log**: `[Timestamp] INFO [TraceID] [Module] [StateChanged] PreviousState -> NewState | {TriggerReason}`
- **Event Bus Log**: `[Timestamp] DEBUG [TraceID] [EventBus] [EventFired] EventName | {PayloadJSON}`
- **Log Levels**: `TRACE` (granular steps), `DEBUG` (state/event changes), `INFO` (lifecycle milestones), `WARN` (recoverable anomalies), `ERROR` (failures).
- **Context**: All logs MUST include a `TraceID` to correlate distributed or asynchronous operations across the system.

## 2. Module Lifecycle & State Machine Management
Every core module MUST implement a strict finite state machine (FSM) to ensure predictable initialization and teardown.
- **Standard States**: 
  - `Uninitialized` -> `Initializing` -> `Loading` (fetching resources) -> `Ready` (dependencies met) -> `Running` (active processing).
  - `Running` <-> `Paused` (suspended execution).
  - `Running`/`Ready` -> `Error` (unrecoverable failure) -> `Recovering` -> `Running`.
  - Any State -> `Releasing` -> `Released` (graceful teardown).
- **Transition Rules**: State mutations MUST be atomic. Invalid transitions MUST throw an exception and log an `[InvalidTransition]` error.
- **Hooks**: Modules MUST implement `onEnterState()` and `onExitState()` hooks to handle resource allocation and cleanup.

## 3. Event Bus & Event-Driven Architecture
The Event Bus is the central nervous system. Direct function calls between independent modules are strictly prohibited; all communication MUST flow through the bus.
- **Event Naming Convention**: `Domain.Entity.Action` (e.g., `User.Input.Submit`, `System.Config.Reloaded`, `Data.Cache.Invalidated`).
- **1:1 Action Mapping**: Every UI interaction, API call, or internal trigger MUST map to exactly one canonical event. This allows agents to bypass the UI and invoke behaviors programmatically by emitting the exact event.
- **Dispatch Modes**: 
  - `Sync`: For critical path validations (blocks until all handlers complete).
  - `Async`: For side-effects and UI updates (fire-and-forget or promise-based).
- **Middleware & Interceptors**: The Event Bus MUST support pre/post-dispatch middleware for logging, payload validation, and dead-letter routing for failed event handlers.

## 4. Unified State Snapshot & Inspection Mechanism
To facilitate deep system inspection without halting execution, all core modules MUST expose a standardized snapshot interface.
- **Interface Contract**: `ISnapshotable { getSnapshot(version: string): JSON }`
- **Snapshot Directives**:
  - `SNAPSHOT.GET <module_id> [depth: deep|shallow]`: Retrieves the current property and state snapshot of a specific module or entity.
  - `SNAPSHOT.GET_GLOBAL`: Aggregates and returns a comprehensive, time-synchronized snapshot of the entire system topology and state.
  - `SNAPSHOT.DELTA <module_id> <base_version>`: Returns only the state changes since a specific version to optimize bandwidth.
- **Constraints**: Snapshots MUST be thread-safe, immutable upon creation, and handle circular references gracefully (e.g., via reference IDs).

## 5. System Bootstrapping & Initialization Sequence
The engine MUST follow a strict, deterministic bootstrapping flow to prevent race conditions, missing dependencies, and early crashes. Direct instantiation of dependent modules before their prerequisites are met is strictly prohibited.
- **Phase 1: Core Bootstrap**: Initialize the foundational services: Logger, Event Bus, and Service Locator/Dependency Injector. Emit `[Engine] [BootPhase] Core_Services_Online`.
- **Phase 2: Registration & Topology**: All modules register themselves and declare their dependencies. The engine calculates a topological sort to determine the exact initialization order.
- **Phase 3: Staggered Loading**: Modules sequentially transition `Uninitialized -> Initializing -> Loading`. Asynchronous resource fetching and data parsing occur in this phase.
- **Phase 4: Readiness Gate**: Modules transition `Loading -> Ready`. The engine blocks the main loop until all critical core modules report `Ready`.
- **Phase 5: Global Activation**: The engine emits the canonical `System.Global.Ready` event. All modules transition `Ready -> Running`, and the Main Game Loop officially begins.
- **Failure Handling**: If any module fails to load or times out, the engine MUST halt the boot sequence, emit an `[Engine] [BootFailed]` error log, and gracefully terminate or fallback to a safe mode.

## 6. Inter-Module Activation & Triggering Flow
This section defines the exact mechanisms for cross-module communication, state triggering, and lazy activation, ensuring zero direct coupling.
- **The Activation Chain**: When Module A completes a logic step, it MUST NOT call Module B directly. Instead, Module A emits an event (e.g., `Combat.Damage.Resolved`). The Event Bus routes this to Module B's listener, which then transitions its own state (e.g., `VFX_Module: Idle -> Emitting`) and executes its logic.
- **State-Triggered Cascades**: A module's internal state change MUST automatically trigger corresponding external events. For example, if `Entity.Health <= 0`, the Entity module transitions to `Dead` and emits `Entity.Lifecycle.Died`. This single event activates the Animation, VFX, Audio, and UI modules simultaneously via their respective listeners.
- **Lazy Wake-up & Sleeping**: To optimize performance, non-critical modules (e.g., specific UI panels, background audio) can enter a `Sleeping` state. They are activated only when a specific trigger event (e.g., `UI.Panel.Open.Request`) is fired, transitioning them to `Loading -> Running`.
- **Timeout & Deadlock Prevention**: If an activation chain stalls (e.g., Module B is waiting for a resource that Module C hasn't emitted yet), the Event Bus middleware MUST detect the timeout and emit a `[EventBus] [ActivationTimeout]` warning.