import { describe, expect, it } from 'vitest'
import { normalizeIsbn, validateBook, validateIsbn, validatePublicationYear, type BookFormValues } from './book'
import { LIMITS } from './limits'

const validValues: BookFormValues = {
  title: 'Dom Casmurro',
  publicationYear: '1899',
  isbn: '',
  author: { id: 'a1', name: 'Machado de Assis' },
  genre: { id: 'g1', name: 'Clássicos' },
}

describe('validateIsbn', () => {
  it('accepts an empty value because the ISBN is optional', () => {
    expect(validateIsbn('')).toBeUndefined()
    expect(validateIsbn('   ')).toBeUndefined()
  })

  it.each(['9788535902775', '978-85-359-0277-5', '978 85 359 0277 5', '0306406152', '080442957x'])(
    'accepts %s',
    (isbn) => expect(validateIsbn(isbn)).toBeUndefined(),
  )

  it.each(['123', '97885359027', '97885359027ab', '978853590277X'])('rejects %s', (isbn) =>
    expect(validateIsbn(isbn)).toBe('O ISBN deve conter 10 ou 13 dígitos.'),
  )

  it('normalizes by removing separators and upper-casing the check digit', () => {
    expect(normalizeIsbn('080-442-957x')).toBe('080442957X')
  })
})

describe('validatePublicationYear', () => {
  it('requires a value', () => {
    expect(validatePublicationYear('')).toBe('O ano de publicação é obrigatório.')
  })

  it('accepts years between 1 and the current year', () => {
    expect(validatePublicationYear('1', 2026)).toBeUndefined()
    expect(validatePublicationYear('2026', 2026)).toBeUndefined()
  })

  it.each(['0', '-5', '2027', '19.9', 'abc'])('rejects %s', (year) =>
    expect(validatePublicationYear(year, 2026)).toBe('O ano de publicação deve estar entre 1 e 2026.'),
  )
})

describe('validateBook', () => {
  it('returns no errors for valid values', () => {
    expect(validateBook(validValues)).toEqual({})
  })

  it('reports every missing required field at once', () => {
    const errors = validateBook({ title: '', publicationYear: '', isbn: '', author: null, genre: null })

    expect(Object.keys(errors).sort()).toEqual(['author', 'genre', 'publicationYear', 'title'])
    expect(errors.author).toBe('Selecione um autor.')
    expect(errors.genre).toBe('Selecione um gênero.')
  })

  it('limits the title length', () => {
    expect(validateBook({ ...validValues, title: 'a'.repeat(LIMITS.bookTitle + 1) }).title).toContain(`${LIMITS.bookTitle} caracteres`)
  })

  it('reports an invalid ISBN only when one is informed', () => {
    expect(validateBook({ ...validValues, isbn: '123' }).isbn).toBe('O ISBN deve conter 10 ou 13 dígitos.')
    expect(validateBook({ ...validValues, isbn: '' }).isbn).toBeUndefined()
  })
})
