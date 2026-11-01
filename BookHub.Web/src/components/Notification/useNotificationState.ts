import { useCallback, useState } from 'react'
import type { AlertColor } from '@mui/material'

export interface NotificationMessage {
  id: number
  message: string
  severity: AlertColor
}

// Errors stay longer than confirmations because the user needs time to read what went wrong.
const AUTO_HIDE_MS: Record<AlertColor, number> = {
  success: 3000,
  info: 3000,
  warning: 5000,
  error: 5000,
}

export function useNotificationState() {
  const [current, setCurrent] = useState<NotificationMessage | null>(null)
  const [open, setOpen] = useState(false)

  const notify = useCallback((message: string, severity: AlertColor = 'success') => {
    setCurrent({ id: Date.now(), message, severity })
    setOpen(true)
  }, [])

  // Closing only hides the snackbar; the message is kept until the exit transition ends,
  // otherwise the content would vanish abruptly instead of fading out.
  const close = useCallback(() => setOpen(false), [])
  const clear = useCallback(() => setCurrent(null), [])

  return {
    current,
    open,
    autoHideDuration: AUTO_HIDE_MS[current?.severity ?? 'success'],
    notify,
    close,
    clear,
  }
}
