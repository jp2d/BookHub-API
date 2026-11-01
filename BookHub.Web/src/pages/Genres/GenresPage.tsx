import AutoStoriesIcon from '@mui/icons-material/AutoStories'
import type { Genre } from '@/api'
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
import { validateGenreName } from '@/validation'
import { useGenresPage } from './useGenresPage'

const columns: DataTableColumn<Genre>[] = [{ key: 'name', header: 'Nome', render: (genre) => genre.name }]

export function GenresPage() {
  const page = useGenresPage()
  const { list, query, formTarget, deleteTarget } = page

  return (
    <>
      <PageHeader title="Gêneros" actionLabel="Novo gênero" onAction={page.openCreate}>
        <SearchField label="Buscar gênero" onSearch={list.setSearch} />
      </PageHeader>

      <QueryState
        isLoading={query.isLoading}
        error={query.error}
        isEmpty={page.rows.length === 0}
        emptyMessage="Nenhum gênero encontrado."
        onRetry={query.refetch}
      >
        <DataTable
          columns={columns}
          rows={page.rows}
          getRowKey={(genre) => genre.id}
          renderActions={(genre) => (
            <RowActions
              itemLabel={genre.name}
              onEdit={() => page.openEdit(genre)}
              onDelete={() => page.askDelete(genre)}
            >
              <RowActionButton
                label="Ver livros"
                ariaLabel={`Ver livros do gênero ${genre.name}`}
                icon={<AutoStoriesIcon fontSize="small" />}
                onClick={() => page.viewBooks(genre)}
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
          title={formTarget.mode === 'edit' ? 'Editar gênero' : 'Novo gênero'}
          label="Nome do gênero"
          initialName={formTarget.mode === 'edit' ? formTarget.item.name : ''}
          validate={validateGenreName}
          submitting={page.isSubmitting}
          onSubmit={(name) => page.submit({ name })}
          onClose={page.closeForm}
        />
      )}

      <ConfirmDialog
        open={deleteTarget !== null}
        title="Excluir gênero"
        message={`Deseja excluir o gênero "${deleteTarget?.name ?? ''}"? Esta ação não pode ser desfeita.`}
        loading={page.isDeleting}
        onConfirm={page.confirmDelete}
        onCancel={page.cancelDelete}
      />
    </>
  )
}
