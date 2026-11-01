import type { FormEvent, ReactNode } from 'react'
import { Button, Dialog, DialogActions, DialogContent, DialogTitle } from '@mui/material'
import styles from './FormDialog.module.css'

interface FormDialogProps {
  title: string
  submitLabel?: string
  submitting?: boolean
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
  onClose: () => void
  children: ReactNode // the form fields
}

// Shell of every create/edit dialog: title, form element, cancel and submit buttons.
// It is mounted only while open, so each opening starts with fresh form state.
export function FormDialog({ title, submitLabel = 'Salvar', submitting = false, onSubmit, onClose, children }: FormDialogProps) {
  return (
    <Dialog open onClose={submitting ? undefined : onClose} maxWidth="sm" fullWidth>
      <form onSubmit={onSubmit} noValidate>
        <DialogTitle>{title}</DialogTitle>
        <DialogContent className={styles.content}>{children}</DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={submitting}>
            Cancelar
          </Button>
          <Button type="submit" variant="contained" disabled={submitting}>
            {submitLabel}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}
