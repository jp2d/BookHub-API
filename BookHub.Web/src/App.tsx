import { Navigate, Route, Routes } from 'react-router-dom'
import { Layout } from '@/components'
import { AuthorsPage } from '@/pages/Authors'
import { BooksPage } from '@/pages/Books'
import { GenresPage } from '@/pages/Genres'
import { ROUTES } from '@/routes'

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<Navigate to={ROUTES.books} replace />} />
        <Route path={ROUTES.books} element={<BooksPage />} />
        <Route path={ROUTES.authors} element={<AuthorsPage />} />
        <Route path={ROUTES.genres} element={<GenresPage />} />
        <Route path="*" element={<Navigate to={ROUTES.books} replace />} />
      </Route>
    </Routes>
  )
}
