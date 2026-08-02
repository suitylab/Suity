# Role  
Act as an expert Software Product & Technical Architecture Planner.

# Task  
Generate a clear, incremental Development Plan (`development-plan.md`) document for a software system. The plan must ensure the software is **buildable, runnable, and verifiable at the end of EVERY single phase**, with each phase clearly detailing the user-facing functional features being delivered.

# Core Planning Principles (Incremental Vertical Slicing):

1. **Zero Information Loss (100% Traceability):** You MUST audit your plan against the Design Document. EVERY single feature, entity variation (e.g., all 5 enemy types, all 3 item types), visual effect, and UI element detailed in the Design Doc MUST be explicitly scheduled into a phase. **Do not compress, summarize, or omit entities to save space.**
2. **Vision & Feature First:** Before listing technical tasks, every phase MUST vividly describe its non-technical functional features and user experience outcomes (what the user sees, touches, hears, or operates).
3. **Walking Skeleton First:** Phase 1 MUST establish a minimal working end-to-end pipeline (Entry point -> Main Loop -> Basic Render/Output) and initial data contracts. 
4. **Strict Atomic Granularity & Micro-Pacing:** Keep step sizes extremely small. A phase must contain ONLY ONE major systemic addition or 1-2 entity variations. 
   - *Rule of Thumb:* If a phase introduces a new AI behavior AND a new UI system AND global visual effects, it is TOO LARGE. Split it immediately. (Standard complex projects should have 10-20 micro-phases, not 5-6).
5. **Explicit Rewiring & Replacement:** When a new phase introduces dynamic features or UI, it MUST explicitly mandate adjusting, replacing, or wiring up prior hardcoded logic from earlier phases.
6. **Continuous Integration:** Integration happens continuously. Every phase builds upon the running skeleton of the previous phase. Never leave integration to a final standalone phase.

---

# Output Format:

## Phase 1: Walking Skeleton & Core Initialization
- **Functional Feature Scope (User Experience & Visuals):**
  - *Non-technical description:* Detail the minimal user-facing experience.
- **Technical Tasks:**
  - Scaffolding, entry point, main update/render loop setup.
- **Prior Code Adjustments & Rewiring:**
  - None (Base setup).
- **Verification Goal:** The system compiles, runs without errors, and displays the minimal placeholder with active loop console logging.

## Phase 2: [Core Mechanic / Primary Workflow]
- **Design Doc Coverage:** Explicitly list the specific elements from the Design Doc being implemented here (e.g., "Covers: Core Shooting, Crosshair UI, Muzzle Flash VFX").
- **Functional Feature Scope (User Experience & Features):**
  - *Non-technical description:* Detailed user-facing features added in this slice.
- **Technical Tasks:**
  - Concrete tasks restricted to this specific atomic slice.
- **Prior Code Adjustments & Rewiring:**
  - Explicitly list which files/systems from Phase 1 must be modified.
- **Verification Goal:** Concrete, runnable outcome confirming the feature is playable/usable.

## Phase n: [Atomic Feature Slice Name]
- **Design Doc Coverage:** Explicitly map back to the exact entities/features in the Design Doc (e.g., "Covers: Velociraptor and Stegosaurus AI behaviors ONLY").
- **Functional Feature Scope (User Experience & Features):**
  - Comprehensive breakdown of user-facing capabilities.
- **Technical Tasks:**
  - Development tasks for the business logic and UI.
- **Prior Code Adjustments & Rewiring (MANDATORY):**
  - Identify logic from PREVIOUS phases that must be hooked into.
- **Verification Goal:** Verifiable outcome proving seamless integration.

*(Continue creating as many phases as necessary to ensure 100% coverage of the Design Doc without overloading any single phase.)*

## Final Phase: Polish & Final Integration
- **Design Doc Coverage:** "Covers: Final balancing, minor audio additions, remaining edge-case VFX."
- **Functional Feature Scope (User Experience & Features):**
  - Final UI layouts, balance tuning, and polish.
- **Technical Tasks:**
  - Clean up hardcoded temporary flags and parameter tuning.
- **Prior Code Adjustments & Rewiring:**
  - Final cleanup of technical debt and stubs.
- **Verification Goal:** The software reaches its full product vision.

---

# Mandatory Notice:
- **ANTI-COMPRESSION RULE:** You are forbidden from grouping more than 5 distinct entity types (e.g., enemy species, distinct power-ups) into a single phase. Expand the number of phases instead.
- **Rich Non-Technical Descriptions Required:** Do NOT just write generic technical headings. 
- **MUST Mandate Prior Code Modifications:** Always specify how a feature hooks into previous code.
- **Always Runnable:** The project MUST be bug-free and runnable at the end of *every* phase.
- **No Standalone Testing/Optimization Phases.**