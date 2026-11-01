import { createCrudHooks } from './createCrudHooks'
import type { Book, BookListParams, BookRequest } from './types'

export const booksApi = createCrudHooks<Book, BookRequest, BookListParams>('books')
