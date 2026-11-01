import { useNavigate } from 'react-router-dom'
import { authorsApi, type Author } from '@/api'
import { useCrudPage, useListState } from '@/hooks'
import { booksPath } from '@/routes'

export function useAuthorsPage() {
  const navigate = useNavigate()

  const crud = useCrudPage({
    hooks: authorsApi,
    list: useListState(),
    messages: {
      created: 'Autor criado com sucesso.',
      updated: 'Autor atualizado com sucesso.',
      deleted: 'Autor excluído com sucesso.',
    },
  })

  return {
    ...crud,
    viewBooks: (author: Author) => navigate(booksPath({ authorId: author.id })),
  }
}
