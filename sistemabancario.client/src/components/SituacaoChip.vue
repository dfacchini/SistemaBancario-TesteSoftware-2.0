<template>
  <v-chip
    :color="ativo ? 'success' : 'error'"
    :prepend-icon="ativo ? 'mdi-check-circle-outline' : 'mdi-close-circle-outline'"
    size="small"
    variant="tonal"
  >
    {{ ativo ? textoAtivo : textoInativo }}
  </v-chip>
</template>

<script lang="ts" setup>
  withDefaults(defineProps<{
    ativo: boolean
    textoAtivo?: string
    textoInativo?: string
  }>(), {
    textoAtivo: 'Ativo',
    textoInativo: 'Inativo',
  })
</script>
