import { useState } from 'react'
import type { UseQueryResult } from '@tanstack/react-query'
import type { ListParams, PagedResult } from '@/api'
import { useDebouncedValue } from '@/hooks/useDebouncedValue'

export type UseListQuery<T> = (params: ListParams) => Pick<UseQueryResult<PagedResult<T>>, 'data' | 'isFetching'>

const SUGGESTIONS = 20

interface UseAsyncAutocompleteOptions<T extends { id: string }> {
  useList: UseListQuery<T>
  value: T | null
}

// Server-side search for an autocomplete: debounces what the user types and asks the API for matches.
export function useAsyncAutocomplete<T extends { id: string }>({ useList, value }: UseAsyncAutocompleteOptions<T>) {
  const [input, setInput] = useState('')
  const search = useDebouncedValue(input, 300)
  const query = useList({ search, pageSize: SUGGESTIONS })

  const fetched = query.data?.items ?? []
  // The selected item may not be among the fetched suggestions (e.g. when editing); keep it selectable.
  const options = value && !fetched.some((item) => item.id === value.id) ? [value, ...fetched] : fetched

  return {
    options,
    loading: query.isFetching,
    onInputChange: (text: string, reason: string) => {
      if (reason === 'input') setInput(text)
      else if (reason === 'clear') setInput('')
    },
  }
}
