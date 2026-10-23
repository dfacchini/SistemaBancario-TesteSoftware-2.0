// Utilities
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { ApiError } from '@/api'

/** Item consumido pelo `<v-snackbar-queue>` de App.vue. */
export interface Notificacao {
  text: string
  color: 'success' | 'info' | 'warning' | 'error'
  timeout?: number
}

export const useNotificacoesStore = defineStore('notificacoes', () => {
  const fila = ref<Notificacao[]>([])

  function sucesso (text: string) {
    fila.value.push({ text, color: 'success' })
  }

  function info (text: string) {
    fila.value.push({ text, color: 'info' })
  }

  function erro (e: unknown) {
    const text = e instanceof ApiError ? e.message : 'Ocorreu um erro inesperado.'
    if (!(e instanceof ApiError)) {
      console.error(e)
    }
    fila.value.push({ text, color: 'error', timeout: 6000 })
  }

  return { fila, sucesso, info, erro }
})
