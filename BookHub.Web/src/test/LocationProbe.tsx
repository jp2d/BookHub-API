import { useLocation } from 'react-router-dom'

// Renders the current URL so tests can assert where the app navigated to.
export function LocationProbe() {
  const location = useLocation()
  return <div data-testid="location">{location.pathname + location.search}</div>
}
