import type { ReactNode } from 'react'
import { Alert, Button, CircularProgress, Typography } from '@mui/material'
import { getErrorMessage } from '@/api'
import styles from './QueryState.module.css'

interface QueryStateProps {
  isLoading: boolean
  error: Error | null
  isEmpty: boolean
  emptyMessage: string
  onRetry: () => void
  children: ReactNode
}

// Renders the loading, error and empty states of any query; the content only when there is data.
export function QueryState({ isLoading, error, isEmpty, emptyMessage, onRetry, children }: QueryStateProps) {
  if (isLoading) {
    return (
      <div className={styles.centered}>
        <CircularProgress aria-label="Carregando" />
      </div>
    )
  }

  if (error) {
    return (
      <Alert
        severity="error"
        action={
          <Button color="inherit" size="small" onClick={onRetry}>
            Tentar de novo
          </Button>
        }
      >
        {getErrorMessage(error, 'Não foi possível carregar os dados.')}
      </Alert>
    )
  }

  if (isEmpty) {
    return (
      <Typography color="text.secondary" className={styles.empty}>
        {emptyMessage}
      </Typography>
    )
  }

  return <>{children}</>
}
