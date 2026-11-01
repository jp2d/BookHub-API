import { useCallback, useState } from 'react'
import type { AlertColor } from '@mui/material'

export interface NotificationMessage {
  id: number
  message: string
  severity: AlertColor
}

export function useNotificationState() {
  const [current, setCurrent] = useState<NotificationMessage | null>(null)

  const notify = useCallback((message: string, severity: AlertColor = 'success') => {
    setCurrent({ id: Date.now(), message, severity })
  }, [])

  const close = useCallback(() => setCurrent(null), [])

  return { current, notify, close }
}
