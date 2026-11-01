import { useEffect, useEffectEvent, useRef, useState, type ChangeEvent } from 'react'
import { useDebouncedValue } from '@/hooks/useDebouncedValue'

interface UseSearchFieldOptions {
  defaultValue: string
  delay: number
  onSearch: (value: string) => void
}

export function useSearchField({ defaultValue, delay, onSearch }: UseSearchFieldOptions) {
  const [text, setText] = useState(defaultValue)
  const debounced = useDebouncedValue(text, delay)
  const lastEmitted = useRef(defaultValue)
  const emit = useEffectEvent(onSearch)

  useEffect(() => {
    if (debounced === lastEmitted.current) return
    lastEmitted.current = debounced
    emit(debounced)
  }, [debounced])

  return {
    text,
    handleChange: (event: ChangeEvent<HTMLInputElement>) => setText(event.target.value),
  }
}
