import { useNavigate } from 'react-router-dom'
import { authorsApi, type Author, type AuthorRequest } from '@/api'
import { useCrudPage } from '@/hooks'
import { booksPath } from '@/routes'

export function useAuthorsPage() {
  const navigate = useNavigate()

  const crud = useCrudPage<Author, AuthorRequest>({
    hooks: authorsApi,
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
