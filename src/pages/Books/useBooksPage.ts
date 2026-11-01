import { authorsApi, booksApi, genresApi } from '@/api'
import type { ActiveFilter } from '@/components'
import { useCrudPage } from '@/hooks'
import { useBooksListState } from './useBooksListState'

export function useBooksPage() {
  const crud = useCrudPage({
    hooks: booksApi,
    list: useBooksListState(),
    messages: {
      created: 'Livro criado com sucesso.',
      updated: 'Livro atualizado com sucesso.',
      deleted: 'Livro excluído com sucesso.',
    },
  })

  const { list } = crud
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

  return { ...crud, activeFilters }
}
