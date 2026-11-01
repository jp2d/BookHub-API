import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from './client'
import type { ListParams, PagedResult } from './types'

interface CrudHooksOptions {
  // Resources whose cached lists embed this one (e.g. a genre name shown in the books list).
  alsoInvalidate?: string[]
}

// Builds the TanStack Query hooks of a REST resource that follows the API's CRUD convention.
// Every entity (genres, authors, books) reuses it: only the types, path and options change.
export function createCrudHooks<TEntity, TRequest, TParams extends object = ListParams>(
  resource: string,
  { alsoInvalidate = [] }: CrudHooksOptions = {},
) {
  const path = `/api/${resource}`

  function useInvalidate() {
    const queryClient = useQueryClient()
    return () =>
      Promise.all([resource, ...alsoInvalidate].map((key) => queryClient.invalidateQueries({ queryKey: [key] })))
  }

  function useList(params: TParams) {
    return useQuery({
      queryKey: [resource, 'list', params],
      queryFn: () => api.get<PagedResult<TEntity>>(path, params),
      placeholderData: keepPreviousData,
    })
  }

  function useCreate() {
    const invalidate = useInvalidate()
    return useMutation({
      mutationFn: (body: TRequest) => api.post<TEntity>(path, body),
      onSuccess: invalidate,
    })
  }

  function useUpdate() {
    const invalidate = useInvalidate()
    return useMutation({
      mutationFn: ({ id, body }: { id: string; body: TRequest }) => api.put<TEntity>(`${path}/${id}`, body),
      onSuccess: invalidate,
    })
  }

  function useDelete() {
    const invalidate = useInvalidate()
    return useMutation({
      mutationFn: (id: string) => api.delete(`${path}/${id}`),
      onSuccess: invalidate,
    })
  }

  return { useList, useCreate, useUpdate, useDelete }
}

export type CrudHooks<TEntity, TRequest, TParams extends object = ListParams> = ReturnType<
  typeof createCrudHooks<TEntity, TRequest, TParams>
>
