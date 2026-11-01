import { describe, expect, it } from 'vitest'
import { json, mockFetch, noContent, problem } from '@/test/mockApi'
import { ApiError, getErrorMessage } from './ApiError'
import { api } from './client'

describe('api client', () => {
  it('sends GET requests to the API with the query string', async () => {
    const requests = mockFetch(() => json({ items: [] }))

    await api.get('/api/books', { search: 'duna', page: 2 })

    expect(requests).toHaveLength(1)
    expect(requests[0].method).toBe('GET')
    expect(requests[0].url.pathname).toBe('/api/books')
    expect(requests[0].url.searchParams.get('search')).toBe('duna')
    expect(requests[0].url.searchParams.get('page')).toBe('2')
  })

  it('sends the body as JSON on POST and PUT', async () => {
    const requests = mockFetch(() => json({ id: '1' }))

    await api.post('/api/genres', { name: 'Terror' })
    await api.put('/api/genres/1', { name: 'Suspense' })

    expect(requests.map((request) => [request.method, request.body])).toEqual([
      ['POST', { name: 'Terror' }],
      ['PUT', { name: 'Suspense' }],
    ])
  })

  it('resolves to undefined on 204 No Content', async () => {
    mockFetch(() => noContent())

    await expect(api.delete('/api/genres/1')).resolves.toBeUndefined()
  })

  it('turns a problem+json response into an ApiError', async () => {
    mockFetch(() => problem(409, 'Conflito.', "Já existe um gênero com o nome 'Terror'."))

    const error = await api.post('/api/genres', { name: 'Terror' }).catch((e: unknown) => e)

    expect(error).toBeInstanceOf(ApiError)
    expect(error).toMatchObject({ status: 409, title: 'Conflito.', detail: "Já existe um gênero com o nome 'Terror'." })
  })

  it('joins ASP.NET validation errors when there is no detail', async () => {
    mockFetch(() =>
      json({ title: 'One or more validation errors occurred.', errors: { name: ['Obrigatório.', 'Curto.'] } }, 400),
    )

    const error = await api.post('/api/genres', {}).catch((e: unknown) => e)

    expect(error).toMatchObject({ status: 400, detail: 'Obrigatório. Curto.' })
  })

  it('falls back to the status text when the error body is not JSON', async () => {
    mockFetch(() => new Response('', { status: 404, statusText: 'Not Found' }))

    const error = await api.get('/api/genres/x').catch((e: unknown) => e)

    expect(error).toMatchObject({ status: 404, title: 'Not Found' })
  })

  it('reports a friendly error when the API is unreachable', async () => {
    mockFetch(() => {
      throw new TypeError('Failed to fetch')
    })

    const error = await api.get('/api/genres').catch((e: unknown) => e)

    expect(error).toMatchObject({ status: 0, title: 'Sem conexão' })
  })
})

describe('getErrorMessage', () => {
  it('prefers the detail of an ApiError, then its title', () => {
    expect(getErrorMessage(new ApiError(400, 'Título', 'Detalhe'))).toBe('Detalhe')
    expect(getErrorMessage(new ApiError(500, 'Título'))).toBe('Título')
  })

  it('uses the fallback for any other error', () => {
    expect(getErrorMessage(new Error('boom'), 'Falhou.')).toBe('Falhou.')
    expect(getErrorMessage('x')).toBe('Ocorreu um erro inesperado.')
  })
})
