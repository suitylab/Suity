# Project Initialization & Architecture Rules (TypeScript + Vite)

## 1. Workspace Discovery
- **NEVER** run interactive CLI commands like `npm create vite@latest`, `npm create`, or `npm init`. All work MUST be fully automated and non-interactive.
- **Do not scaffold the project from scratch.** Do not create initial root files, initial source files, or initial configuration files.
- **Explore first:** Before writing or modifying any code, inspect the existing workspace file structure and read the scaffold files already present in the project folder.
- Treat the existing scaffold as the source of truth. Do not overwrite, regenerate, replace, or reset it unless the user explicitly instructs you to do so.
- If a scaffold file appears missing or incomplete, make only the smallest non-destructive change needed to continue.

## 2. Architecture & Code Organization
- Design a multi-file, object-oriented software structure.
- Do not put all core code into a single file.
- Separate concerns into modules, classes, or components as appropriate.

## 3. Asset Handling & Rendering Standards
- **Strict Binary File Prohibition:** Do NOT create, request, or reference external binary asset files (e.g., `.png`, `.jpg`, `.jpeg`, `.webp`, `.wav`, `.mp3`).
- **2D Graphics:** Use **Pixi.js** (`pixi.js`). Draw all graphics programmatically using vector primitives (`PIXI.Graphics`) or inline SVG.
- **3D Graphics:** Use **Three.js** (`three`). Construct models purely via procedural combinations of 3D primitive geometries paired with procedural materials.

## 4. Web Platform Runtime Notes

### Environment
- This project runs on a **web development platform** (in-browser sandbox). There are no real OS processes or a true shell, and the filesystem is an in-memory workspace. Do not rely on host-only behaviors.

### Supported compound commands
- **Sequential chaining** — `;` runs commands in order regardless of success.
- **Conditional chaining** — `&&` runs the next command only if the previous one succeeded; `||` runs it only if the previous one failed.
- **Output redirection** — capture output into a workspace file: `> file` (overwrite), `>> file` (append), `2>&1` (merge stderr).

### Limitations
- Not supported: pipes (`|`), input redirection (`<`, heredoc `<<`), glob expansion (`*`, `?`), environment-variable prefixes (`FOO=bar cmd`), and `cd`.
- A dev-start command (`npm run dev`) automatically stops any running dev server before a later command executes — put the dev command **last** in a chain.
- Use `npm run dev` to start the live preview in the built-in preview panel.