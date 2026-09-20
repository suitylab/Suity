# Project Initialization & Architecture Rules (TypeScript + Vite)

## 1. Project Setup & Automation Strategy
- **NEVER** run interactive CLI commands like `npm create vite@latest`, `npm create`, or `npm init`. All initialization MUST be fully automated and non-interactive.
- **Scaffold from Scratch:** Build the project structure manually by creating all setup files directly. Explicitly generate:
  - `package.json`
  - `tsconfig.json`
  - `vite.config.ts`
  - `.gitignore`
  - `index.html`

## 2. Directory Structure Guidelines
- Place all application source code exclusively inside the `src/` directory.
- Organized source layout:
  - `src/main.ts` (Entry point)
  - `src/style.css` (Global styles)
  - `src/components/` (Components and modules)

## 3. Lenient TypeScript Configuration (`tsconfig.json`)
- Prioritize dynamic prototyping and rapid code generation over strict type safety.
- The `tsconfig.json` file MUST enforce permissive compiler options:
  - Set `"strict": false`
  - Set `"noUnusedLocals": false`
  - Set `"noUnusedParameters": false`
  - Set `"noImplicitAny": false`

## 4. Asset Handling & Rendering Standards
- **Strict Binary File Prohibition:** Do NOT create, request, or reference external binary asset files (e.g., `.png`, `.jpg`, `.jpeg`, `.webp`, `.wav`, `.mp3`).
- **2D Graphics Standard:**
  - Use **Pixi.js** (`pixi.js`) as the default engine.
  - Draw all graphics programmatically using vector primitives (`PIXI.Graphics`) or inline SVG elements.
- **3D Graphics Standard:**
  - Use **Three.js** (`three`) as the default engine.
  - Construct models purely via procedural combinations of 3D primitive geometries (e.g., `BoxGeometry`, `SphereGeometry`, `CylinderGeometry`) paired with procedural materials (`MeshStandardMaterial`, `MeshBasicMaterial`).