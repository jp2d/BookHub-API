import { Link, Outlet } from 'react-router-dom'
import { AppBar, Container, Tab, Tabs, Toolbar, Typography } from '@mui/material'
import MenuBookIcon from '@mui/icons-material/MenuBook'
import { NAVIGATION } from '@/routes'
import { useActiveSection } from './useActiveSection'
import styles from './Layout.module.css'

export function Layout() {
  const activeSection = useActiveSection()

  return (
    <div className={styles.root}>
      <AppBar position="static" elevation={1}>
        <Toolbar className={styles.toolbar}>
          <MenuBookIcon />
          <Typography variant="h6" component="div" className={styles.brand}>
            BookHub
          </Typography>
          <Tabs value={activeSection} textColor="inherit" indicatorColor="secondary" aria-label="Navegação principal">
            {NAVIGATION.map((item) => (
              <Tab key={item.path} value={item.path} label={item.label} component={Link} to={item.path} />
            ))}
          </Tabs>
        </Toolbar>
      </AppBar>
      <Container maxWidth="lg" className={styles.content}>
        <Outlet />
      </Container>
    </div>
  )
}
