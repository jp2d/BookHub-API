import type { ReactNode } from 'react'
import { IconButton, Tooltip, type IconButtonProps } from '@mui/material'

interface RowActionButtonProps {
  label: string // tooltip text
  ariaLabel?: string // accessible name; defaults to the tooltip text
  icon: ReactNode
  color?: IconButtonProps['color']
  onClick: () => void
}

// Icon button with tooltip: the building block of every row action.
export function RowActionButton({ label, ariaLabel, icon, color, onClick }: RowActionButtonProps) {
  return (
    <Tooltip title={label}>
      <IconButton aria-label={ariaLabel ?? label} color={color} onClick={onClick}>
        {icon}
      </IconButton>
    </Tooltip>
  )
}
