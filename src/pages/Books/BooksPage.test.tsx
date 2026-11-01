import { describe, expect, it } from 'vitest'
import { screen, waitFor, within } from '@testing-library/react'
import type { Author, Book, Genre } from '@/api'
import { json, mockFetch, noContent, paged, problem, type ApiHandler } from '@/test/mockApi'
import { renderWithProviders } from '@/test/renderWithProviders'
import { BooksPage } from './BooksPage'

const orwell: Author = { id: 'a1', name: 'George Orwell' }
const asimov: Author = { id: 'a2', name: 'Isaac Asimov' }
const dystopia: Genre = { id: 'g1', name: 'Distopia' }
const sciFi: Genre = { id: 'g2', name: 'Ficção Científica' }

const book1984: Book = {
  id: 'b1',
  title: '1984',
  publicationYear: 1949,
  isbn: '9780451524935',
  authorId: orwell.id,
  authorName: orwell.name,
  genreId: dystopia.id,
  genreName: dystopia.name,
}
const bookFoundation: Book = {
  id: 'b2',
  title: 'Fundação',
  publicationYear: 1951,
  isbn: null,
  authorId: asimov.id,
  authorName: asimov.name,
  genreId: sciFi.id,
  genreName: sciFi.name,
}

// Default API: two books, two authors, two genres. A test overrides only what it cares about.
function setupApi(override?: ApiHandler, books: Book[] = [book1984, bookFoundation]) {
  return mockFetch((request) => {
    const overridden = override?.(request)
    if (overridden) return overridden

    const { pathname } = request.url
    if (request.method === 'GET' && pathname === '/api/books') return json(paged(books))
    if (request.method === 'GET' && pathname === '/api/authors') return json(paged([orwell, asimov]))
    if (request.method === 'GET' && pathname === '/api/genres') return json(paged([dystopia, sciFi]))
    if (request.method === 'GET' && pathname === `/api/authors/${orwell.id}`) return json(orwell)
    return undefined
  })
}

