import { Chip, Typography } from '@mui/material'
import styles from './FilterChips.module.css'

export interface ActiveFilter {
  key: string
  label: string
  onRemove: () => void
}

// Shows the filters currently applied to a list, each one removable. Renders nothing when there are none.
export function FilterChips({ filters }: { filters: ActiveFilter[] }) {
  if (filters.length === 0) return null

  return (
    <div className={styles.root} aria-label="Filtros ativos">
      <Typography variant="body2" color="text.secondary">
        Filtros:
      </Typography>
      {filters.map((filter) => (
        <Chip key={filter.key} label={filter.label} color="primary" variant="outlined" onDelete={filter.onRemove} />
      ))}
    </div>
  )
}
