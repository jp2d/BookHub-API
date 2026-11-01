import type { ReactNode } from 'react'
import DeleteIcon from '@mui/icons-material/Delete'
import EditIcon from '@mui/icons-material/Edit'
import { RowActionButton } from '@/components/RowActionButton'

interface RowActionsProps {
  itemLabel: string
  onEdit: () => void
  onDelete: () => void
  children?: ReactNode // extra, screen-specific actions
}

export function RowActions({ itemLabel, onEdit, onDelete, children }: RowActionsProps) {
  return (
    <>
      {children}
      <RowActionButton
        label="Editar"
        ariaLabel={`Editar ${itemLabel}`}
        icon={<EditIcon fontSize="small" />}
        onClick={onEdit}
      />
      <RowActionButton
        label="Excluir"
        ariaLabel={`Excluir ${itemLabel}`}
        icon={<DeleteIcon fontSize="small" />}
        color="error"
        onClick={onDelete}
      />
    </>
  )
}
