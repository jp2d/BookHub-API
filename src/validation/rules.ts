export type FieldErrors<T> = Partial<Record<keyof T, string>>

export const required = (value: string, label: string): string | undefined =>
  value.trim() ? undefined : `${label} é obrigatório.`

export const maxLength = (value: string, max: number, label: string): string | undefined =>
  value.trim().length > max ? `${label} deve ter no máximo ${max} caracteres.` : undefined
