import { ApiError, parseApiError } from './ApiError'
import { buildQuery } from './buildQuery'
import { API_URL } from './config'

interface RequestOptions {
  body?: unknown
  query?: object
}

async function request<T>(method: string, path: string, { body, query }: RequestOptions = {}): Promise<T> {
  let response: Response

  try {
    response = await fetch(`${API_URL}${path}${buildQuery(query)}`, {
      method,
      headers: body === undefined ? undefined : { 'Content-Type': 'application/json' },
      body: body === undefined ? undefined : JSON.stringify(body),
    })
  } catch {
    throw new ApiError(0, 'Sem conexão', 'Não foi possível conectar à API. Verifique se ela está em execução.')
  }

  if (!response.ok) throw await parseApiError(response)
  if (response.status === 204) return undefined as T

  return (await response.json()) as T
}

export const api = {
  get: <T>(path: string, query?: object) => request<T>('GET', path, { query }),
  post: <T>(path: string, body: unknown) => request<T>('POST', path, { body }),
  put: <T>(path: string, body: unknown) => request<T>('PUT', path, { body }),
  delete: (path: string) => request<void>('DELETE', path),
}
