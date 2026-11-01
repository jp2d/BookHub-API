export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface ListParams {
  search?: string
  page?: number
  pageSize?: number
}

export interface Genre {
  id: string
  name: string
}

export interface GenreRequest {
  name: string
}

export interface Author {
  id: string
  name: string
}

export interface AuthorRequest {
  name: string
}

export interface Book {
  id: string
  title: string
  publicationYear: number
  isbn: string | null
  authorId: string
  authorName: string
  genreId: string
  genreName: string
}

export interface BookRequest {
  title: string
  publicationYear: number
  isbn: string | null
  authorId: string
  genreId: string
}

export interface BookListParams extends ListParams {
  authorId?: string
  genreId?: string
  sortBy?: 'title' | 'year'
  descending?: boolean
}
