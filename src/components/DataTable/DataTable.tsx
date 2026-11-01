import type { ReactNode } from 'react'
import { Paper, Table, TableBody, TableCell, TableContainer, TableHead, TablePagination, TableRow } from '@mui/material'
import styles from './DataTable.module.css'

export interface DataTableColumn<T> {
  key: string
  header: string
  render: (row: T) => ReactNode
  align?: 'left' | 'right' | 'center'
}

export interface DataTablePagination {
  page: number // 0-based
  pageSize: number
  totalCount: number
  onPageChange: (page: number) => void
  onPageSizeChange: (pageSize: number) => void
}

interface DataTableProps<T> {
  columns: DataTableColumn<T>[]
  rows: T[]
  getRowKey: (row: T) => string
  renderActions?: (row: T) => ReactNode
  pagination: DataTablePagination
}

const PAGE_SIZE_OPTIONS = [5, 10, 25, 50]

// Generic table with server-side pagination. Each screen only describes its columns.
export function DataTable<T>({ columns, rows, getRowKey, renderActions, pagination }: DataTableProps<T>) {
  return (
    <Paper className={styles.paper}>
      <TableContainer>
        <Table>
          <TableHead>
            <TableRow>
              {columns.map((column) => (
                <TableCell key={column.key} align={column.align}>
                  {column.header}
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
