export const colorTokens = [
  {
    name: 'Background',
    variable: '--cq-color-background',
    value: '#070b10',
  },
  {
    name: 'Surface',
    variable: '--cq-color-surface',
    value: '#0d141c',
  },
  {
    name: 'Elevated',
    variable: '--cq-color-surface-elevated',
    value: '#111b25',
  },
  {
    name: 'Cyan Accent',
    variable: '--cq-color-accent',
    value: '#28d7c5',
  },
  {
    name: 'Blue Accent',
    variable: '--cq-color-accent-blue',
    value: '#5aa7ff',
  },
  {
    name: 'Warning',
    variable: '--cq-color-warning',
    value: '#f3b34b',
  },
] as const

export const foundationItems = [
  {
    label: 'React, TypeScript, Vite',
    description:
      'The web platform is isolated from the Unity game and ready for typed UI development.',
  },
  {
    label: 'Tailwind theme bindings',
    description:
      'CSS variables are mapped into Tailwind utilities with ClassQuest-specific names.',
  },
  {
    label: 'Reusable visual language',
    description:
      'Dark surfaces, restrained cyan and blue accents, amber warning states, grid texture, and glow effects are centralized.',
  },
] as const
