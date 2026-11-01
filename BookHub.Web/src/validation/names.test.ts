import { describe, expect, it } from 'vitest'
import { LIMITS } from './limits'
import { nameValidator, validateAuthorName, validateGenreName } from './names'
import { maxLength, required } from './rules'

describe('rules', () => {
  it('required rejects empty and blank values', () => {
    expect(required('', 'O nome')).toBe('O nome é obrigatório.')
    expect(required('   ', 'O nome')).toBe('O nome é obrigatório.')
    expect(required('Terror', 'O nome')).toBeUndefined()
  })

  it('maxLength counts the trimmed value', () => {
    expect(maxLength('abc', 3, 'O nome')).toBeUndefined()
    expect(maxLength('  abc  ', 3, 'O nome')).toBeUndefined()
    expect(maxLength('abcd', 3, 'O nome')).toBe('O nome deve ter no máximo 3 caracteres.')
  })
})

describe('name validators', () => {
  it('validates the genre name using the API limit', () => {
    expect(validateGenreName('Ficção')).toBeUndefined()
    expect(validateGenreName('')).toBe('O nome do gênero é obrigatório.')
    expect(validateGenreName('a'.repeat(LIMITS.genreName))).toBeUndefined()
    expect(validateGenreName('a'.repeat(LIMITS.genreName + 1))).toContain(`${LIMITS.genreName} caracteres`)
  })

  it('validates the author name using the API limit', () => {
    expect(validateAuthorName('Machado de Assis')).toBeUndefined()
    expect(validateAuthorName(' ')).toBe('O nome do autor é obrigatório.')
    expect(validateAuthorName('a'.repeat(LIMITS.authorName + 1))).toContain(`${LIMITS.authorName} caracteres`)
  })

  it('builds validators for any label and limit', () => {
    const validate = nameValidator('O campo', 5)

    expect(validate('abcde')).toBeUndefined()
    expect(validate('abcdef')).toBe('O campo deve ter no máximo 5 caracteres.')
  })
})
