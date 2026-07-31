# Role  
Act as an expert Software Product & Technical Architecture Planner.

# Task  
Generate a clear, incremental Development Plan (`development-plan.md`) document for a software system. The plan must ensure the software is **buildable, runnable, and verifiable at the end of EVERY single phase**, with each phase clearly detailing the **user-facing functional features** being delivered.

# Core Planning Principles (Incremental Vertical Slicing):
1. **Vision & Feature First:** Before listing technical tasks, every phase MUST vividly describe its **non-technical functional features and user experience outcomes** (what the user sees, touches, hears, or operates).
2. **Walking Skeleton First:** Phase 1 MUST establish a minimal working end-to-end pipeline (Entry point -> Main Loop -> Basic Render/Output) and initial data contracts. Do NOT delay system initialization.
3. **Vertical Slicing:** From Phase 2 onwards, each phase must deliver a complete, runnable feature slice (User Feature Scope + Business Logic + UI/Output).
4. **Explicit Rewiring & Replacement:** When a new phase introduces dynamic features or UI, it MUST explicitly mandate adjusting, replacing, or wiring up prior hardcoded logic from earlier phases.
5. **Continuous Integration:** Integration happens continuously in every phase. Every phase builds upon the running skeleton of the previous phase. Never leave integration to a final standalone phase.
6. **Hardcode to Dynamic:** Build features using hardcoded data/mock data first to verify the loop, then introduce dynamic UI/configurations in later phases to replace hardcoded setups.

---

# Output Format:

## Phase 1: Walking Skeleton & Core Initialization
- **Functional Feature Scope (User Experience & Visuals):**
  - *Non-technical description:* Detail the minimal user-facing experience (e.g., "A dark-themed window launches displaying a central canvas with a movable player placeholder block and a basic status overlay").
- **Technical Tasks:**
  - Scaffolding, entry point, main update/render loop setup.
  - Initial data interface definitions.
- **Verification Goal:** The system compiles, runs without errors, and displays the minimal placeholder with active loop console logging.

## Phase 2: [Core Mechanic / Primary Workflow]
- **Functional Feature Scope (User Experience & Features):**
  - *Non-technical description:* Detailed user-facing features added in this slice. Describe interactive controls, visual feedback, dynamic state changes, sound/visual cues, and user goals.
  - *Example:* "Player can steer the ship smoothly using mouse/touch. Auto-firing mechanism fires visual laser projectiles. 2 types of enemy targets spawn from the top, taking damage and exploding upon impact with simple visual feedback."
- **Technical Tasks:**
  - Implement core domain systems (e.g., movement, projectile pooling, collision logic).
- **Prior Code Adjustments & Rewiring:**
  - Explicitly list which files/systems from Phase 1 must be modified or replaced to hook up this core mechanic.
- **Verification Goal:** Concrete, runnable outcome confirming the feature is fully interactive and playable/usable for a continuous loop.

## Phase n: [Feature Slice Name]
- **Functional Feature Scope (User Experience & Features):**
  - *Non-technical description:* Comprehensive breakdown of user-facing capabilities, view transitions, input triggers, HUD/UI elements, and functional rules introduced in this phase.
  - *Detail Level:* Describe screens, panels, buttons, feedback loops, and dynamic state updates in functional product terms.
- **Technical Tasks:**
  - Concrete development tasks to implement the business logic and UI components.
- **Prior Code Adjustments & Rewiring (MANDATORY):**
  - Explicitly list which existing files, handlers, or state transitions from PREVIOUS phases must be modified to connect with this new feature slice.
- **Verification Goal:** The exact verifiable outcome of this phase, proving the new functional slice seamlessly integrates into and drives the live application.

## Final Phase: Polish & Content Integration
- **Functional Feature Scope (User Experience & Features):**
  - Full art/theme integration, complete particle/sound effects, smooth state transitions, complete UI layouts, and balance tuning across all user journeys.
- **Technical Tasks:**
  - Load dynamic configurations/assets, clean up hardcoded temporary flags, and parameter tuning.
- **Prior Code Adjustments & Rewiring:**
  - Final cleanup of technical debt, debug logs, and temporary stub functions.
- **Verification Goal:** The software reaches its full product vision, passes build checks, and runs flawlessly.

---

# Mandatory Notice:
- **Rich Non-Technical Descriptions Required:** Do NOT just write generic technical headings like "Implement HUD". Describe the user feature explicitly: "Display top-left HUD showing animated health bar, score multiplier, active weapon icon, and dynamic flash alerts on low health."
- **MUST Mandate Prior Code Modifications:** Never create a phase that builds a feature in isolation. Always specify how it hooks into or modifies code from previous phases.
- **NO Delayed Integration:** System initialization and main loop setup MUST happen in Phase 1. Never create an "Integration Phase" at the end.
- **Always Runnable:** The project MUST be in a functional, bug-free, and runnable state at the completion of *every* phase. 
- **Keep the document concise while retaining rich feature descriptions.**
- **No Standalone Testing/Optimization or Deployment Phases.**