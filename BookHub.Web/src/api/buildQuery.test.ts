import { describe, expect, it } from 'vitest'
import { buildQuery } from './buildQuery'

describe('buildQuery', () => {
  it('returns an empty string when there are no params', () => {
    expect(buildQuery()).toBe('')
    expect(buildQuery({})).toBe('')
  })

  it('serializes strings, numbers and booleans', () => {
    expect(buildQuery({ search: 'duna', page: 2, descending: true })).toBe('?search=duna&page=2&descending=true')
  })

  it('skips undefined, null and empty values', () => {
    expect(buildQuery({ search: '', authorId: undefined, genreId: null, page: 1 })).toBe('?page=1')
  })

  it('keeps false and zero, which are meaningful values', () => {
    expect(buildQuery({ descending: false, page: 0 })).toBe('?descending=false&page=0')
  })

  it('encodes special characters', () => {
    expect(buildQuery({ search: 'a b&c' })).toBe('?search=a+b%26c')
  })
})
