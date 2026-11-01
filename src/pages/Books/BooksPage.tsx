import { ConfirmDialog, DataTable, FilterChips, PageHeader, QueryState, RowActions, SearchField } from '@/components'
import { BookFormDialog } from './BookFormDialog'
import { createBooksColumns } from './booksColumns'
import { useBooksPage } from './useBooksPage'

export function BooksPage() {
  const page = useBooksPage()
  const { list, query, formTarget, deleteTarget } = page

  const columns = createBooksColumns({ onAuthorClick: list.filterByAuthor, onGenreClick: list.filterByGenre })

  return (
    <>
      <PageHeader title="Livros" actionLabel="Novo livro" onAction={page.openCreate}>
        <SearchField label="Buscar por título" defaultValue={list.search} onSearch={list.setSearch} />
      </PageHeader>

      <FilterChips filters={page.activeFilters} />

      <QueryState
        isLoading={query.isLoading}
        error={query.error}
        isEmpty={page.rows.length === 0}
        emptyMessage="Nenhum livro encontrado."
        onRetry={query.refetch}
      >
        <DataTable
          columns={columns}
          rows={page.rows}
          getRowKey={(book) => book.id}
          renderActions={(book) => (
            <RowActions itemLabel={book.title} onEdit={() => page.openEdit(book)} onDelete={() => page.askDelete(book)} />
          )}
          sorting={{ key: list.sortBy, descending: list.descending, onSort: list.sort }}
          pagination={{
            page: list.page,
            pageSize: list.pageSize,
            totalCount: page.totalCount,
            onPageChange: list.setPage,
            onPageSizeChange: list.setPageSize,
          }}
        />
      </QueryState>

      {formTarget.mode !== 'closed' && (
        <BookFormDialog
          book={formTarget.mode === 'edit' ? formTarget.item : undefined}
          submitting={page.isSubmitting}
          onSubmit={page.submit}
          onClose={page.closeForm}
        />
      )}

      <ConfirmDialog
        open={deleteTarget !== null}
        title="Excluir livro"
        message={`Deseja excluir o livro "${deleteTarget?.title ?? ''}"? Esta ação não pode ser desfeita.`}
        loading={page.isDeleting}
        onConfirm={page.confirmDelete}
        onCancel={page.cancelDelete}
      />
    </>
  )
}
