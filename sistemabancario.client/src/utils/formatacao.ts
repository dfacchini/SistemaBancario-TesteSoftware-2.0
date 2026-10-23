/**
 * utils/formatacao.ts
 *
 * Formatação em pt-BR e regras de validação reutilizadas pelos formulários (Vuetify `rules`).
 */

const moeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })

export function formataMoeda (valor: number) {
  return moeda.format(valor)
}

type Regra = (valor: unknown) => true | string

const vazio = (valor: unknown) => valor === null || valor === undefined || String(valor).trim() === ''

export const regras = {
  obrigatorio: (mensagem = 'Campo obrigatório.'): Regra => valor => !vazio(valor) || mensagem,

  /** RF-17: entradas não numéricas são recusadas. */
  inteiro: (): Regra => valor => vazio(valor) || /^\d+$/.test(String(valor).trim()) || 'Digite um número.',

  /** RF-06: 18 a 65 anos, inclusive. */
  idade: (): Regra => valor =>
    vazio(valor) || (Number(valor) >= 18 && Number(valor) <= 65) || 'A idade do cliente precisa estar entre 18 e 65 anos.',

  email: (): Regra => valor => vazio(valor) || /^[^\s@]+@[^\s@][^\s.@]*\.[^\s@]+$/.test(String(valor)) || 'E-mail inválido.',

  maximo: (tamanho: number): Regra => valor => vazio(valor) || String(valor).length <= tamanho || `Máximo de ${tamanho} caracteres.`,

  positivo: (mensagem = 'Informe um valor maior que zero.'): Regra => valor => (Number(valor) > 0) || mensagem,

  naoNegativo: (): Regra => valor => vazio(valor) || Number(valor) >= 0 || 'O valor não pode ser negativo.',
}
