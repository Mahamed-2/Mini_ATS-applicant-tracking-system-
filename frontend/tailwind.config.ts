import type { Config } from 'tailwindcss';

export default {
  content: [
    './index.html',
    './src/**/*.{vue,js,ts,jsx,tsx}',
  ],
  darkMode: ['class', '[data-theme="dark"]'],
  theme: {
    extend: {
      colors: {
        canvas: 'var(--canvas)',
        'surface-card': 'var(--surface-card)',
        'surface-modal': 'var(--surface-modal)',
        'surface-hover': 'var(--surface-hover)',
        border: {
          subtle: 'var(--border-subtle)',
          strong: 'var(--border-strong)',
        },
        primary: {
          DEFAULT: 'var(--primary)',
          hover: 'var(--primary-hover)',
          subtle: 'var(--primary-subtle)',
        },
        ai: {
          accent: 'var(--ai-accent)',
          hover: 'var(--ai-accent-hover)',
          subtle: 'var(--ai-subtle)',
          border: 'var(--ai-border)',
        },
        success: {
          DEFAULT: 'var(--success)',
          hover: 'var(--success-hover)',
          subtle: 'var(--success-subtle)',
          border: 'var(--success-border)',
        },
        warning: {
          DEFAULT: 'var(--warning)',
          hover: 'var(--warning-hover)',
          subtle: 'var(--warning-subtle)',
          border: 'var(--warning-border)',
        },
        danger: {
          DEFAULT: 'var(--danger)',
          hover: 'var(--danger-hover)',
          subtle: 'var(--danger-subtle)',
          border: 'var(--danger-border)',
        },
        info: {
          DEFAULT: 'var(--info)',
          hover: 'var(--info-hover)',
          subtle: 'var(--info-subtle)',
          border: 'var(--info-border)',
        },
        text: {
          primary: 'var(--text-primary)',
          muted: 'var(--text-muted)',
          variant: 'var(--text-variant)',
        },
      },
      borderRadius: {
        xs: 'var(--radius-xs)',
        sm: 'var(--radius-sm)',
        md: 'var(--radius-md)',
        lg: 'var(--radius-lg)',
        full: 'var(--radius-full)',
      },
      fontFamily: {
        sans: ['Inter', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'sans-serif'],
        mono: ['ui-monospace', 'SFMono-Regular', 'Menlo', 'Monaco', 'Consolas', 'monospace'],
      },
      boxShadow: {
        card: '0 1px 3px 0 rgba(15, 23, 42, 0.04)',
        modal: '0 8px 24px -4px rgba(15, 23, 42, 0.08), 0 2px 6px -1px rgba(15, 23, 42, 0.04)',
        drag: '0 12px 28px -6px rgba(15, 23, 42, 0.16)',
        'ai-glow': '0 0 12px -2px rgba(124, 58, 237, 0.25)',
      },
    },
  },
  plugins: [],
} satisfies Config;
