import { useState } from 'react'
import { getErrorMessage, type CrudHooks } from '@/api'
import { useNotify } from '@/components/Notification'

export type FormTarget<T> = { mode: 'closed' } | { mode: 'create' } | { mode: 'edit'; item: T }

interface CrudMessages {
  created: string
  updated: string
  deleted: string
}

// What the CRUD page needs from the screen's list state (search/filters/pagination live in the screen).
export interface CrudListState<TParams> {
  params: TParams
  page: number // 0-based
  setPage: (page: number) => void
}

interface UseCrudPageOptions<TEntity, TRequest, TParams extends object, TList extends CrudListState<TParams>> {
  hooks: CrudHooks<TEntity, TRequest, TParams>
  list: TList
  messages: CrudMessages
}

// Everything a CRUD list screen needs besides its markup: queries, form/confirm dialogs and
// mutations with user feedback. Genres, authors and books configure it instead of repeating it.
// All type arguments are inferred from `hooks` and `list`, so `list` keeps the screen's own type.
export function useCrudPage<
  TEntity extends { id: string },
  TRequest,
  TParams extends object,
  TList extends CrudListState<TParams>,
>({ hooks, list, messages }: UseCrudPageOptions<TEntity, TRequest, TParams, TList>) {
  const notify = useNotify()
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
