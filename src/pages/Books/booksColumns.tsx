import { Chip } from '@mui/material'
import type { Book } from '@/api'
import type { DataTableColumn } from '@/components'

interface BooksColumnsHandlers {
  onAuthorClick: (authorId: string) => void
  onGenreClick: (genreId: string) => void
}

// Author and genre are chips: they show the relationship and, when clicked, filter the list by it.
export function createBooksColumns({ onAuthorClick, onGenreClick }: BooksColumnsHandlers): DataTableColumn<Book>[] {
  return [
    { key: 'title', header: 'Título', sortable: true, render: (book) => book.title },
    { key: 'year', header: 'Ano', sortable: true, render: (book) => book.publicationYear },
    { key: 'isbn', header: 'ISBN', render: (book) => book.isbn ?? '—' },
    {
      key: 'author',
      header: 'Autor',
      render: (book) => (
        <Chip
          size="small"
          label={book.authorName}
          onClick={() => onAuthorClick(book.authorId)}
          aria-label={`Filtrar pelo autor ${book.authorName}`}
        />
      ),
    },
    {
      key: 'genre',
      header: 'Gênero',
      render: (book) => (
        <Chip
          size="small"
          color="primary"
          variant="outlined"
          label={book.genreName}
          onClick={() => onGenreClick(book.genreId)}
          aria-label={`Filtrar pelo gênero ${book.genreName}`}
        />
      ),
    },
  ]
}
