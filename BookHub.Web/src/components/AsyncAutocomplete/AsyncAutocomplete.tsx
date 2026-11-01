import { Autocomplete, CircularProgress, TextField } from '@mui/material'
import { useAsyncAutocomplete, type UseListQuery } from './useAsyncAutocomplete'

interface AsyncAutocompleteProps<T extends { id: string }> {
  label: string
  value: T | null
  onChange: (value: T | null) => void
  useList: UseListQuery<T> // a CRUD list hook, e.g. authorsApi.useList
  getOptionLabel: (option: T) => string
  error?: string
  disabled?: boolean
}

// Select-one field whose options come from the API as the user types (works for any entity with an id).
export function AsyncAutocomplete<T extends { id: string }>({
  label,
  value,
  onChange,
  useList,
  getOptionLabel,
  error,
  disabled,
}: AsyncAutocompleteProps<T>) {
  const { options, loading, onInputChange } = useAsyncAutocomplete({ useList, value })

  return (
    <Autocomplete
      options={options}
      value={value}
      onChange={(_, selected) => onChange(selected)}
      onInputChange={(_, text, reason) => onInputChange(text, reason)}
      getOptionLabel={getOptionLabel}
      isOptionEqualToValue={(option, selected) => option.id === selected.id}
      filterOptions={(items) => items} // filtering is done by the server
      loading={loading}
      disabled={disabled}
      loadingText="Carregando..."
      noOptionsText="Nenhum resultado"
      renderInput={(params) => (
        <TextField
          {...params}
          label={label}
          error={Boolean(error)}
          helperText={error}
          slotProps={{
            ...params.slotProps,
            input: {
              ...params.slotProps.input,
              endAdornment: (
                <>
                  {loading ? <CircularProgress color="inherit" size={18} /> : null}
                  {params.slotProps.input.endAdornment}
                </>
              ),
            },
          }}
        />
      )}
    />
  )
}
