import { authorsApi, booksApi, genresApi } from '@/api'
import type { ActiveFilter } from '@/components'
import { useBooksListState } from './useBooksListState'

export function useBooksPage() {
  const list = useBooksListState()
  const query = booksApi.useList(list.params)
  const author = authorsApi.useOne(list.authorId)
  const genre = genresApi.useOne(list.genreId)

  const activeFilters: ActiveFilter[] = []
  if (list.authorId) {
    activeFilters.push({
      key: 'author',
      label: `Autor: ${author.data?.name ?? '...'}`,
      onRemove: () => list.filterByAuthor(undefined),
    })
  }
  if (list.genreId) {
    activeFilters.push({
      key: 'genre',
      label: `Gênero: ${genre.data?.name ?? '...'}`,
      onRemove: () => list.filterByGenre(undefined),
    })
  }

  return {
    list,
    query,
    rows: query.data?.items ?? [],
    totalCount: query.data?.totalCount ?? 0,
    activeFilters,
  }
}
