import { Navigate, Route, Routes } from 'react-router-dom'
import { Layout } from '@/components'
import { PlaceholderPage } from '@/pages/Placeholder'
import { NAVIGATION, ROUTES } from '@/routes'

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<Navigate to={ROUTES.books} replace />} />
        {NAVIGATION.map((item) => (
          <Route key={item.path} path={item.path} element={<PlaceholderPage title={item.label} />} />
        ))}
        <Route path="*" element={<Navigate to={ROUTES.books} replace />} />
      </Route>
    </Routes>
  )
}
