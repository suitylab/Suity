# Role
Act as an expert Software Architect and Technical Writer.

# Task
Generate a comprehensive Technical Specification (tech-spec) document for a software system.

# Structure the document with the following sections:

1. System Architecture
Provide a high-level overview of the system design. Describe the architectural pattern, main components, data flow, and the rationale behind key architectural decisions.

2. Core Modules Description
Detail the primary modules of the application. Explain the specific responsibilities, boundaries, and interaction interfaces of each module without diving into code-level implementations.
- Example 1 (Backend): Base Framework, Database, Network, Business Logic, etc.
- Example 2 (Frontend): Base Framework, Core Logic, Rendering, UI, etc.

3. Core Algorithm Principles and Key Points
Explain the conceptual logic of the system's critical algorithms. Describe the step-by-step execution flow, data structures utilized, and complexity considerations using descriptive text.

4. Core Pipeline
Provide a high-level narrative of the application’s full lifecycle — from startup, initialization, and runtime event loop, to shutdown — outlining the major stages, state transitions, and cross-module coordination that define the system’s operational rhythm, without delving into implementation specifics.

5. Scaffolding (Initial Setup)
Scaffolding startup file list of current coding stack (e.g., `.gitignore`, `tsconfig.json`, `vite.config.ts`, `package.json`)

6. Folder Structure
Detailed and complete file tree view with component purposes.

7. External Dependencies
List the third-party libraries, frameworks, external APIs, and infrastructure services required. Explain the role and justification for each dependency in the system ecosystem.

# Adhere strictly to the following guidelines:
- Keep document clear and professional in markdown format.
- Describe structures, logic, and flows using natural language.
- DO NOT include any actual programming code or syntax-specific implementations.