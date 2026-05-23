# Technical Specification: Simple HTML + CSS Static Web Application

## Project Overview
This document outlines the technical standards and architecture for a modern, framework-agnostic static web application built exclusively using standard HTML5 and CSS3. The project prioritizes semantic markup, maintainable styling, responsive design, performance, accessibility, and strict separation of structure and presentation. **No JavaScript behavior is required or permitted.**

## Technology Stack
- **Markup:** HTML5 (Semantic, Valid, Accessible)
- **Styling:** CSS3 (Custom Properties, Flexbox/Grid, Container Queries, Responsive Design)
- **Development Server (Optional):** Live Server / `npx serve` / Python `http.server` (for local preview only)
- **Validation:** W3C Markup Validation Service / CSS Validation Service
- **Format & Lint (Optional):** Prettier / Stylelint (for team consistency)

## Project Structure Example
- This project uses a **Flat & Modular File Structure** as follows:

```text
/
├── assets/
│   ├── images/          # Optimized images (WebP/AVIF fallbacks recommended)
│   └── fonts/           # Local font files (WOFF2)
├── css/
│   └── main.css         # Global styles, CSS variables, reset, & responsive rules
├── index.html           # Root HTML (Single entry point)
├── .gitignore
└── README.md            # Project documentation (optional)
```

## Entry Point Specification (`index.html`)
The root HTML file must adhere strictly to the following structure. It serves as the single, self-contained entry point for the application.

```html
<!doctype html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <meta name="description" content="(App description)" />
    <title>(App Title)</title>
    <link rel="stylesheet" href="/css/main.css" />
  </head>
  <body>
    <header>...</header>
    <main id="content">...</main>
    <footer>...</footer>
  </body>
</html>
```

**Requirements:**
- **Encoding:** Must be `UTF-8`.
- **Viewport:** Must include responsive meta tag.
- **Semantic Structure:** Use proper HTML5 semantic tags (`header`, `main`, `section`, `article`, `nav`, `aside`, `footer`). Avoid presentational `div` nesting.
- **Resource Loading:** Styles must be linked in `<head>`. **Do NOT include `<script>` tags, inline styles, or inline event handlers.**
- **Accessibility:** Ensure proper heading hierarchy (`h1`–`h6`), `alt` attributes for images, and ARIA attributes only when native semantics are insufficient.
- **Clean Markup:** Output must pass W3C validation without errors.

## .gitignore file initialization (`.gitignore`)
```text
.DS_Store
Thumbs.db
*.log
.env*
!.env.example
.vscode/
.idea/
```

## Configuration & Architecture
- **CSS Architecture:** Use CSS Custom Properties (`:root {}`) for theming, spacing, and typography scales. Prefer logical properties (`margin-inline`, `padding-block`) over physical ones.
- **Layout Strategy:** Mobile-first approach. Use Flexbox for 1D layouts, CSS Grid for 2D layouts. Avoid float/position hacks for primary layout.
- **Reset/Normalize:** Include a lightweight CSS reset or modern normalize to ensure cross-browser consistency before custom styles.
- **Performance:** Minimize HTTP requests. Use `srcset`/`sizes` for responsive images. Leverage `loading="lazy"` for below-the-fold media. Prefer `WOFF2` for fonts with `font-display: swap`.

## Coding Standards
- **Separation of Concerns:** HTML exclusively for structure & semantics. CSS exclusively for presentation & layout. **Zero JavaScript allowed.**
- **Naming Conventions:**
  - HTML/CSS Classes: `kebab-case` or BEM (`block__element--modifier`, e.g., `card__title--featured`)
  - IDs: Only for anchors or JS-free anchor navigation (e.g., `id="top"`). Avoid using IDs for styling.
  - Assets: `kebab-case.ext` (e.g., `hero-banner.webp`, `icon-search.svg`)
- **CSS Best Practices:** 
  - Use `rem`/`em` for typography & spacing. Use `vh`/`vw`/`%`/`min()`/`max()` for responsive sizing.
  - Avoid `!important`. Resolve specificity conflicts through architecture, not overrides.
  - Use modern selectors (`:is()`, `:where()`, `:has()`) responsibly to simplify complex rules.
- **Formatting:** Consistent indentation (2 spaces), trailing semicolons, and logical grouping (Box Model → Typography → Layout → Visuals → Responsive).

## Development & Deployment
- **Development:** Open `index.html` directly in a browser for basic preview, or use a lightweight static server (e.g., `npx serve`) to avoid local `file://` CORS restrictions with custom fonts/modules.
- **Validation:** Run code through W3C HTML/CSS validators before finalizing.
- **Production Deployment:** No build step, minification, or bundling required. Commit/push static files directly to hosting (GitHub Pages, Netlify, Vercel, or traditional FTP/SFTP).
- **Caching:** Leverage native HTTP caching via hosting provider headers if available.

## Dependencies
- **None.** This project relies exclusively on native browser APIs. No external CSS frameworks, icon libraries, or npm packages are required or permitted.

## **Important Notice**
- The `index.html` file must strictly follow the Entry Point Specification. **Do NOT** write logical, layout, or interactive code inside `index.html`.
- Maintain strict separation: Structure in `/index.html`, styling in `/css/main.css`.
- All asset paths must be relative and consistent (e.g., `/css/main.css`, `/assets/images/logo.webp`) to ensure reliable local and production deployment.
- Test across modern browsers (Chrome, Firefox, Safari, Edge) and multiple viewport sizes. Ensure graceful degradation and full accessibility compliance.