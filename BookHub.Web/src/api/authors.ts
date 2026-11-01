import { createCrudHooks } from './createCrudHooks'
import type { Author, AuthorRequest } from './types'

export const authorsApi = createCrudHooks<Author, AuthorRequest>('authors', { alsoInvalidate: ['books'] })
