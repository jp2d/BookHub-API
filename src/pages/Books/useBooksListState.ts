import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import type { BookListParams } from '@/api'
import { DEFAULT_PAGE_SIZE } from '@/hooks'

type UrlChanges = Record<string, string | undefined>

// List state of the books screen. Search, filters and sorting live in the URL, so links such as
// "view this author's books" and a browser reload keep the same view; pagination is local.
export function useBooksListState() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [page, setPage] = useState(0)
  const [pageSize, setPageSizeValue] = useState(DEFAULT_PAGE_SIZE)

  const search = searchParams.get('search') ?? ''
  const authorId = searchParams.get('authorId') ?? undefined
  const genreId = searchParams.get('genreId') ?? undefined
  const sortBy: NonNullable<BookListParams['sortBy']> = searchParams.get('sortBy') === 'year' ? 'year' : 'title'
  const descending = searchParams.get('descending') === 'true'

  function updateUrl(changes: UrlChanges) {
    setSearchParams((current) => {
      const next = new URLSearchParams(current)
      for (const [key, value] of Object.entries(changes)) {
        if (value) next.set(key, value)
        else next.delete(key)
      }
      return next
    })
    setPage(0)
  }

  const params: BookListParams = { search, authorId, genreId, sortBy, descending, page: page + 1, pageSize }

  return {
    search,
    authorId,
    genreId,
    sortBy,
    descending,
    page,
    pageSize,
    params,
    setPage,
    setPageSize: (value: number) => {
      setPageSizeValue(value)
      setPage(0)
    },
    setSearch: (value: string) => updateUrl({ search: value }),
    filterByAuthor: (id: string | undefined) => updateUrl({ authorId: id }),
    filterByGenre: (id: string | undefined) => updateUrl({ genreId: id }),
    sort: (key: string) => {
      if (key === sortBy) updateUrl({ descending: descending ? undefined : 'true' })
      else updateUrl({ sortBy: key === 'title' ? undefined : key, descending: undefined })
    },
  }
}
