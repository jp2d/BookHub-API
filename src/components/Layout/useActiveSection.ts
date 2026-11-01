import { useLocation } from 'react-router-dom'
import { NAVIGATION } from '@/routes'

// Returns the path of the navigation item that matches the current URL (or false when none does).
export function useActiveSection(): string | false {
  const { pathname } = useLocation()

  return NAVIGATION.find((item) => pathname.startsWith(item.path))?.path ?? false
}
