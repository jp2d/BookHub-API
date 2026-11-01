import { useState, type ChangeEvent, type FormEvent } from 'react'
import type { NameValidator } from '@/validation'

interface UseNameFormOptions {
  initialName: string
  validate: NameValidator
  onSubmit: (name: string) => void
}

export function useNameForm({ initialName, validate, onSubmit }: UseNameFormOptions) {
  const [name, setName] = useState(initialName)
  const [error, setError] = useState<string | undefined>()

  const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
    setName(event.target.value)
    setError(undefined)
  }

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const validationError = validate(name)
    if (validationError) {
      setError(validationError)
      return
    }

    onSubmit(name.trim())
  }

  return { name, error, handleChange, handleSubmit }
}
