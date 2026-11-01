import type { ReactElement } from 'react'
import { MemoryRouter } from 'react-router-dom'
import { CssBaseline, ThemeProvider } from '@mui/material'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { NotificationProvider } from '@/components'
import { theme } from '@/theme'

// Renders a screen with the same providers as the real app (a fresh query cache per test, no retries).
export function renderWithProviders(ui: ReactElement, route = '/') {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

  return {
    user: userEvent.setup(),
    ...render(
      <QueryClientProvider client={queryClient}>
        <ThemeProvider theme={theme}>
          <CssBaseline />
          <NotificationProvider>
            <MemoryRouter initialEntries={[route]}>{ui}</MemoryRouter>
          </NotificationProvider>
        </ThemeProvider>
      </QueryClientProvider>,
    ),
  }
}
