<template>
  <v-dialog v-model="aberto" max-width="440" persistent>
    <v-card prepend-icon="mdi-bank-plus" title="Nova conta corrente">
      <v-form ref="formulario" @submit.prevent="salvar">
        <v-card-text>
          <v-number-input
            v-model="saldoInicial"
            autofocus
            control-variant="hidden"
            label="Saldo inicial"
            :precision="2"
            prefix="R$"
            :rules="[regras.obrigatorio('Informe o saldo inicial.'), regras.naoNegativo()]"
          />
        </v-card-text>

        <v-card-actions>
          <v-spacer />
          <v-btn :disabled="salvando" @click="aberto = false">Cancelar</v-btn>
          <v-btn color="primary" :loading="salvando" type="submit" variant="flat">Cadastrar</v-btn>
        </v-card-actions>
      </v-form>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
  import { ref, useTemplateRef, watch } from 'vue'
  import { useBancoStore } from '@/stores/banco'
  import { regras } from '@/utils/formatacao'

  const aberto = defineModel<boolean>({ default: false })

  const banco = useBancoStore()
  const formulario = useTemplateRef('formulario')

  const saldoInicial = ref<number | null>(0)
  const salvando = ref(false)

  watch(aberto, valor => {
    if (valor) {
      saldoInicial.value = 0
      formulario.value?.resetValidation()
    }
  })

  async function salvar () {
    const { valid } = await formulario.value!.validate()
    if (!valid) return

    salvando.value = true
    const conta = await banco.cadastrarConta({ saldoInicial: saldoInicial.value ?? 0 })
    salvando.value = false

    if (conta) {
      aberto.value = false
    }
  }
</script>
