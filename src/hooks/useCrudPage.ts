import { useState } from 'react'
import { getErrorMessage, type CrudHooks } from '@/api'
import { useNotify } from '@/components/Notification'
import { useListState } from './useListState'

export type FormTarget<T> = { mode: 'closed' } | { mode: 'create' } | { mode: 'edit'; item: T }

interface CrudMessages {
  created: string
  updated: string
  deleted: string
}

interface UseCrudPageOptions<TEntity, TRequest> {
  hooks: CrudHooks<TEntity, TRequest>
  messages: CrudMessages
}

// Everything a CRUD list screen needs besides its markup: list state, queries, form/confirm dialogs
// and mutations with user feedback. Genres, authors and books configure it instead of repeating it.
export function useCrudPage<TEntity extends { id: string }, TRequest>({
  hooks,
  messages,
}: UseCrudPageOptions<TEntity, TRequest>) {
  const notify = useNotify()
  const list = useListState()
  const query = hooks.useList(list.params)
  const createMutation = hooks.useCreate()
  const updateMutation = hooks.useUpdate()
  const deleteMutation = hooks.useDelete()

  const [formTarget, setFormTarget] = useState<FormTarget<TEntity>>({ mode: 'closed' })
  const [deleteTarget, setDeleteTarget] = useState<TEntity | null>(null)

  const rows = query.data?.items ?? []
  const closeForm = () => setFormTarget({ mode: 'closed' })

  async function submit(body: TRequest) {
    try {
      if (formTarget.mode === 'edit') {
        await updateMutation.mutateAsync({ id: formTarget.item.id, body })
        notify(messages.updated)
      } else {
        await createMutation.mutateAsync(body)
        notify(messages.created)
      }
      closeForm()
    } catch (error) {
      notify(getErrorMessage(error), 'error')
    }
  }

  async function confirmDelete() {
    if (!deleteTarget) return

    try {
      await deleteMutation.mutateAsync(deleteTarget.id)
      notify(messages.deleted)
      if (rows.length === 1 && list.page > 0) list.setPage(list.page - 1)
    } catch (error) {
      notify(getErrorMessage(error), 'error')
    } finally {
      setDeleteTarget(null)
    }
  }

  return {
    list,
    query,
    rows,
    totalCount: query.data?.totalCount ?? 0,
    formTarget,
    openCreate: () => setFormTarget({ mode: 'create' }),
    openEdit: (item: TEntity) => setFormTarget({ mode: 'edit', item }),
    closeForm,
    submit,
    isSubmitting: createMutation.isPending || updateMutation.isPending,
    deleteTarget,
    askDelete: setDeleteTarget,
    cancelDelete: () => setDeleteTarget(null),
    confirmDelete,
    isDeleting: deleteMutation.isPending,
  }
}
