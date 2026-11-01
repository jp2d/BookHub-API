import type { ReactNode } from 'react'
import { Alert, Fade, Snackbar } from '@mui/material'
import { NotificationContext } from './NotificationContext'
import { useNotificationState } from './useNotificationState'
import styles from './NotificationProvider.module.css'

const TRANSITION_MS = { enter: 200, exit: 500 }

export function NotificationProvider({ children }: { children: ReactNode }) {
  const { current, open, autoHideDuration, notify, close, clear } = useNotificationState()

  return (
    <NotificationContext.Provider value={notify}>
      {children}
      <Snackbar
        key={current?.id}
        open={open}
        autoHideDuration={autoHideDuration}
        onClose={(_, reason) => reason !== 'clickaway' && close()}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
        slots={{ transition: Fade }}
        slotProps={{ transition: { onExited: clear } }}
        transitionDuration={TRANSITION_MS}
      >
        {current ? (
          <Alert severity={current.severity} variant="filled" onClose={close} className={styles.alert}>
            {current.message}
          </Alert>
        ) : undefined}
      </Snackbar>
    </NotificationContext.Provider>
  )
}
