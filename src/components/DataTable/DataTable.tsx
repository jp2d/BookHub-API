import type { ReactNode } from 'react'
import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TableSortLabel,
} from '@mui/material'
import styles from './DataTable.module.css'

export interface DataTableColumn<T> {
  key: string // also the sort key when the column is sortable
  header: string
  render: (row: T) => ReactNode
  align?: 'left' | 'right' | 'center'
  sortable?: boolean
}

export interface DataTablePagination {
  page: number // 0-based
  pageSize: number
  totalCount: number
  onPageChange: (page: number) => void
  onPageSizeChange: (pageSize: number) => void
}

export interface DataTableSorting {
  key: string
  descending: boolean
  onSort: (key: string) => void
}

interface DataTableProps<T> {
  columns: DataTableColumn<T>[]
  rows: T[]
  getRowKey: (row: T) => string
  renderActions?: (row: T) => ReactNode
  pagination: DataTablePagination
  sorting?: DataTableSorting
}

const PAGE_SIZE_OPTIONS = [5, 10, 25, 50]

// Generic table with server-side pagination and optional sorting. Each screen only describes its columns.
export function DataTable<T>({ columns, rows, getRowKey, renderActions, pagination, sorting }: DataTableProps<T>) {
  return (
    <Paper className={styles.paper}>
      <TableContainer>
        <Table>
          <TableHead>
            <TableRow>
              {columns.map((column) => (
                <TableCell
                  key={column.key}
                  align={column.align}
                  sortDirection={sorting?.key === column.key ? (sorting.descending ? 'desc' : 'asc') : false}
                >
                  {column.sortable && sorting ? (
                    <TableSortLabel
                      active={sorting.key === column.key}
                      direction={sorting.descending ? 'desc' : 'asc'}
                      onClick={() => sorting.onSort(column.key)}
                    >
                      {column.header}
                    </TableSortLabel>
                  ) : (
                    column.header
                  )}
                </TableCell>
              ))}
              {renderActions && <TableCell align="right">Ações</TableCell>}
            </TableRow>
          </TableHead>
          <TableBody>
            {rows.map((row) => (
              <TableRow key={getRowKey(row)} hover>
                {columns.map((column) => (
                  <TableCell key={column.key} align={column.align}>
                    {column.render(row)}
                  </TableCell>
                ))}
                {renderActions && (
                  <TableCell align="right" className={styles.actions}>
                    {renderActions(row)}
                  </TableCell>
                )}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
      <TablePagination
        component="div"
        count={pagination.totalCount}
        page={pagination.page}
        rowsPerPage={pagination.pageSize}
        rowsPerPageOptions={PAGE_SIZE_OPTIONS}
        onPageChange={(_, page) => pagination.onPageChange(page)}
        onRowsPerPageChange={(event) => pagination.onPageSizeChange(Number(event.target.value))}
        labelRowsPerPage="Itens por página:"
        labelDisplayedRows={({ from, to, count }) => `${from}–${to} de ${count}`}
      />
    </Paper>
  )
}
