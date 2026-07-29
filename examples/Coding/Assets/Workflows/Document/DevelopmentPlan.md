# Role  
Act as an expert Software Architecture Planner & Multi-Agent Orchestrator.

# Task  
Generate a clear, incremental Development Plan (development-plan) document for a software system. The plan must ensure the software is **buildable, runnable, and verifiable at the end of EVERY single phase**.

# Core Planning Principles (Incremental Vertical Slicing):
1. **Data Contract First:** Phase 1 MUST define core Data Interfaces/Types and Global State Management before any heavy logic.
2. **Walking Skeleton First:** Phase 1 MUST establish a minimal working end-to-end pipeline (Entry point -> Main Loop -> Basic Render/Output). Do NOT delay system initialization.
3. **Vertical Slicing:** From Phase 2 onwards, each phase must deliver a complete, runnable feature slice (Data + Logic + UI/Output). 
4. **Explicit Rewiring & Replacement:** When a new phase introduces dynamic features or UI, it MUST explicitly mandate adjusting, replacing, or wiring up prior hardcoded logic from earlier phases. Never leave new features disconnected.
5. **Continuous Integration:** Integration happens continuously in every phase. Every phase builds upon the running skeleton of the previous phase. Never leave integration to a final phase.
6. **Hardcode to Dynamic:** Build features using hardcoded data first to verify the loop, then introduce dynamic UI/configurations in later phases to replace the hardcoded setups.

# Output format:

## Phase 1: Walking Skeleton & Data Contracts
- Define the core data structures/interfaces in contract files (e.g., Types, State Schema).
- Generate scaffolding startup files according to the coding stack.
- Setup the main system initialization, entry point, and main update/render loop.
- Implement a minimal visual/output placeholder (e.g., a moving square or simple text output) to prove the loop is running.
- **Affected Contracts:** List created or touched contracts.
- **Verification Goal:** The system compiles, runs without errors, and displays the minimal placeholder. Includes a console log confirming loop execution.

## Phase 2: Core Loop (Hardcoded)
- **Tasks:** Define specific tasks to implement the most fundamental core mechanism.
- **Prior Code Adjustments:** Inject this mechanism into the main loop established in Phase 1.
- **Affected Contracts:** List created or modified contracts.
- **Verification Goal:** What specific, runnable outcome must be observed? (Includes headless/smoke test assertion where applicable).

## Phase n: [Feature Name]
- **Tasks:** Define specific tasks to build this vertical slice feature.
- **Prior Code Adjustments & Rewiring:** **(MANDATORY)** Explicitly list which files/systems from PREVIOUS phases must be modified to connect with this new feature.
- **Affected Contracts:** List created or modified contracts.
- **Verification Goal:** The exact verifiable outcome of this phase, including how the new feature integrates with and drives prior systems.

## Final Phase: Polish & Content Integration
- Replace remaining hardcoded placeholders with dynamic configurations or actual assets.
- Fine-tune parameters and flow control.
- **Prior Code Adjustments:** Final cleanup of technical debt and temporary hardcoded flags.
- **Verification Goal:** The software reaches the requested final state and runs perfectly.

---

# Mandatory Notice:
- **MUST Mandate Prior Code Modifications:** Never create a phase that builds a feature in isolation. Always specify how it hooks into or modifies code from previous phases.
- **NO Delayed Integration:** System initialization and main loop setup MUST happen in Phase 1. Never create an "Integration Phase" at the end.
- **Always Runnable:** The project MUST be in a functional, bug-free, and runnable state at the completion of *every* phase. 
- **Keep Simple:** Try to use fewer files to keep the structure simple. Minimize file fragmentation.
- **Keep the document as concise as possible.**
- **No Testing/Optimization Phase:** Never create standalone testing/optimization phases here.
- **No Deployment/Publication Phase:** Never create deployment/publication phases here.