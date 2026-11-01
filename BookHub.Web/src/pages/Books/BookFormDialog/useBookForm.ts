import { useState, type FormEvent } from 'react'
import type { Book, BookRequest } from '@/api'
import { normalizeIsbn, validateBook, type BookFormValues, type FieldErrors } from '@/validation'

interface UseBookFormOptions {
  book?: Book // present when editing
  onSubmit: (request: BookRequest) => void
}

function initialValues(book?: Book): BookFormValues {
  if (!book) return { title: '', publicationYear: '', isbn: '', author: null, genre: null }

  return {
    title: book.title,
    publicationYear: String(book.publicationYear),
    isbn: book.isbn ?? '',
    author: { id: book.authorId, name: book.authorName },
    genre: { id: book.genreId, name: book.genreName },
  }
}

export function useBookForm({ book, onSubmit }: UseBookFormOptions) {
  const [values, setValues] = useState<BookFormValues>(() => initialValues(book))
  const [errors, setErrors] = useState<FieldErrors<BookFormValues>>({})

  function setField<K extends keyof BookFormValues>(field: K, value: BookFormValues[K]) {
    setValues((current) => ({ ...current, [field]: value }))
    setErrors((current) => ({ ...current, [field]: undefined }))
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const validation = validateBook(values)
    if (Object.keys(validation).length > 0) {
      setErrors(validation)
      return
    }

    onSubmit({
      title: values.title.trim(),
      publicationYear: Number(values.publicationYear),
      isbn: values.isbn.trim() ? normalizeIsbn(values.isbn) : null,
      authorId: values.author!.id,
      genreId: values.genre!.id,
    })
  }

  return { values, errors, setField, handleSubmit }
}
