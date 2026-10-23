<template>
  <v-dialog v-model="aberto" max-width="560" persistent>
    <v-card prepend-icon="mdi-account-plus-outline" title="Novo cliente">
      <v-form ref="formulario" @submit.prevent="salvar">
        <v-card-text class="grid gap-2">
          <v-text-field
            v-model="nome"
            autofocus
            counter="150"
            label="Nome"
            :rules="[regras.obrigatorio('Informe o nome.'), regras.maximo(150)]"
          />

          <v-text-field
            v-model="email"
            label="E-mail"
            :rules="[regras.obrigatorio('Informe o e-mail.'), regras.email(), regras.maximo(254)]"
            type="email"
          />

          <v-number-input
            v-model="idade"
            control-variant="stacked"
            hint="Entre 18 e 65 anos (RF-06)"
            label="Idade"
            persistent-hint
            :rules="[regras.obrigatorio('Informe a idade.'), regras.idade()]"
          />

          <v-radio-group
            v-model="usarContaExistente"
            class="mt-2"
            hide-details
            inline
            label="Conta corrente"
          >
            <v-radio label="Abrir nova conta" :value="false" />

            <v-radio
              :disabled="banco.contasSemTitular.length === 0"
              label="Vincular conta existente"
              :value="true"
            />
          </v-radio-group>

          <v-select
            v-if="usarContaExistente"
            v-model="contaCorrenteId"
            :item-title="(c: ContaCorrente) => `Conta ${c.id} — ${formataMoeda(c.saldo)}${c.ativa ? '' : ' (inativa)'}`"
            item-value="id"
            :items="banco.contasSemTitular"
            label="Conta sem titular"
            :rules="[regras.obrigatorio('Escolha uma conta.')]"
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
  import type { ContaCorrente } from '@/api'
  import { ref, useTemplateRef, watch } from 'vue'
  import { useBancoStore } from '@/stores/banco'
  import { formataMoeda, regras } from '@/utils/formatacao'

  const aberto = defineModel<boolean>({ default: false })

  const banco = useBancoStore()
  const formulario = useTemplateRef('formulario')

  const nome = ref('')
  const email = ref('')
  const idade = ref<number | null>(null)
  const usarContaExistente = ref(false)
  const contaCorrenteId = ref<number | null>(null)
  const salvando = ref(false)

  watch(aberto, valor => {
    if (valor) {
      nome.value = ''
      email.value = ''
      idade.value = null
      usarContaExistente.value = false
      contaCorrenteId.value = null
      formulario.value?.resetValidation()
    }
  })

  async function salvar () {
    const { valid } = await formulario.value!.validate()
    if (!valid) return

    salvando.value = true
    const cliente = await banco.cadastrarCliente({
      nome: nome.value.trim(),
      email: email.value.trim(),
      idade: idade.value!,
      contaCorrenteId: usarContaExistente.value ? contaCorrenteId.value : null,
    })
    salvando.value = false

    if (cliente) {
      aberto.value = false
    }
  }
</script>
