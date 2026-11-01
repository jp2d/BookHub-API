import type { ReactElement } from 'react'
import { describe, expect, it } from 'vitest'
import { screen, waitFor, within } from '@testing-library/react'
import { LocationProbe } from './LocationProbe'
import { json, mockFetch, noContent, paged, problem, type ApiHandler } from './mockApi'
import { renderWithProviders } from './renderWithProviders'

interface NamedItem {
  id: string
  name: string
}

export interface NameCrudSuiteConfig {
  suiteName: string
  Page: () => ReactElement
  apiPath: string
  items: [NamedItem, NamedItem]
  searchLabel: string
  booksFilterParam: 'genreId' | 'authorId'
  texts: {
    newButton: string
    dialogTitleNew: string
    dialogTitleEdit: string
    fieldLabel: string
    required: string
    created: string
    updated: string
    deleted: string
    empty: string
    deleteMessage: (name: string) => RegExp
    viewBooksLabel: (name: string) => string
  }
  // What the API answers when it rejects a creation (duplicate genre, invalid name...).
  creationRejection: { status: number; title: string; detail: string }
  // What the API answers when the item still has books.
  deletionRefusal: string
}

// The genres and authors screens only differ in texts and endpoint (both use the same generic blocks),
// so one suite describes the behaviour the user expects from a "name-only CRUD" screen.
export function describeNameCrudScreen(config: NameCrudSuiteConfig) {
  const { Page, apiPath, items, texts } = config
  const [first, second] = items

  function setupApi(override?: ApiHandler, list: NamedItem[] = items, totalCount = list.length) {
    return mockFetch((request) => {
      const overridden = override?.(request)
      if (overridden) return overridden

      if (request.method === 'GET' && request.url.pathname === apiPath) {
        return json({ ...paged(list), totalCount, totalPages: Math.ceil(totalCount / 10) })
      }
      return undefined
    })
  }

  const renderPage = () =>
    renderWithProviders(
      <>
        <Page />
        <LocationProbe />
      </>,
    )

  describe(config.suiteName, () => {
    it('lists the items', async () => {
      setupApi()
      renderPage()

      expect(await screen.findByText(first.name)).toBeInTheDocument()
      expect(screen.getByText(second.name)).toBeInTheDocument()
    })

    it('shows a message when there is nothing to list', async () => {
      setupApi(undefined, [])
      renderPage()

      expect(await screen.findByText(texts.empty)).toBeInTheDocument()
    })

    it('searches on the server after the user stops typing', async () => {
      const requests = setupApi()
      const { user } = renderPage()
      await screen.findByText(first.name)

      await user.type(screen.getByLabelText(config.searchLabel), 'abc')

      await waitFor(() => expect(requests.some((r) => r.url.searchParams.get('search') === 'abc')).toBe(true))
    })

    it('asks the server for the next page', async () => {
      const requests = setupApi(undefined, items, 25)
      const { user } = renderPage()
      await screen.findByText(first.name)

      await user.click(screen.getByRole('button', { name: 'Go to next page' }))

      await waitFor(() => expect(requests.some((r) => r.url.searchParams.get('page') === '2')).toBe(true))
    })

    it('opens the booklist filtered by the item', async () => {
      setupApi()
      const { user } = renderPage()

      await user.click(await screen.findByRole('button', { name: texts.viewBooksLabel(first.name) }))

      expect(screen.getByTestId('location')).toHaveTextContent(`/books?${config.booksFilterParam}=${first.id}`)
    })

    describe('creating', () => {
      it('validates the form without calling the API', async () => {
        const requests = setupApi()
        const { user } = renderPage()
        await screen.findByText(first.name)

        await user.click(screen.getByRole('button', { name: texts.newButton }))
        await user.click(screen.getByRole('button', { name: 'Salvar' }))

        expect(await screen.findByText(texts.required)).toBeInTheDocument()
        expect(requests.some((r) => r.method === 'POST')).toBe(false)
      })

      it('sends the trimmed name and confirms success', async () => {
        const requests = setupApi((request) =>
          request.method === 'POST' && request.url.pathname === apiPath
            ? json({ id: 'new', name: 'Novo' }, 201)
            : undefined,
        )
        const { user } = renderPage()
        await screen.findByText(first.name)

        await user.click(screen.getByRole('button', { name: texts.newButton }))
        expect(screen.getByRole('dialog', { name: texts.dialogTitleNew })).toBeInTheDocument()
        await user.type(screen.getByLabelText(texts.fieldLabel), '  Novo  ')
        await user.click(screen.getByRole('button', { name: 'Salvar' }))

        expect(await screen.findByText(texts.created)).toBeInTheDocument()
        expect(requests.find((r) => r.method === 'POST')?.body).toEqual({ name: 'Novo' })
        await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument())
      })

      it('keeps the dialog open and shows the message when the API rejects the name', async () => {
        const { status, title, detail } = config.creationRejection
        setupApi((request) => (request.method === 'POST' ? problem(status, title, detail) : undefined))
        const { user } = renderPage()
        await screen.findByText(first.name)

        await user.click(screen.getByRole('button', { name: texts.newButton }))
        await user.type(screen.getByLabelText(texts.fieldLabel), first.name)
        await user.click(screen.getByRole('button', { name: 'Salvar' }))

        expect(await screen.findByText(detail)).toBeInTheDocument()
        expect(screen.getByRole('dialog')).toBeInTheDocument()
      })
    })

    describe('editing', () => {
      it('opens with the current name and sends the change', async () => {
        const requests = setupApi((request) =>
          request.method === 'PUT' && request.url.pathname === `${apiPath}/${first.id}`
            ? json({ id: first.id, name: 'Renomeado' })
            : undefined,
        )
        const { user } = renderPage()

        await user.click(await screen.findByRole('button', { name: `Editar ${first.name}` }))
        expect(screen.getByRole('dialog', { name: texts.dialogTitleEdit })).toBeInTheDocument()
        const field = screen.getByLabelText(texts.fieldLabel)
        expect(field).toHaveValue(first.name)

        await user.clear(field)
        await user.type(field, 'Renomeado')
        await user.click(screen.getByRole('button', { name: 'Salvar' }))

        expect(await screen.findByText(texts.updated)).toBeInTheDocument()
        expect(requests.find((r) => r.method === 'PUT')?.body).toEqual({ name: 'Renomeado' })
      })
    })

    describe('deleting', () => {
      it('asks for confirmation and then deletes', async () => {
        const requests = setupApi((request) =>
          request.method === 'DELETE' && request.url.pathname === `${apiPath}/${first.id}` ? noContent() : undefined,
        )
        const { user } = renderPage()

        await user.click(await screen.findByRole('button', { name: `Excluir ${first.name}` }))
        const dialog = await screen.findByRole('dialog')
        expect(within(dialog).getByText(texts.deleteMessage(first.name))).toBeInTheDocument()
        await user.click(within(dialog).getByRole('button', { name: 'Excluir' }))

        expect(await screen.findByText(texts.deleted)).toBeInTheDocument()
        expect(requests.some((r) => r.method === 'DELETE' && r.url.pathname === `${apiPath}/${first.id}`)).toBe(true)
      })

      it('does nothing when the user cancels', async () => {
        const requests = setupApi()
        const { user } = renderPage()

        await user.click(await screen.findByRole('button', { name: `Excluir ${first.name}` }))
        await user.click(within(await screen.findByRole('dialog')).getByRole('button', { name: 'Cancelar' }))

        expect(requests.some((r) => r.method === 'DELETE')).toBe(false)
      })

      it('shows the server message when the item still has books', async () => {
        setupApi((request) =>
          request.method === 'DELETE' ? problem(409, 'Conflito.', config.deletionRefusal) : undefined,
        )
        const { user } = renderPage()

        await user.click(await screen.findByRole('button', { name: `Excluir ${first.name}` }))
        await user.click(within(await screen.findByRole('dialog')).getByRole('button', { name: 'Excluir' }))

        expect(await screen.findByText(config.deletionRefusal)).toBeInTheDocument()
      })
    })
  })
}
