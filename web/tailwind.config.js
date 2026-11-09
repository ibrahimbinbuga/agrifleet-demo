/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        ops: {
          950: '#0a0f0c',
          900: '#101712',
          850: '#141c17',
          800: '#1b251f',
          700: '#2a372f',
          500: '#6f8177',
          300: '#a9b8af',
          100: '#e4ebe6',
        },
        field: {
          600: '#3f9a4f',
          500: '#57b865',
          400: '#7fd18a',
        },
      },
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        mono: ['ui-monospace', 'SFMono-Regular', 'Menlo', 'monospace'],
      },
    },
  },
  plugins: [],
};
