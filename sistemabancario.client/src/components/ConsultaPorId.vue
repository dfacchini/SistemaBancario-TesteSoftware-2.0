<template>
  <v-card :prepend-icon="icone" :title="titulo" variant="tonal">
    <v-card-text>
      <v-form class="flex flex-wrap items-start gap-3" @submit.prevent="consultar">
        <v-text-field
          v-model="id"
          class="min-w-40 max-w-60"
          density="comfortable"
          inputmode="numeric"
          :label="rotulo"
          :rules="[regras.obrigatorio('Informe o ID.'), regras.inteiro()]"
          validate-on="submit"
        />

        <v-btn
          class="mt-1"
          color="primary"
          :loading="consultando"
          prepend-icon="mdi-magnify"
          type="submit"
        >
          Consultar
        </v-btn>
      </v-form>

      <v-alert
        v-if="naoEncontrado"
        class="mt-2"
        density="compact"
        type="warning"
        variant="tonal"
      >
        {{ mensagemNaoEncontrado }}
      </v-alert>

      <div v-else-if="resultado" class="mt-2">
        <slot :resultado="resultado" />
      </div>
    </v-card-text>
  </v-card>
</template>

<script lang="ts" setup generic="T">
  import { ref, shallowRef } from 'vue'
  import { ApiError } from '@/api'
  import { useNotificacoesStore } from '@/stores/notificacoes'
  import { regras } from '@/utils/formatacao'

  /** RF-01 / RF-10: consulta por ID; RF-17: só aceita números. */
  const props = defineProps<{
    titulo: string
    rotulo: string
    icone?: string
    mensagemNaoEncontrado: string
    buscar: (id: number) => Promise<T>
  }>()

  defineSlots<{ default: (props: { resultado: T }) => unknown }>()

  const notificacoes = useNotificacoesStore()

  const id = ref('')
  const consultando = ref(false)
  const resultado = shallowRef<T | null>(null)
  const naoEncontrado = ref(false)

  async function consultar (evento: Promise<{ valid: boolean }>) {
    if (!(await evento).valid) return

    consultando.value = true
    resultado.value = null
    naoEncontrado.value = false
    try {
      resultado.value = await props.buscar(Number(id.value))
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) {
        naoEncontrado.value = true
      } else {
        notificacoes.erro(error)
      }
    } finally {
      consultando.value = false
    }
  }
</script>
