import { Paper, Typography } from '@mui/material'
import styles from './PlaceholderPage.module.css'

export function PlaceholderPage({ title }: { title: string }) {
  return (
    <Paper className={styles.root}>
      <Typography variant="h5" gutterBottom>
        {title}
      </Typography>
      <Typography color="text.secondary">Em construção.</Typography>
    </Paper>
  )
}
