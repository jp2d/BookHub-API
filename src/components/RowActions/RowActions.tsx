import type { ReactNode } from 'react'
import { IconButton, Tooltip } from '@mui/material'
import DeleteIcon from '@mui/icons-material/Delete'
import EditIcon from '@mui/icons-material/Edit'

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
      <Tooltip title="Editar">
        <IconButton aria-label={`Editar ${itemLabel}`} onClick={onEdit}>
          <EditIcon fontSize="small" />
        </IconButton>
      </Tooltip>
      <Tooltip title="Excluir">
        <IconButton aria-label={`Excluir ${itemLabel}`} color="error" onClick={onDelete}>
          <DeleteIcon fontSize="small" />
        </IconButton>
      </Tooltip>
    </>
  )
}
