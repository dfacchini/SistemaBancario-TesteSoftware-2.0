/**
 * api/tipos.ts
 *
 * Contratos da API (espelham os records de Endpoints/Contratos.cs).
 */

export interface Cliente {
  id: number
  nome: string
  idade: number
  email: string
  ativo: boolean
  contaCorrenteId: number
}

/** Sem `contaCorrenteId`, a API abre uma nova conta com saldo zero para o cliente. */
export interface NovoCliente {
  nome: string
  idade: number
  email: string
  contaCorrenteId?: number | null
}

/** `alterado` é falso quando o cliente já estava na situação pedida (HU-02 / HU-03). */
export interface OperacaoClienteResposta {
  alterado: boolean
  mensagem: string
  cliente: Cliente
}

export interface LimpezaClientesResposta {
  mensagem: string
  removidos: number
}

export interface ContaCorrente {
  id: number
  saldo: number
  ativa: boolean
  titularId: number | null
  titularNome: string | null
}

export interface NovaConta {
  saldoInicial: number
}

export interface OperacaoContaResposta {
  alterado: boolean
  mensagem: string
  conta: ContaCorrente
}

export interface Transferencia {
  contaOrigemId: number
  contaDestinoId: number
  valor: number
}

export interface TransferenciaResposta {
  mensagem: string
  origem: ContaCorrente
  destino: ContaCorrente
}
