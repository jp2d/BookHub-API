import { useNavigate } from 'react-router-dom'
import { genresApi, type Genre, type GenreRequest } from '@/api'
import { useCrudPage } from '@/hooks'
import { booksPath } from '@/routes'

export function useGenresPage() {
  const navigate = useNavigate()

  const crud = useCrudPage<Genre, GenreRequest>({
    hooks: genresApi,
    messages: {
      created: 'Gênero criado com sucesso.',
      updated: 'Gênero atualizado com sucesso.',
      deleted: 'Gênero excluído com sucesso.',
    },
  })

  return {
    ...crud,
    viewBooks: (genre: Genre) => navigate(booksPath({ genreId: genre.id })),
  }
}
