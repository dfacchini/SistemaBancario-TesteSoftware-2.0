<template>
  <v-container class="grid gap-6" max-width="1200">
    <div class="flex flex-wrap items-center gap-3">
      <h1 class="text-3xl font-medium mr-auto">Clientes</h1>

      <v-btn color="primary" prepend-icon="mdi-account-plus-outline" @click="dialogoCadastro = true">
        Novo cliente
      </v-btn>

      <v-btn
        color="error"
        :disabled="banco.clientes.length === 0"
        prepend-icon="mdi-delete-sweep-outline"
        variant="outlined"
        @click="pedeConfirmacao({
          titulo: 'Limpar base de clientes',
          texto: `Todos os ${banco.clientes.length} clientes serão removidos. As contas correntes são mantidas. Deseja continuar?`,
          textoConfirmar: 'Limpar base',
          acao: banco.limparClientes,
        })"
      >
        Limpar base
      </v-btn>
    </div>

    <ConsultaPorId
      v-slot="{ resultado }"
      :buscar="clientesApi.consultar"
      icone="mdi-account-search-outline"
      mensagem-nao-encontrado="Cliente não encontrado!"
      rotulo="ID do cliente"
      titulo="Consultar cliente por ID"
    >
      <v-list class="bg-transparent" density="compact">
        <v-list-item subtitle="Id" :title="String(resultado.id)" />
        <v-list-item subtitle="Nome" :title="resultado.nome" />
        <v-list-item subtitle="Email" :title="resultado.email" />
        <v-list-item subtitle="Idade" :title="`${resultado.idade} anos`" />

        <v-list-item subtitle="Status">
          <SituacaoChip :ativo="resultado.ativo" />
        </v-list-item>
      </v-list>
    </ConsultaPorId>

    <v-card>
      <v-card-title class="flex flex-wrap items-center gap-3">
        <span class="mr-auto">Clientes do banco</span>

        <v-text-field
          v-model="busca"
          class="max-w-80"
          clearable
          density="compact"
          hide-details
          placeholder="Buscar por nome ou e-mail"
          prepend-inner-icon="mdi-magnify"
          variant="outlined"
        />
      </v-card-title>

      <v-data-table
        :headers="cabecalhos"
        :items="banco.clientes"
        :loading="banco.carregando"
        no-data-text="Nenhum cliente cadastrado."
        :search="busca"
      >
        <template #item.contaCorrenteId="{ value }">
          <v-btn
            prepend-icon="mdi-bank-outline"
            size="small"
            :to="`/contas?id=${value}`"
            variant="text"
          >
            {{ value }}
          </v-btn>
        </template>

        <template #item.ativo="{ value }">
          <SituacaoChip :ativo="value" />
        </template>

        <template #item.acoes="{ item }">
          <div class="flex justify-end gap-1">
            <v-btn
              v-if="item.ativo"
              color="warning"
              prepend-icon="mdi-account-off-outline"
              size="small"
              variant="text"
              @click="banco.desativarCliente(item.id)"
            >
              Desativar
            </v-btn>

            <v-btn
              v-else
              color="success"
              prepend-icon="mdi-account-check-outline"
              size="small"
              variant="text"
              @click="banco.ativarCliente(item.id)"
            >
              Ativar
            </v-btn>

            <v-btn
              aria-label="Remover cliente"
              color="error"
              icon="mdi-delete-outline"
              size="small"
              variant="text"
              @click="pedeConfirmacao({
                titulo: 'Remover cliente',
                texto: `Remover ${item.nome} (ID ${item.id})? A conta ${item.contaCorrenteId} não será removida.`,
                textoConfirmar: 'Remover',
                acao: () => banco.removerCliente(item.id),
              })"
            />
          </div>
        </template>
      </v-data-table>
    </v-card>

    <ClienteDialog v-model="dialogoCadastro" />
    <ConfirmacaoDialog v-model="dialogoConfirmacao" v-bind="confirmacao" />
  </v-container>
</template>

<script lang="ts" setup>
  import type { Cliente } from '@/api'
  import type { DataTableHeader } from 'vuetify'
  import { ref, shallowRef } from 'vue'
  import { clientesApi } from '@/api'
  import ClienteDialog from '@/components/ClienteDialog.vue'
  import ConfirmacaoDialog from '@/components/ConfirmacaoDialog.vue'
  import ConsultaPorId from '@/components/ConsultaPorId.vue'
  import SituacaoChip from '@/components/SituacaoChip.vue'
  import { useBancoStore } from '@/stores/banco'

  interface Confirmacao {
    titulo: string
    texto: string
    textoConfirmar: string
    acao: () => Promise<unknown>
  }

  const banco = useBancoStore()

  const cabecalhos: DataTableHeader<Cliente>[] = [
    { title: 'ID', key: 'id', width: 80 },
    { title: 'Nome', key: 'nome' },
    { title: 'E-mail', key: 'email' },
    { title: 'Idade', key: 'idade', align: 'end' },
    { title: 'Conta', key: 'contaCorrenteId', align: 'center' },
    { title: 'Status', key: 'ativo' },
    { title: '', key: 'acoes', sortable: false, align: 'end' },
  ]

  const busca = ref('')
  const dialogoCadastro = ref(false)
  const dialogoConfirmacao = ref(false)
  const confirmacao = shallowRef<Confirmacao>({ titulo: '', texto: '', textoConfirmar: '', acao: async () => {} })

  function pedeConfirmacao (dados: Confirmacao) {
    confirmacao.value = dados
    dialogoConfirmacao.value = true
  }
</script>
