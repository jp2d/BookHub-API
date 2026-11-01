import { buildQuery } from '@/api/buildQuery'

export const ROUTES = {
  books: '/books',
  authors: '/authors',
  genres: '/genres',
} as const

export interface NavigationItem {
  path: string
  label: string
}

// Adding a section to the main navigation is a one-line change here (open/closed).
export const NAVIGATION: NavigationItem[] = [
  { path: ROUTES.books, label: 'Livros' },
  { path: ROUTES.authors, label: 'Autores' },
  { path: ROUTES.genres, label: 'Gêneros' },
]

export interface BooksFilters {
  authorId?: string
  genreId?: string
}

// Link to the books list already filtered (the books screen reads these from the URL).
export const booksPath = (filters: BooksFilters): string => `${ROUTES.books}${buildQuery(filters)}`
