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

## 5. Web Platform Runtime Notes

### Environment
- This project is developed and executed on a **web development platform** (an in-browser sandbox runtime), so behavior differs from local development: there are no real OS processes or a true shell, and the filesystem is an in-memory workspace file system. Do not rely on host-only behaviors (e.g., spawning arbitrary binaries, real `dist` folders, or OS paths).

### Supported compound commands
The platform command runner supports the following constructs:

- **Sequential chaining** — `;` runs commands in order regardless of success:
  - `npm install ; npm run dev`
- **Conditional chaining** — `&&` runs the next command only if the previous one succeeded; `||` runs it only if the previous one failed:
  - `npm install && npm run build`
- **Output redirection** — capture a command's output into a workspace file (created in the workspace **Master** folder and visible in the editor):
  - `> file` (overwrite), `>> file` (append), `2>&1` (also merge stderr)
  - Example: `npm run build > build.log 2>&1`

Notes / limitations:
- Not supported: pipes (`|`), input redirection (`<`, heredoc `<<`), glob expansion (`*`, `?`), environment-variable prefixes (`FOO=bar cmd`), and `cd`.
- A dev-start command (`npm run dev`) automatically stops any running dev server before a later command executes — put the dev command **last** in a chain.
- Use `npm run dev` to start the live preview in the built-in preview panel.

### Build behavior (`npm run build`)
- `npm run build` only **compiles**; it **cannot build** — the sandbox runtime cannot persist final bundle artifacts, so **no `dist` output and no build log are produced** on success.
- Compile errors, however, **are still printed** to the command output.
- Rule of thumb: **empty command output after `npm run build` means the compilation passed.**