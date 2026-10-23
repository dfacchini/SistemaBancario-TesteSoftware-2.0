/**
 * router/index.ts
 *
 * Manual routes for ./src/pages/*.vue
 */

// Composables
import { createRouter, createWebHistory } from 'vue-router'
import Index from '@/pages/index.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: Index,
      meta: { titulo: 'Painel' },
    },
    {
      path: '/clientes',
      component: () => import('@/pages/clientes.vue'),
      meta: { titulo: 'Clientes' },
    },
    {
      path: '/contas',
      component: () => import('@/pages/contas.vue'),
      meta: { titulo: 'Contas correntes' },
    },
    {
      path: '/transferencias',
      component: () => import('@/pages/transferencias.vue'),
      meta: { titulo: 'Transferências' },
    },
    {
      path: '/:caminho(.*)*',
      redirect: '/',
    },
  ],
})

router.afterEach(to => {
  document.title = to.meta.titulo ? `${to.meta.titulo} · Sistema Bancário` : 'Sistema Bancário'
})

export default router

declare module 'vue-router' {
  interface RouteMeta {
    titulo?: string
  }
}
