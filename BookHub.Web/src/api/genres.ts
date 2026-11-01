import { createCrudHooks } from './createCrudHooks'
import type { Genre, GenreRequest } from './types'

export const genresApi = createCrudHooks<Genre, GenreRequest>('genres', { alsoInvalidate: ['books'] })
