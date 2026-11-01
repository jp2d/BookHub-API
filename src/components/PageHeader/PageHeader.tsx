import type { ReactNode } from 'react'
import { Button, Typography } from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import styles from './PageHeader.module.css'

interface PageHeaderProps {
  title: string
  actionLabel: string
  onAction: () => void
  children?: ReactNode // search and filters
}

export function PageHeader({ title, actionLabel, onAction, children }: PageHeaderProps) {
  return (
    <div className={styles.root}>
      <Typography variant="h5" component="h1" className={styles.title}>
        {title}
      </Typography>
      {children}
      <Button variant="contained" startIcon={<AddIcon />} onClick={onAction}>
        {actionLabel}
      </Button>
    </div>
  )
}
