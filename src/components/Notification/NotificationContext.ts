import { createContext, useContext } from 'react'
import type { AlertColor } from '@mui/material'

export type Notify = (message: string, severity?: AlertColor) => void

export const NotificationContext = createContext<Notify | null>(null)

export function useNotify(): Notify {
  const notify = useContext(NotificationContext)
  if (!notify) throw new Error('useNotify must be used inside NotificationProvider')
  return notify
}
