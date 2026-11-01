import { TextField } from '@mui/material'
import { authorsApi, genresApi, type Book, type BookRequest } from '@/api'
import { AsyncAutocomplete, FormDialog } from '@/components'
import { useBookForm } from './useBookForm'
import styles from './BookFormDialog.module.css'

interface BookFormDialogProps {
  book?: Book // present when editing
  submitting: boolean
  onSubmit: (request: BookRequest) => void
  onClose: () => void
}

export function BookFormDialog({ book, submitting, onSubmit, onClose }: BookFormDialogProps) {
  const { values, errors, setField, handleSubmit } = useBookForm({ book, onSubmit })

  return (
    <FormDialog title={book ? 'Editar livro' : 'Novo livro'} submitting={submitting} onSubmit={handleSubmit} onClose={onClose}>
      <TextField
        autoFocus
        label="Título"
        value={values.title}
        onChange={(event) => setField('title', event.target.value)}
        error={Boolean(errors.title)}
        helperText={errors.title}
        disabled={submitting}
        fullWidth
      />
      <div className={styles.row}>
        <TextField
          label="Ano de publicação"
          type="number"
          value={values.publicationYear}
          onChange={(event) => setField('publicationYear', event.target.value)}
          error={Boolean(errors.publicationYear)}
          helperText={errors.publicationYear}
          disabled={submitting}
        />
        <TextField
          label="ISBN (opcional)"
          value={values.isbn}
          onChange={(event) => setField('isbn', event.target.value)}
          error={Boolean(errors.isbn)}
          helperText={errors.isbn ?? '10 ou 13 dígitos'}
          disabled={submitting}
        />
      </div>
      <AsyncAutocomplete
        label="Autor"
        value={values.author}
        onChange={(author) => setField('author', author)}
        useList={authorsApi.useList}
        getOptionLabel={(author) => author.name}
        error={errors.author}
        disabled={submitting}
      />
      <AsyncAutocomplete
        label="Gênero"
        value={values.genre}
        onChange={(genre) => setField('genre', genre)}
        useList={genresApi.useList}
        getOptionLabel={(genre) => genre.name}
        error={errors.genre}
        disabled={submitting}
      />
    </FormDialog>
  )
}
