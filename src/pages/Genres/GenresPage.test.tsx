import { describeNameCrudScreen } from '@/test/nameCrudScreenSuite'
import { GenresPage } from './GenresPage'

describeNameCrudScreen({
  suiteName: 'GenresPage',
  Page: GenresPage,
  apiPath: '/api/genres',
  items: [
    { id: 'g1', name: 'Distopia' },
    { id: 'g2', name: 'Fantasia' },
  ],
  searchLabel: 'Buscar gênero',
  booksFilterParam: 'genreId',
  texts: {
    newButton: 'Novo gênero',
    dialogTitleNew: 'Novo gênero',
    dialogTitleEdit: 'Editar gênero',
    fieldLabel: 'Nome do gênero',
    required: 'O nome do gênero é obrigatório.',
    created: 'Gênero criado com sucesso.',
    updated: 'Gênero atualizado com sucesso.',
    deleted: 'Gênero excluído com sucesso.',
    empty: 'Nenhum gênero encontrado.',
    deleteMessage: (name) => new RegExp(`Deseja excluir o gênero "${name}"`),
    viewBooksLabel: (name) => `Ver livros do gênero ${name}`,
  },
  creationRejection: { status: 409, title: 'Conflito.', detail: "Já existe um gênero com o nome 'Distopia'." },
  deletionRefusal: 'Não é possível excluir um gênero que possui livros.',
})
