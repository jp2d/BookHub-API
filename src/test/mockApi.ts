import { vi } from 'vitest'

export interface RecordedRequest {
  url: URL
  method: string
  body: unknown
}

export type ApiHandler = (request: RecordedRequest) => Response | Promise<Response> | undefined

export const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } })

export const noContent = () => new Response(null, { status: 204 })

export const problem = (status: number, title: string, detail?: string) =>
  new Response(JSON.stringify({ title, detail, status }), {
    status,
    headers: { 'Content-Type': 'application/problem+json' },
  })

export const paged = <T>(items: T[]) => ({ items, page: 1, pageSize: 10, totalCount: items.length, totalPages: 1 })

// Replaces `fetch` with a handler and records every request, so tests can assert what the UI sent.
export function mockFetch(handler: ApiHandler) {
  const requests: RecordedRequest[] = []

  vi.stubGlobal(
    'fetch',
    vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
      const request: RecordedRequest = {
        url: new URL(typeof input === 'string' ? input : input.toString()),
        method: init?.method ?? 'GET',
        body: init?.body ? JSON.parse(String(init.body)) : undefined,
      }
      requests.push(request)

      const response = await handler(request)
      if (!response) throw new Error(`Unhandled request: ${request.method} ${request.url}`)
      return response
    }),
  )

  return requests
}
