// Utilities
import type { Cliente, ContaCorrente, NovaConta, NovoCliente, Transferencia } from '@/api'
import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { ApiError, clientesApi, contasApi } from '@/api'
import { useNotificacoesStore } from './notificacoes'

/**
 * Estado compartilhado de clientes e contas. Toda operação que altera dados recarrega as duas listas,
 * porque elas se afetam (ex.: cadastrar cliente pode abrir conta; remover cliente libera a conta).
 */
export const useBancoStore = defineStore('banco', () => {
  const notificacoes = useNotificacoesStore()

  const clientes = ref<Cliente[]>([])
  const contas = ref<ContaCorrente[]>([])
  const carregando = ref(false)
  const erroCarregamento = ref<string | null>(null)

  const clientesAtivos = computed(() => clientes.value.filter(c => c.ativo).length)
  const contasAtivas = computed(() => contas.value.filter(c => c.ativa).length)
  const saldoTotal = computed(() => contas.value.reduce((total, c) => total + c.saldo, 0))
  const contasSemTitular = computed(() => contas.value.filter(c => c.titularId === null))

  async function carregar () {
    carregando.value = true
    try {
      const [listaClientes, listaContas] = await Promise.all([clientesApi.listar(), contasApi.listar()])
      clientes.value = listaClientes
      contas.value = listaContas
      erroCarregamento.value = null
    } catch (error) {
      erroCarregamento.value = error instanceof ApiError ? error.message : 'Não foi possível carregar os dados.'
    } finally {
      carregando.value = false
    }
  }

  /** Executa a operação, notifica o resultado e recarrega os dados. Em caso de erro, retorna `undefined`. */
  async function executar<T> (operacao: () => Promise<T>, aoConcluir?: (resultado: T) => void): Promise<T | undefined> {
    try {
      const resultado = await operacao()
      aoConcluir?.(resultado)
      await carregar()
      return resultado
    } catch (error) {
      notificacoes.erro(error)
      return undefined
    }
  }

  function notificaOperacao (r: { alterado: boolean, mensagem: string }) {
    if (r.alterado) {
      notificacoes.sucesso(r.mensagem)
    } else {
      notificacoes.info(r.mensagem)
    }
  }

  // Clientes

  const cadastrarCliente = (novo: NovoCliente) => executar(() => clientesApi.cadastrar(novo), c =>
    notificacoes.sucesso(`Cliente ${c.nome} cadastrado com sucesso (conta ${c.contaCorrenteId}).`))

  const ativarCliente = (id: number) => executar(() => clientesApi.ativar(id), notificaOperacao)

  const desativarCliente = (id: number) => executar(() => clientesApi.desativar(id), notificaOperacao)

  const removerCliente = (id: number) => executar(() => clientesApi.remover(id), () =>
    notificacoes.sucesso('Cliente removido com sucesso! A conta corrente foi mantida.'))

  const limparClientes = () => executar(() => clientesApi.limpar(), r =>
    notificacoes.sucesso(`${r.mensagem} (${r.removidos} removido(s))`))

  // Contas

  const cadastrarConta = (nova: NovaConta) => executar(() => contasApi.cadastrar(nova), c =>
    notificacoes.sucesso(`Conta ${c.id} cadastrada com sucesso!`))

  const ativarConta = (id: number) => executar(() => contasApi.ativar(id), notificaOperacao)

  const desativarConta = (id: number) => executar(() => contasApi.desativar(id), notificaOperacao)

  const removerConta = (id: number) => executar(() => contasApi.remover(id), () =>
    notificacoes.sucesso('Conta removida com sucesso!'))

  const transferir = (transferencia: Transferencia) => executar(() => contasApi.transferir(transferencia), r =>
    notificacoes.sucesso(r.mensagem))

  return {
    clientes,
    contas,
    carregando,
    erroCarregamento,
    clientesAtivos,
    contasAtivas,
    saldoTotal,
    contasSemTitular,
    carregar,
    cadastrarCliente,
    ativarCliente,
    desativarCliente,
    removerCliente,
    limparClientes,
    cadastrarConta,
    ativarConta,
    desativarConta,
    removerConta,
    transferir,
  }
})
