import { TextField } from '@mui/material'
import { FormDialog } from '@/components/FormDialog'
import type { NameValidator } from '@/validation'
import { useNameForm } from './useNameForm'

interface NameFormDialogProps {
  title: string
  label: string
  initialName?: string
  validate: NameValidator
  submitting: boolean
  onSubmit: (name: string) => void
  onClose: () => void
}

// Create/edit dialog for entities that only have a name (genres and authors).
export function NameFormDialog({
  title,
  label,
  initialName = '',
  validate,
  submitting,
  onSubmit,
  onClose,
}: NameFormDialogProps) {
  const { name, error, handleChange, handleSubmit } = useNameForm({ initialName, validate, onSubmit })

  return (
    <FormDialog title={title} submitting={submitting} onSubmit={handleSubmit} onClose={onClose}>
      <TextField
        autoFocus
        label={label}
        value={name}
        onChange={handleChange}
        error={Boolean(error)}
        helperText={error}
        disabled={submitting}
        fullWidth
      />
    </FormDialog>
  )
}
