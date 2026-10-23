/**
 * api/http.ts
 *
 * Cliente HTTP da API SistemaBancario. Em desenvolvimento o Vite encaminha `/api` para a API .NET
 * (ver `server.proxy` em vite.config.mts), então as URLs são relativas.
 */

/** Erro devolvido pela API no formato ProblemDetails (RFC 9457). */
export class ApiError extends Error {
  constructor (
    message: string,
    public readonly status: number,
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

interface ProblemDetails {
  title?: string
  detail?: string
}

const mensagensPadrao: Record<number, string> = {
  400: 'Requisição inválida.',
  404: 'Registro não encontrado.',
  409: 'Operação em conflito com o estado atual.',
}

export async function requisicao<T> (metodo: string, url: string, corpo?: unknown): Promise<T> {
  let resposta: Response
  try {
    resposta = await fetch(url, {
      method: metodo,
      headers: corpo === undefined ? undefined : { 'Content-Type': 'application/json' },
      body: corpo === undefined ? undefined : JSON.stringify(corpo),
    })
  } catch {
    throw new ApiError('Não foi possível conectar à API. Verifique se ela está em execução.', 0)
  }

  if (!resposta.ok) {
    let problema: ProblemDetails = {}
    try {
      problema = await resposta.json()
    } catch {
      // Sem corpo JSON (ex.: proxy sem API rodando).
    }
    const mensagem = problema.detail ?? mensagensPadrao[resposta.status] ?? problema.title ?? `Erro ${resposta.status}.`
    throw new ApiError(mensagem, resposta.status)
  }

  if (resposta.status === 204) {
    return undefined as T
  }
  return resposta.json() as Promise<T>
}

export const http = {
  get: <T>(url: string) => requisicao<T>('GET', url),
  post: <T>(url: string, corpo?: unknown) => requisicao<T>('POST', url, corpo),
  delete: <T = void>(url: string) => requisicao<T>('DELETE', url),
}
