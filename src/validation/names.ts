import { LIMITS } from './limits'
import { maxLength, required } from './rules'

export type NameValidator = (value: string) => string | undefined

export const nameValidator =
  (label: string, max: number): NameValidator =>
  (value) =>
    required(value, label) ?? maxLength(value, max, label)

export const validateGenreName = nameValidator('O nome do gênero', LIMITS.genreName)
export const validateAuthorName = nameValidator('O nome do autor', LIMITS.authorName)
