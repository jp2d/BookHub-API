import { genresApi, type Genre, type GenreRequest } from '@/api'
import { useCrudPage } from '@/hooks'

export function useGenresPage() {
  return useCrudPage<Genre, GenreRequest>({
    hooks: genresApi,
    messages: {
      created: 'Gênero criado com sucesso.',
      updated: 'Gênero atualizado com sucesso.',
      deleted: 'Gênero excluído com sucesso.',
    },
  })
}
