import { DataTable, FilterChips, PageHeader, QueryState, SearchField } from '@/components'
import { createBooksColumns } from './booksColumns'
import { useBooksPage } from './useBooksPage'

export function BooksPage() {
  const { list, query, rows, totalCount, activeFilters } = useBooksPage()

  const columns = createBooksColumns({ onAuthorClick: list.filterByAuthor, onGenreClick: list.filterByGenre })

  return (
    <>
      <PageHeader title="Livros">
        <SearchField label="Buscar por título" defaultValue={list.search} onSearch={list.setSearch} />
      </PageHeader>

      <FilterChips filters={activeFilters} />

      <QueryState
        isLoading={query.isLoading}
        error={query.error}
        isEmpty={rows.length === 0}
        emptyMessage="Nenhum livro encontrado."
        onRetry={query.refetch}
      >
        <DataTable
          columns={columns}
          rows={rows}
          getRowKey={(book) => book.id}
          sorting={{ key: list.sortBy, descending: list.descending, onSort: list.sort }}
          pagination={{
            page: list.page,
            pageSize: list.pageSize,
            totalCount,
            onPageChange: list.setPage,
            onPageSizeChange: list.setPageSize,
          }}
        />
      </QueryState>
    </>
  )
}
