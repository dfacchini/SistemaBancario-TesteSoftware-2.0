<template>
  <v-container class="grid gap-6" max-width="1200">
    <div>
      <h1 class="text-3xl font-medium">Painel</h1>
      <p class="opacity-70">Visão geral de clientes e contas correntes.</p>
    </div>

    <div class="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
      <v-card
        v-for="indicador in indicadores"
        :key="indicador.titulo"
        :loading="banco.carregando"
        :to="indicador.rota"
        variant="tonal"
      >
        <v-card-item>
          <template #prepend>
            <v-avatar color="primary" :icon="indicador.icone" variant="tonal" />
          </template>

          <v-card-subtitle>{{ indicador.titulo }}</v-card-subtitle>
          <v-card-title class="text-3xl font-mono">{{ indicador.valor }}</v-card-title>
        </v-card-item>

        <v-card-text class="opacity-70">{{ indicador.detalhe }}</v-card-text>
      </v-card>
    </div>

    <div>
      <h2 class="text-xl font-medium mb-3">Ações rápidas</h2>

      <div class="flex flex-wrap gap-3">
        <v-btn prepend-icon="mdi-account-search-outline" to="/clientes" variant="outlined">Consultar cliente</v-btn>
        <v-btn prepend-icon="mdi-bank-outline" to="/contas" variant="outlined">Consultar conta</v-btn>

        <v-btn color="primary" prepend-icon="mdi-swap-horizontal" to="/transferencias" variant="flat">
          Nova transferência
        </v-btn>
      </div>
    </div>
  </v-container>
</template>

<script lang="ts" setup>
  import { computed } from 'vue'
  import { useBancoStore } from '@/stores/banco'
  import { formataMoeda } from '@/utils/formatacao'

  const banco = useBancoStore()

  const indicadores = computed(() => [
    {
      titulo: 'Clientes',
      valor: banco.clientes.length,
      detalhe: `${banco.clientesAtivos} ativo(s), ${banco.clientes.length - banco.clientesAtivos} inativo(s)`,
      icone: 'mdi-account-group-outline',
      rota: '/clientes',
    },
    {
      titulo: 'Contas correntes',
      valor: banco.contas.length,
      detalhe: `${banco.contasAtivas} ativa(s), ${banco.contasSemTitular.length} sem titular`,
      icone: 'mdi-bank-outline',
      rota: '/contas',
    },
    {
      titulo: 'Saldo total',
      valor: formataMoeda(banco.saldoTotal),
      detalhe: 'Soma dos saldos de todas as contas',
      icone: 'mdi-cash-multiple',
      rota: '/contas',
    },
  ])
</script>
