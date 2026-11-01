export class ApiError extends Error {
  status: number
  title: string
  detail?: string

  constructor(status: number, title: string, detail?: string) {
    super(detail ?? title)
    this.name = 'ApiError'
    this.status = status
    this.title = title
    this.detail = detail
  }
}

interface ProblemDetails {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

export async function parseApiError(response: Response): Promise<ApiError> {
  let problem: ProblemDetails | undefined

  try {
    problem = (await response.json()) as ProblemDetails
  } catch {
    // The body is not JSON (for example an empty 404): fall back to the status text.
  }

  const validation = problem?.errors ? Object.values(problem.errors).flat().join(' ') : undefined
  const title = problem?.title ?? response.statusText ?? 'Erro na requisição'

  return new ApiError(response.status, title, problem?.detail ?? validation)
}

export function getErrorMessage(error: unknown, fallback = 'Ocorreu um erro inesperado.'): string {
  return error instanceof ApiError ? (error.detail ?? error.title) : fallback
}
