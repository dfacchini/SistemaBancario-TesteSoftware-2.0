<template>
  <v-dialog v-model="aberto" max-width="440">
    <v-card :prepend-icon="icone" :title="titulo">
      <v-card-text>
        <slot>{{ texto }}</slot>
      </v-card-text>

      <v-card-actions>
        <v-spacer />
        <v-btn :disabled="processando" @click="aberto = false">Cancelar</v-btn>

        <v-btn :color="cor" :loading="processando" variant="flat" @click="confirmar">
          {{ textoConfirmar }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
  import { ref } from 'vue'

  const props = withDefaults(defineProps<{
    titulo: string
    texto?: string
    textoConfirmar?: string
    cor?: string
    icone?: string
    /** Executada ao confirmar; o diálogo fecha quando ela termina. */
    acao: () => Promise<unknown>
  }>(), {
    texto: '',
    textoConfirmar: 'Confirmar',
    cor: 'error',
    icone: 'mdi-alert-outline',
  })

  const aberto = defineModel<boolean>({ default: false })
  const processando = ref(false)

  async function confirmar () {
    processando.value = true
    try {
      await props.acao()
    } finally {
      processando.value = false
      aberto.value = false
    }
  }
</script>
