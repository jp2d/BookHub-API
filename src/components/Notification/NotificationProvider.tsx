import type { ReactNode } from 'react'
import { Alert, Snackbar } from '@mui/material'
import { NotificationContext } from './NotificationContext'
import { useNotificationState } from './useNotificationState'
import styles from './NotificationProvider.module.css'

export function NotificationProvider({ children }: { children: ReactNode }) {
  const { current, notify, close } = useNotificationState()

  return (
    <NotificationContext.Provider value={notify}>
      {children}
      <Snackbar
        key={current?.id}
        open={current !== null}
        autoHideDuration={5000}
        onClose={(_, reason) => reason !== 'clickaway' && close()}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
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
