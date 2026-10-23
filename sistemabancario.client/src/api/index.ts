/**
 * api/index.ts
 *
 * Chamadas à API agrupadas por recurso. Os comentários RF-xx referem-se a requisitos-funcionais.md.
 */

import type {
  Cliente,
  ContaCorrente,
  LimpezaClientesResposta,
  NovaConta,
  NovoCliente,
  OperacaoClienteResposta,
  OperacaoContaResposta,
  Transferencia,
  TransferenciaResposta,
} from './tipos'
import { http } from './http'

export const clientesApi = {
  listar: () => http.get<Cliente[]>('/api/clientes'), // RF-09
  consultar: (id: number) => http.get<Cliente>(`/api/clientes/${id}`), // RF-01
  cadastrar: (cliente: NovoCliente) => http.post<Cliente>('/api/clientes', cliente), // RF-05 / RF-06
  ativar: (id: number) => http.post<OperacaoClienteResposta>(`/api/clientes/${id}/ativar`), // RF-02
  desativar: (id: number) => http.post<OperacaoClienteResposta>(`/api/clientes/${id}/desativar`), // RF-03
  remover: (id: number) => http.delete(`/api/clientes/${id}`), // RF-07
  limpar: () => http.delete<LimpezaClientesResposta>('/api/clientes'), // RF-08
}

export const contasApi = {
  listar: () => http.get<ContaCorrente[]>('/api/contas'), // RF-14
  consultar: (id: number) => http.get<ContaCorrente>(`/api/contas/${id}`), // RF-10
  cadastrar: (conta: NovaConta) => http.post<ContaCorrente>('/api/contas', conta), // RF-11
  ativar: (id: number) => http.post<OperacaoContaResposta>(`/api/contas/${id}/ativar`),
  desativar: (id: number) => http.post<OperacaoContaResposta>(`/api/contas/${id}/desativar`),
  remover: (id: number) => http.delete(`/api/contas/${id}`), // RF-13
  transferir: (transferencia: Transferencia) => http.post<TransferenciaResposta>('/api/transferencias', transferencia), // RF-15
}

export { ApiError } from './http'
export type * from './tipos'
