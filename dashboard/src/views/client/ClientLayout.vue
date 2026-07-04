<template>
  <div class="flex gap-4 w-full h-full overflow-hidden p-4">
    <!-- Sidebar — mesma linguagem visual da sidebar da empresa (cartão
         arredondado, background-200, item ativo em primary-400). Sem
         secções agrupadas: só 4 itens no nav principal, agrupar seria
         ruído. "Definições" foi movido para o bloco do rodapé para bater
         certo com o formato do admin (ver Sidebar.vue): fica junto ao
         nome do utilizador e ao "Sair", separado por divisória. -->
    <div class="min-w-60 max-w-60 h-full">
      <div class="h-full rounded-2xl flex flex-col bg-background-200 dark:bg-background-950 p-4">
        <div class="flex w-full px-4 py-6 items-center overflow-hidden">
          <img src="@/assets/icons/drivolution-logo.png" alt="Drivolution logo" />
        </div>

        <nav class="flex flex-col w-full h-full items-center gap-1 overflow-y-auto overflow-x-hidden">
          <ClientNavItem to="/client/dashboard" icon="dashboard">{{ t('client.nav.overview') }}</ClientNavItem>
          <ClientNavItem to="/client/orders" icon="local_shipping">{{ t('client.nav.orders') }}</ClientNavItem>
          <ClientNavItem to="/client/vehicle-config" icon="directions_car">{{ t('client.nav.myVehicles') }}</ClientNavItem>
          <ClientNavItem to="/client/support" icon="help">{{ t('client.nav.support') }}</ClientNavItem>
        </nav>

        <div class="pt-2 border-t border-background-300 dark:border-background-800 flex flex-col gap-1">
          <div class="px-2 py-1 text-sm text-background-500 dark:text-background-400 truncate">
            {{ authStore.user?.name }}
            <span class="ml-1 text-xs opacity-60">({{ authStore.user?.role }})</span>
          </div>
          <ClientNavItem to="/client/settings" icon="settings">{{ t('client.nav.settings') }}</ClientNavItem>
          <button
            class="flex items-center gap-2 w-full px-3 py-2 rounded-xl text-sm text-background-500 hover:text-danger-500 hover:bg-background-100 dark:hover:bg-background-900 transition-colors"
            @click="authStore.logout()"
          >
            <span class="material-symbols-rounded text-xl">logout</span>
            {{ t('client.logout') }}
          </button>
        </div>
      </div>
    </div>

    <!-- Coluna principal. IMPORTANTE: #app tem h-screen + overflow-hidden
         globalmente (ver style.css) — a página nunca faz scroll ao nível do
         documento. Este é o padrão real usado pelo Dashboard.vue admin:
         o próprio bloco de conteúdo precisa do seu overflow-y-auto. -->
    <div class="flex flex-col h-full w-full gap-4 overflow-hidden">
      <header class="h-16 shrink-0 flex items-center justify-end gap-4 px-6 rounded-2xl bg-background-50 dark:bg-background-900 border border-background-200 dark:border-background-800">
        <button class="relative text-background-400 hover:text-background-600 dark:hover:text-background-200 transition-colors">
          <span class="material-symbols-rounded text-xl">notifications</span>
        </button>
        <div class="flex items-center gap-2.5">
          <div class="h-8 w-8 rounded-full bg-primary-100 dark:bg-primary-900/40 text-primary-600 dark:text-primary-300 flex items-center justify-center text-xs font-semibold">
            {{ initials }}
          </div>
          <span class="hidden sm:inline text-sm text-background-700 dark:text-background-300">{{ authStore.user?.name }}</span>
        </div>
      </header>

      <main class="flex-1 min-h-0 overflow-y-auto overflow-x-hidden rounded-2xl">
        <router-view />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/authStore'
import ClientNavItem from '@/components/client/ClientNavItem.vue'

const { t } = useI18n()
const authStore = useAuthStore()

const initials = computed(() => {
  const name = authStore.user?.name ?? ''
  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map(p => p[0]?.toUpperCase())
    .join('') || '?'
})
</script>
