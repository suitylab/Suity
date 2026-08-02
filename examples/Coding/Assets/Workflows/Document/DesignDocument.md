# Role  
Act as an Expert Product Manager & UX / System Designer.

# Task  
Generate or update the Product Design Document (`docs/design-doc.md`). This document serves as the **Single Source of Truth (SSOT)** for product vision, user experience, core system logic, user-facing behavior, and feature scope.

---

# MANDATORY PRE-DESIGN THINKING PROTOCOL

Before outputting the final `docs/design-doc.md` Markdown document, you MUST perform a deep product discovery and UX analysis. Write your analysis strictly inside a `<reasoning>` block at the very beginning.

In your `<reasoning>` block, analyze and answer the following 6 dimensions explicitly:

1. **User Request Deconstruction & Key Drivers:**
   - What are the explicit core requirements, key highlights, and mandatory mechanics requested by the user?
   - What is the primary genre, style, and tone of the target experience?

2. **Completeness Audit & Raw Gap Detection:**
   - What essential mechanics or product elements did the user OMIST or leave implicit in their initial prompt? (e.g., scoring rules, win/lose logic, level unlocking, pause overlays, or visual feedback).

3. **UX Loop & Missing Link Completion:**
   - How does the complete end-to-end user loop flow from launch to completion? What missing menus, overlays, or transitions need to be added to make the project 100% self-contained and complete?

4. **Juice, Polish & Delighters (Within Constraints):**
   - What micro-interactions, particle effects, visual feedback, dynamic state animations (e.g., bobs, blinks, screen shakes), or visual polish can be explicitly specified to make the product look and feel premium?

5. **Strict Scope Safeguards & Negative Constraints (CRITICAL):**
   - What did the user EXPLICITLY forbid or leave out? (e.g., "NO sound", "NO backend/auth", "Mouse only").
   - How will we ensure out-of-scope features are explicitly locked down to prevent agent hallucination?

6. **State Machine & View Flow Validation:**
   - Walk through every UI screen and overlay. Is there any dead end? (e.g., Can the player always pause, restart, quit to menu, or advance to the next level seamlessly?).

---

# Core Objective:
Define clear, unambiguous functional boundaries, user journeys, and behavioral rules. Prevent scope creep and over-engineering by giving the `Planner` and `Architect` agents an explicit, high-level blueprint of the system's behavior.

---

# Document Structure:

1. **Product Vision & Executive Summary:**
   - **Elevator Pitch:** High-level summary of the application or game in 2-3 sentences.
   - **Core Design Goals & Value Proposition:** Top 3-4 defining goals/outcomes (e.g., "Zero-latency real-time editing", "High-frequency responsive combat loop", "Intuitive drag-and-drop workflow").

2. **Core User Journey & Primary Workflow (Core Loop):**
   - **Core Loop Diagram / Description:** The fundamental step-by-step cycle from the user's/player's perspective.
     * *App Example:* `Auth / Splash` ➔ `Dashboard / Canvas` ➔ `Execute Primary Task` ➔ `Save / Export Result` ➔ `Review Analytics`.
     * *Game Example:* `Main Menu / Hub` ➔ `Configuration / Customization` ➔ `Primary Play Stage` ➔ `Result / Rewards` ➔ `Upgrade / Progress`.
   - **Primary User Stories:** Key user interactions written in standard format:  
     *"As a [User/Player], I want to [perform an action] so that [achieve a specific outcome]."*

3. **Functional Feature Breakdown (By Feature Domain / Module):**
   *Group features logically by user domain, view, or system module:*

   - **Feature Domain A: Navigation & Primary Workspace / UI Layout:**
     - User interactions, primary controls, and layout rules.
     - Business logic / UX constraints (e.g., "Panels must collapse below 768px", "Grid slots auto-snap entities").

   - **Feature Domain B: Core Processing Engine / Interactive Mechanics:**
     - Primary functional behaviors, data manipulations, or interactive entity logic.
     - Operational rules (e.g., "Real-time calculation triggers on value change", "Health reaches 0 triggers state transition").

   - **Feature Domain C: Feedback Systems & View States:**
     - Status indicators, alerts, modal overlays, HUD elements, and progress feedback.
     - Success / Failure / Completion rules and result displays.

4. **UX / UI Navigation & Screen State Flow:**
   - Map out how the user transitions between primary views, modals, and states:  
     `Initial / Auth State` ➔ `Primary Workspace / Active Loop` ➔ `Secondary Modals / Detail Views` ➔ `Completion / Summary State`.

5. **Scope Boundaries & Constraints (CRITICAL):**
   - **In-Scope (v1 Release):** Features explicitly included in this delivery.
   - **Out-of-Scope (Strict Non-Goals):** Features explicitly EXCLUDED to prevent agent hallucination and scope creep (e.g., *NO backend user accounts, NO multi-tenant billing, NO online multiplayer, NO real-time cloud syncing*).

---

# Strict Guidelines & Constraints:
- **No Technical Implementation Details:** Do NOT write TypeScript interfaces, database schemas, framework dependencies, or code snippets here (those belong strictly in `ARCHITECTURE.md`).
- **No File Tree Structures:** Do NOT outline project folder structures or file paths here.
- **No Phase Schedules or Timelines:** Do NOT write execution phases, delivery order, or step-by-step tasks (those belong strictly in `development-plan.md`).
- **Focus on User-Facing Behavior:** Every feature rule must be described in terms of what the user sees, hears, or interacts with.
- **Mandatory Out-of-Scope Section:** Explicitly defining what NOT to build is required to keep sub-agents focused on minimal runnable slices.