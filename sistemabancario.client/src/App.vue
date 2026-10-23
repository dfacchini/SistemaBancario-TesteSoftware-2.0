<template>
  <v-app>
    <v-app-bar color="primary" flat>
      <v-app-bar-nav-icon v-if="mobile" aria-label="Abrir menu" @click="menuAberto = !menuAberto" />

      <v-app-bar-title>
        <span class="inline-flex items-center gap-2">
          <v-icon icon="mdi-bank" />
          Sistema Bancário
        </span>
      </v-app-bar-title>

      <template #append>
        <v-btn
          aria-label="Recarregar dados"
          icon="mdi-refresh"
          :loading="banco.carregando"
          @click="banco.carregar()"
        />
      </template>
    </v-app-bar>

    <!-- RF-16: o menu do console vira a navegação lateral. -->
    <v-navigation-drawer v-model="menuAberto" :permanent="!mobile">
      <v-list nav>
        <v-list-item
          v-for="item in menu"
          :key="item.rota"
          exact
          :prepend-icon="item.icone"
          :title="item.titulo"
          :to="item.rota"
        />
      </v-list>
    </v-navigation-drawer>

    <v-main>
      <v-alert
        v-if="banco.erroCarregamento"
        class="m-4"
        type="error"
        variant="tonal"
      >
        {{ banco.erroCarregamento }}
        <template #append>
          <v-btn variant="text" @click="banco.carregar()">Tentar novamente</v-btn>
        </template>
      </v-alert>

      <router-view />
    </v-main>

    <v-snackbar-queue v-model="notificacoes.fila" closable location="bottom end" />
  </v-app>
</template>

<script lang="ts" setup>
  import { onMounted, ref } from 'vue'
  import { useDisplay } from 'vuetify'
  import { useBancoStore } from '@/stores/banco'
  import { useNotificacoesStore } from '@/stores/notificacoes'

  const { mobile } = useDisplay()
  const banco = useBancoStore()
  const notificacoes = useNotificacoesStore()

  const menuAberto = ref(!mobile.value)

  const menu = [
    { titulo: 'Painel', rota: '/', icone: 'mdi-view-dashboard-outline' },
    { titulo: 'Clientes', rota: '/clientes', icone: 'mdi-account-group-outline' },
    { titulo: 'Contas correntes', rota: '/contas', icone: 'mdi-bank-outline' },
    { titulo: 'Transferências', rota: '/transferencias', icone: 'mdi-swap-horizontal' },
  ]

  // RF-18: os dados pré-cadastrados vêm da API (seed do banco) na primeira carga.
  onMounted(() => banco.carregar())
</script>
