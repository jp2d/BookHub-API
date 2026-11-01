import AutoStoriesIcon from '@mui/icons-material/AutoStories'
import type { Author } from '@/api'
import {
  ConfirmDialog,
  DataTable,
  NameFormDialog,
  PageHeader,
  QueryState,
  RowActionButton,
  RowActions,
  SearchField,
  type DataTableColumn,
} from '@/components'
import { validateAuthorName } from '@/validation'
import { useAuthorsPage } from './useAuthorsPage'

const columns: DataTableColumn<Author>[] = [{ key: 'name', header: 'Nome', render: (author) => author.name }]

export function AuthorsPage() {
  const page = useAuthorsPage()
  const { list, query, formTarget, deleteTarget } = page

  return (
    <>
      <PageHeader title="Autores" actionLabel="Novo autor" onAction={page.openCreate}>
        <SearchField label="Buscar autor" onSearch={list.setSearch} />
      </PageHeader>

      <QueryState
        isLoading={query.isLoading}
        error={query.error}
        isEmpty={page.rows.length === 0}
        emptyMessage="Nenhum autor encontrado."
        onRetry={query.refetch}
      >
        <DataTable
          columns={columns}
          rows={page.rows}
          getRowKey={(author) => author.id}
          renderActions={(author) => (
            <RowActions itemLabel={author.name} onEdit={() => page.openEdit(author)} onDelete={() => page.askDelete(author)}>
              <RowActionButton
                label="Ver livros"
                ariaLabel={`Ver livros de ${author.name}`}
                icon={<AutoStoriesIcon fontSize="small" />}
                onClick={() => page.viewBooks(author)}
              />
            </RowActions>
          )}
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
        <NameFormDialog
          title={formTarget.mode === 'edit' ? 'Editar autor' : 'Novo autor'}
          label="Nome do autor"
          initialName={formTarget.mode === 'edit' ? formTarget.item.name : ''}
          validate={validateAuthorName}
          submitting={page.isSubmitting}
          onSubmit={(name) => page.submit({ name })}
          onClose={page.closeForm}
        />
      )}

      <ConfirmDialog
        open={deleteTarget !== null}
        title="Excluir autor"
        message={`Deseja excluir o autor "${deleteTarget?.name ?? ''}"? Esta ação não pode ser desfeita.`}
        loading={page.isDeleting}
        onConfirm={page.confirmDelete}
        onCancel={page.cancelDelete}
      />
    </>
  )
}
