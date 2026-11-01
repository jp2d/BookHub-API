import { useState } from 'react'
import type { ListParams } from '@/api'

export const DEFAULT_PAGE_SIZE = 10

// Search + pagination state of a list screen. The UI page is 0-based (MUI); the API page is 1-based.
export function useListState(initialPageSize = DEFAULT_PAGE_SIZE) {
  const [search, setSearchValue] = useState('')
  const [page, setPage] = useState(0)
  const [pageSize, setPageSizeValue] = useState(initialPageSize)

  const params: ListParams = { search, page: page + 1, pageSize }

  return {
    search,
    page,
    pageSize,
    params,
    setPage,
    setSearch: (value: string) => {
      setSearchValue(value)
      setPage(0)
    },
    setPageSize: (value: number) => {
      setPageSizeValue(value)
      setPage(0)
    },
  }
}
