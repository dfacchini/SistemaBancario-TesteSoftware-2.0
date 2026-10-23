<template>
  <v-container class="grid gap-6" max-width="760">
    <h1 class="text-3xl font-medium">Transferência entre contas</h1>

    <v-card>
      <v-form ref="formulario" @submit.prevent="transferir">
        <v-card-text class="grid gap-2">
          <v-select
            v-model="origemId"
            :item-props="propsConta"
            :item-title="tituloConta"
            item-value="id"
            :items="banco.contas"
            label="Conta de origem"
            prepend-inner-icon="mdi-bank-transfer-out"
            :rules="[regras.obrigatorio('Escolha a conta de origem.')]"
          />

          <v-select
            v-model="destinoId"
            :item-props="propsConta"
            :item-title="tituloConta"
            item-value="id"
            :items="contasDestino"
            label="Conta de destino"
            prepend-inner-icon="mdi-bank-transfer-in"
            :rules="[regras.obrigatorio('Escolha a conta de destino.')]"
          />

          <v-number-input
            v-model="valor"
            control-variant="hidden"
            :hint="origem ? `Saldo disponível: ${formataMoeda(origem.saldo)}` : undefined"
            label="Valor"
            persistent-hint
            :precision="2"
            prefix="R$"
            :rules="[regras.positivo(), regraSaldo]"
          />

          <v-alert
            v-if="contaInativa"
            class="mt-2"
            density="compact"
            type="warning"
            variant="tonal"
          >
            As duas contas precisam estar ativas para transferir.
          </v-alert>
        </v-card-text>

        <v-card-actions>
          <v-spacer />

          <v-btn
            color="primary"
            :disabled="contaInativa"
            :loading="enviando"
            prepend-icon="mdi-swap-horizontal"
            type="submit"
            variant="flat"
          >
            Transferir
          </v-btn>
        </v-card-actions>
      </v-form>
    </v-card>

    <v-card v-if="ultima" prepend-icon="mdi-check-circle-outline" :title="ultima.mensagem" variant="tonal">
      <v-card-text class="grid sm:grid-cols-2 gap-4">
        <div v-for="conta in [ultima.origem, ultima.destino]" :key="conta.id">
          <div class="opacity-70">
            {{ conta.id === ultima.origem.id ? 'Origem' : 'Destino' }} — conta {{ conta.id }}
            <template v-if="conta.titularNome">({{ conta.titularNome }})</template>
          </div>

          <div class="text-xl font-mono">{{ formataMoeda(conta.saldo) }}</div>
        </div>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script lang="ts" setup>
  import type { ContaCorrente, TransferenciaResposta } from '@/api'
  import { computed, ref, useTemplateRef, watch } from 'vue'
  import { useRoute } from 'vue-router'
  import { useBancoStore } from '@/stores/banco'
  import { formataMoeda, regras } from '@/utils/formatacao'

  /** RF-15: o saldo da origem precisa ser maior ou igual ao valor (zerar é permitido). */
  const banco = useBancoStore()
  const route = useRoute()
  const formulario = useTemplateRef('formulario')

  const origemId = ref<number | null>(typeof route.query.origem === 'string' ? Number(route.query.origem) : null)
  const destinoId = ref<number | null>(null)
  const valor = ref<number | null>(null)
  const enviando = ref(false)
  const ultima = ref<TransferenciaResposta | null>(null)

  const origem = computed(() => banco.contas.find(c => c.id === origemId.value))
  const destino = computed(() => banco.contas.find(c => c.id === destinoId.value))
  const contasDestino = computed(() => banco.contas.filter(c => c.id !== origemId.value))
  const contaInativa = computed(() => origem.value?.ativa === false || destino.value?.ativa === false)

  watch(origemId, id => {
    if (id === destinoId.value) destinoId.value = null
  })

  function tituloConta (conta: ContaCorrente) {
    return `Conta ${conta.id} — ${conta.titularNome ?? 'sem titular'}`
  }

  function propsConta (conta: ContaCorrente) {
    return { subtitle: `${formataMoeda(conta.saldo)}${conta.ativa ? '' : ' · inativa'}` }
  }

  function regraSaldo (v: unknown) {
    return !origem.value || Number(v) <= origem.value.saldo || 'Saldo insuficiente na conta de origem.'
  }

  async function transferir () {
    const { valid } = await formulario.value!.validate()
    if (!valid) return

    enviando.value = true
    const resposta = await banco.transferir({
      contaOrigemId: origemId.value!,
      contaDestinoId: destinoId.value!,
      valor: valor.value!,
    })
    enviando.value = false

    if (resposta) {
      ultima.value = resposta
      valor.value = null
      formulario.value?.resetValidation()
    }
  }
</script>
