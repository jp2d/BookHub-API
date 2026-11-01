import { Navigate, Route, Routes } from 'react-router-dom'
import { Layout } from '@/components'
import { GenresPage } from '@/pages/Genres'
import { PlaceholderPage } from '@/pages/Placeholder'
import { ROUTES } from '@/routes'

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<Navigate to={ROUTES.books} replace />} />
        <Route path={ROUTES.books} element={<PlaceholderPage title="Livros" />} />
        <Route path={ROUTES.authors} element={<PlaceholderPage title="Autores" />} />
        <Route path={ROUTES.genres} element={<GenresPage />} />
        <Route path="*" element={<Navigate to={ROUTES.books} replace />} />
      </Route>
    </Routes>
  )
}
