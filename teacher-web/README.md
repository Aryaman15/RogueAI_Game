# ClassQuest Teacher Web

React, TypeScript, Vite, Tailwind CSS, and Lucide React foundation for the ClassQuest teacher web platform.

## Scripts

```bash
npm run dev
npm run build
npm run preview
npm run lint
```

## Design System

The visual foundation lives in `src/styles/` and `src/design-system/`.

- `src/styles/tokens.css` defines ClassQuest CSS variables for color, spacing, radius, shadows, typography, transitions, and glow.
- `src/index.css` imports Tailwind CSS and maps ClassQuest variables into Tailwind utility names with `@theme inline`.
- `src/styles/base.css` sets global rendering, focus, selection, and page defaults.
- `src/styles/effects.css` contains reusable technology-grid and subtle-glow utilities.
- `src/design-system/foundation.ts` keeps starter token metadata for setup verification.

No landing page sections, navbar, or hero have been built yet.
