import { InputAdornment, TextField } from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import { useSearchField } from './useSearchField'
import styles from './SearchField.module.css'

interface SearchFieldProps {
  defaultValue?: string
  label?: string
  delay?: number
  onSearch: (value: string) => void
}

// Debounced input: the parent receives the text only after the user stops typing.
// To reset it from outside, change the component's `key`.
export function SearchField({ defaultValue = '', label = 'Buscar', delay = 400, onSearch }: SearchFieldProps) {
  const { text, handleChange } = useSearchField({ defaultValue, delay, onSearch })

  return (
    <TextField
      className={styles.field}
      size="small"
      label={label}
      value={text}
      onChange={handleChange}
      slotProps={{
        input: {
          startAdornment: (
            <InputAdornment position="start">
              <SearchIcon fontSize="small" />
            </InputAdornment>
          ),
        },
      }}
    />
  )
}
