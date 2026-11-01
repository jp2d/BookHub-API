import { describeNameCrudScreen } from '@/test/nameCrudScreenSuite'
import { AuthorsPage } from './AuthorsPage'

describeNameCrudScreen({
  suiteName: 'AuthorsPage',
  Page: AuthorsPage,
  apiPath: '/api/authors',
  items: [
    { id: 'a1', name: 'George Orwell' },
    { id: 'a2', name: 'Isaac Asimov' },
  ],
  searchLabel: 'Buscar autor',
  booksFilterParam: 'authorId',
  texts: {
    newButton: 'Novo autor',
    dialogTitleNew: 'Novo autor',
    dialogTitleEdit: 'Editar autor',
    fieldLabel: 'Nome do autor',
    required: 'O nome do autor é obrigatório.',
    created: 'Autor criado com sucesso.',
    updated: 'Autor atualizado com sucesso.',
    deleted: 'Autor excluído com sucesso.',
    empty: 'Nenhum autor encontrado.',
    deleteMessage: (name) => new RegExp(`Deseja excluir o autor "${name}"`),
    viewBooksLabel: (name) => `Ver livros de ${name}`,
  },
  creationRejection: { status: 400, title: 'Requisição inválida.', detail: 'O nome do autor é obrigatório.' },
  deletionRefusal: 'Não é possível excluir um autor que possui livros.',
})
