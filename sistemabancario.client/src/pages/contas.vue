<template>
  <v-container class="grid gap-6" max-width="1200">
    <div class="flex flex-wrap items-center gap-3">
      <h1 class="text-3xl font-medium mr-auto">Contas correntes</h1>

      <v-btn color="primary" prepend-icon="mdi-bank-plus" @click="dialogoCadastro = true">
        Nova conta
      </v-btn>
    </div>

    <ConsultaPorId
      v-slot="{ resultado }"
      :buscar="contasApi.consultar"
      icone="mdi-bank-outline"
      mensagem-nao-encontrado="Conta não encontrada!"
      rotulo="ID da conta"
      titulo="Consultar conta corrente por ID"
    >
      <v-list class="bg-transparent" density="compact">
        <v-list-item subtitle="Id" :title="String(resultado.id)" />
        <v-list-item subtitle="Saldo" :title="formataMoeda(resultado.saldo)" />
        <v-list-item subtitle="Titular" :title="resultado.titularNome ?? 'Sem titular'" />

        <v-list-item subtitle="Status">
          <SituacaoChip :ativo="resultado.ativa" texto-ativo="Ativa" texto-inativo="Inativa" />
        </v-list-item>
      </v-list>
    </ConsultaPorId>

    <v-card>
      <v-card-title class="flex flex-wrap items-center gap-3">
        <span class="mr-auto">Contas do banco</span>

        <v-text-field
          v-model="busca"
          class="max-w-80"
          clearable
          density="compact"
          hide-details
          placeholder="Buscar por ID ou titular"
          prepend-inner-icon="mdi-magnify"
          variant="outlined"
        />
      </v-card-title>

      <v-data-table
        :headers="cabecalhos"
        :items="banco.contas"
        :loading="banco.carregando"
        no-data-text="Nenhuma conta cadastrada."
        :search="busca"
      >
        <template #item.titularNome="{ value }">
          <span :class="{ 'opacity-60 italic': !value }">{{ value ?? 'Sem titular' }}</span>
        </template>

        <template #item.saldo="{ value }">
          <span class="font-mono">{{ formataMoeda(value) }}</span>
        </template>

        <template #item.ativa="{ value }">
          <SituacaoChip :ativo="value" texto-ativo="Ativa" texto-inativo="Inativa" />
        </template>

        <template #item.acoes="{ item }">
          <div class="flex justify-end gap-1">
            <v-btn
              :disabled="!item.ativa || item.saldo <= 0"
              prepend-icon="mdi-bank-transfer-out"
              size="small"
              :to="`/transferencias?origem=${item.id}`"
              variant="text"
            >
              Transferir
            </v-btn>

            <v-btn
              v-if="item.ativa"
              color="warning"
              size="small"
              variant="text"
              @click="banco.desativarConta(item.id)"
            >
              Desativar
            </v-btn>

            <v-btn
              v-else
              color="success"
              size="small"
              variant="text"
              @click="banco.ativarConta(item.id)"
            >
              Ativar
            </v-btn>

            <v-tooltip
              :disabled="item.titularId === null"
              location="top"
              :text="`Pertence a ${item.titularNome}; remova o cliente antes.`"
            >
              <template #activator="{ props: tooltip }">
                <span v-bind="tooltip">
                  <v-btn
                    aria-label="Remover conta"
                    color="error"
                    :disabled="item.titularId !== null"
                    icon="mdi-delete-outline"
                    size="small"
                    variant="text"
                    @click="pedeRemocao(item.id)"
                  />
                </span>
              </template>
            </v-tooltip>
          </div>
        </template>
      </v-data-table>
    </v-card>

    <ContaDialog v-model="dialogoCadastro" />

    <ConfirmacaoDialog
      v-model="dialogoRemocao"
      :acao="() => banco.removerConta(contaParaRemover!)"
      :texto="`Remover a conta ${contaParaRemover}? Esta ação não pode ser desfeita.`"
      texto-confirmar="Remover"
      titulo="Remover conta corrente"
    />
  </v-container>
</template>

<script lang="ts" setup>
  import type { ContaCorrente } from '@/api'
  import type { DataTableHeader } from 'vuetify'
  import { ref } from 'vue'
  import { useRoute } from 'vue-router'
  import { contasApi } from '@/api'
  import ConfirmacaoDialog from '@/components/ConfirmacaoDialog.vue'
  import ConsultaPorId from '@/components/ConsultaPorId.vue'
  import ContaDialog from '@/components/ContaDialog.vue'
  import SituacaoChip from '@/components/SituacaoChip.vue'
  import { useBancoStore } from '@/stores/banco'
  import { formataMoeda } from '@/utils/formatacao'

  const banco = useBancoStore()
  const route = useRoute()

  const cabecalhos: DataTableHeader<ContaCorrente>[] = [
    { title: 'ID', key: 'id', width: 80 },
    { title: 'Titular', key: 'titularNome' },
    { title: 'Saldo', key: 'saldo', align: 'end' },
    { title: 'Status', key: 'ativa' },
    { title: '', key: 'acoes', sortable: false, align: 'end' },
  ]

  // Link vindo da tela de clientes (/contas?id=N) já filtra a conta.
  const busca = ref(typeof route.query.id === 'string' ? route.query.id : '')
  const dialogoCadastro = ref(false)
  const dialogoRemocao = ref(false)
  const contaParaRemover = ref<number | null>(null)

  function pedeRemocao (id: number) {
    contaParaRemover.value = id
    dialogoRemocao.value = true
  }
</script>
