import { createTheme } from '@mui/material/styles'

// Single source of truth for design tokens. `cssVariables` exposes them to the CSS Modules as
// custom properties (--mui-palette-*, --mui-shape-borderRadius, --mui-spacing, ...).
export const theme = createTheme({
  cssVariables: true,
  palette: {
    primary: { main: '#1565c0' },
    background: { default: '#f5f7fa' },
  },
  shape: { borderRadius: 8 },
})
