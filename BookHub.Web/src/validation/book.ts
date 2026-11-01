import type { Author, Genre } from '@/api'
import { LIMITS } from './limits'
import { maxLength, required, type FieldErrors } from './rules'

export interface BookFormValues {
  title: string
  publicationYear: string // text while typing; converted to a number on submit
  isbn: string
  author: Author | null
  genre: Genre | null
}

export const normalizeIsbn = (value: string): string => value.replace(/[-\s]/g, '').toUpperCase()

export function validateIsbn(value: string): string | undefined {
  if (!value.trim()) return undefined // optional

  const isbn = normalizeIsbn(value)
  const isValid = /^\d{9}[\dX]$/.test(isbn) || /^\d{13}$/.test(isbn)

  return isValid ? undefined : 'O ISBN deve conter 10 ou 13 dígitos.'
}

export function validatePublicationYear(value: string, currentYear = new Date().getFullYear()): string | undefined {
  const missing = required(value, 'O ano de publicação')
  if (missing) return missing

  const year = Number(value)
  const isValid = /^\d+$/.test(value.trim()) && year >= 1 && year <= currentYear

  return isValid ? undefined : `O ano de publicação deve estar entre 1 e ${currentYear}.`
}

export function validateBook(values: BookFormValues): FieldErrors<BookFormValues> {
  const errors: FieldErrors<BookFormValues> = {}

  const title = required(values.title, 'O título') ?? maxLength(values.title, LIMITS.bookTitle, 'O título')
  if (title) errors.title = title

  const year = validatePublicationYear(values.publicationYear)
  if (year) errors.publicationYear = year

  const isbn = validateIsbn(values.isbn)
  if (isbn) errors.isbn = isbn

  if (!values.author) errors.author = 'Selecione um autor.'
  if (!values.genre) errors.genre = 'Selecione um gênero.'

  return errors
}