describe('BooksPage', () => {
  it('lists the books with their author and genre', async () => {
    setupApi()
    renderWithProviders(<BooksPage />)

    const row = (await screen.findByText('1984')).closest('tr')!
    expect(within(row).getByText('1949')).toBeInTheDocument()
    expect(within(row).getByText('9780451524935')).toBeInTheDocument()
    expect(within(row).getByText('George Orwell')).toBeInTheDocument()
    expect(within(row).getByText('Distopia')).toBeInTheDocument()

    const otherRow = screen.getByText('Fundação').closest('tr')!
    expect(within(otherRow).getByText('—')).toBeInTheDocument() // book without ISBN
  })

  it('shows a message when there are no books', async () => {
    setupApi(undefined, [])
    renderWithProviders(<BooksPage />)

    expect(await screen.findByText('Nenhum livro encontrado.')).toBeInTheDocument()
  })

  it('shows the API error and lets the user retry', async () => {
    let failing = true
    setupApi((request) =>
      failing && request.url.pathname === '/api/books' ? problem(500, 'Erro interno do servidor.') : undefined,
    )
    const { user } = renderWithProviders(<BooksPage />)

    expect(await screen.findByText('Erro interno do servidor.')).toBeInTheDocument()

    failing = false
    await user.click(screen.getByRole('button', { name: 'Tentar de novo' }))

    expect(await screen.findByText('1984')).toBeInTheDocument()
  })

  it('filters by author when its chip is clicked', async () => {
    const requests = setupApi()
    const { user } = renderWithProviders(<BooksPage />)

    await user.click(await screen.findByRole('button', { name: 'Filtrar pelo autor George Orwell' }))

    expect(await screen.findByText('Autor: George Orwell')).toBeInTheDocument()
    await waitFor(() =>
      expect(requests.some((r) => r.url.pathname === '/api/books' && r.url.searchParams.get('authorId') === orwell.id)).toBe(true),
    )
  })

  it('starts already filtered when the URL has a filter', async () => {
    const requests = setupApi()
    renderWithProviders(<BooksPage />, `/books?authorId=${orwell.id}`)

    expect(await screen.findByText('Autor: George Orwell')).toBeInTheDocument()
    expect(requests.find((r) => r.url.pathname === '/api/books')?.url.searchParams.get('authorId')).toBe(orwell.id)
  })

  describe('creating a book', () => {
    it('validates the form without calling the API', async () => {
      const requests = setupApi()
      const { user } = renderWithProviders(<BooksPage />)
      await screen.findByText('1984')

      await user.click(screen.getByRole('button', { name: 'Novo livro' }))
      await user.click(screen.getByRole('button', { name: 'Salvar' }))

      expect(await screen.findByText('O título é obrigatório.')).toBeInTheDocument()
      expect(screen.getByText('O ano de publicação é obrigatório.')).toBeInTheDocument()
      expect(screen.getByText('Selecione um autor.')).toBeInTheDocument()
      expect(screen.getByText('Selecione um gênero.')).toBeInTheDocument()
      expect(requests.some((r) => r.method === 'POST')).toBe(false)
    })

    async function fillValidForm(user: ReturnType<typeof renderWithProviders>['user']) {
      await user.click(screen.getByRole('button', { name: 'Novo livro' }))
      await user.type(screen.getByLabelText('Título'), 'Duna')
      await user.type(screen.getByLabelText('Ano de publicação'), '1965')
      await user.type(screen.getByLabelText('ISBN (opcional)'), '978-0-441-17271-9')

      await user.click(screen.getByLabelText('Autor'))
      await user.click(await screen.findByRole('option', { name: 'Isaac Asimov' }))
      await user.click(screen.getByLabelText('Gênero'))
      await user.click(await screen.findByRole('option', { name: 'Ficção Científica' }))
    }

    it('sends the normalized payload and confirms success', async () => {
      const requests = setupApi((request) =>
        request.method === 'POST' && request.url.pathname === '/api/books' ? json({ ...bookFoundation, title: 'Duna' }, 201) : undefined,
      )
      const { user } = renderWithProviders(<BooksPage />)
      await screen.findByText('1984')

      await fillValidForm(user)
      await user.click(screen.getByRole('button', { name: 'Salvar' }))

      expect(await screen.findByText('Livro criado com sucesso.')).toBeInTheDocument()
      expect(requests.find((r) => r.method === 'POST')?.body).toEqual({
        title: 'Duna',
        publicationYear: 1965,
        isbn: '9780441172719',
        authorId: asimov.id,
        genreId: sciFi.id,
      })
      await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument())
    })

    it('keeps the dialog open and shows the server error when the API rejects the book', async () => {
      setupApi((request) =>
        request.method === 'POST' && request.url.pathname === '/api/books'
          ? problem(400, 'Requisição inválida.', 'O ano de publicação deve estar entre 1 e 2026.')
          : undefined,
      )
      const { user } = renderWithProviders(<BooksPage />)
      await screen.findByText('1984')

      await fillValidForm(user)
      await user.click(screen.getByRole('button', { name: 'Salvar' }))

      expect(await screen.findByText('O ano de publicação deve estar entre 1 e 2026.')).toBeInTheDocument()
      expect(screen.getByRole('dialog')).toBeInTheDocument()
    })
  })

  describe('deleting a book', () => {
    it('asks for confirmation and then deletes', async () => {
      const requests = setupApi((request) =>
        request.method === 'DELETE' && request.url.pathname === `/api/books/${book1984.id}` ? noContent() : undefined,
      )
      const { user } = renderWithProviders(<BooksPage />)

      await user.click(await screen.findByRole('button', { name: 'Excluir 1984' }))
      const dialog = await screen.findByRole('dialog')
      expect(within(dialog).getByText(/Deseja excluir o livro "1984"/)).toBeInTheDocument()

      await user.click(within(dialog).getByRole('button', { name: 'Excluir' }))

      expect(await screen.findByText('Livro excluído com sucesso.')).toBeInTheDocument()
      expect(requests.some((r) => r.method === 'DELETE' && r.url.pathname === `/api/books/${book1984.id}`)).toBe(true)
    })

    it('does not delete when the user cancels', async () => {
      const requests = setupApi()
      const { user } = renderWithProviders(<BooksPage />)

      await user.click(await screen.findByRole('button', { name: 'Excluir 1984' }))
      await user.click(within(await screen.findByRole('dialog')).getByRole('button', { name: 'Cancelar' }))

      expect(requests.some((r) => r.method === 'DELETE')).toBe(false)
    })

    it('shows the server message when the deletion is refused', async () => {
      setupApi((request) =>
        request.method === 'DELETE' ? problem(409, 'Conflito.', 'A operação viola um relacionamento existente.') : undefined,
      )
      const { user } = renderWithProviders(<BooksPage />)

      await user.click(await screen.findByRole('button', { name: 'Excluir 1984' }))
      await user.click(within(await screen.findByRole('dialog')).getByRole('button', { name: 'Excluir' }))

      expect(await screen.findByText('A operação viola um relacionamento existente.')).toBeInTheDocument()
    })
  })
})
