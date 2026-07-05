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
          <ClientNavItem to="/client/new-order" icon="directions_car">{{ t('client.nav.newOrder') }}</ClientNavItem>
          <ClientNavItem to="/client/dashboard" icon="dashboard">{{ t('client.nav.overview') }}</ClientNavItem>
          <ClientNavItem to="/client/orders" icon="local_shipping">{{ t('client.nav.orders') }}</ClientNavItem>
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
      <header class="h-16 shrink-0 flex items-center justify-end gap-4 px-6 rounded-2xl bg-background-50 dark:bg-background-900 border border-background-200 dark:border-background-800 relative">
        <button
          class="relative text-background-400 hover:text-background-600 dark:hover:text-background-200 transition-colors"
          @click="toggleNotifications"
        >
          <span class="material-symbols-rounded text-xl">notifications</span>
          <span
            v-if="unreadCount > 0"
            class="absolute -top-1 -right-1 h-4 min-w-4 px-1 rounded-full bg-danger-500 text-white text-[10px] font-semibold flex items-center justify-center"
          >
            {{ unreadCount > 9 ? '9+' : unreadCount }}
          </span>
        </button>

        <!-- Dropdown de notificações -->
        <div
          v-if="showNotifications"
          class="absolute top-14 right-20 w-80 max-h-96 overflow-y-auto bg-background-50 dark:bg-background-900 border border-background-200 dark:border-background-800 rounded-2xl shadow-lg z-50"
        >
          <div class="flex items-center justify-between px-4 py-3 border-b border-background-200 dark:border-background-800">
            <span class="text-sm font-semibold text-background-900 dark:text-background-50">{{ t('client.notifications.title') }}</span>
            <div class="flex items-center gap-3">
              <button
                v-if="unreadCount > 0"
                class="text-xs text-primary-500 hover:text-primary-600 dark:hover:text-primary-400"
                @click="markAllRead"
              >
                {{ t('client.notifications.markAllRead') }}
              </button>
              <button
                v-if="notifications.length > 0"
                class="text-xs text-danger-500 hover:text-danger-600"
                @click="clearAll"
              >
                {{ t('client.notifications.clearAll') }}
              </button>
            </div>
          </div>

          <div v-if="notifications.length === 0" class="px-4 py-8 text-center text-sm text-background-400 dark:text-background-500">
            {{ t('client.notifications.empty') }}
          </div>

          <button
            v-for="n in notifications"
            :key="n.id"
            class="w-full text-left px-4 py-3 border-b border-background-100 dark:border-background-800 last:border-0 hover:bg-background-100 dark:hover:bg-background-800/60 transition-colors flex gap-2 items-start"
            @click="openNotification(n)"
          >
            <span
              v-if="!n.isRead"
              class="mt-1.5 h-1.5 w-1.5 rounded-full bg-primary-500 shrink-0"
            />
            <span v-else class="mt-1.5 h-1.5 w-1.5 shrink-0" />
            <span class="text-sm text-background-700 dark:text-background-300">{{ n.message }}</span>
          </button>
        </div>

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
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import ClientNavItem from '@/components/client/ClientNavItem.vue'
import { clientPortalService, type ClientNotification } from '@/services/clientPortalService'
import { toast } from '@/plugins/toast'

const { t } = useI18n()
const authStore = useAuthStore()
const router = useRouter()

const initials = computed(() => {
  const name = authStore.user?.name ?? ''
  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map(p => p[0]?.toUpperCase())
    .join('') || '?'
})

// ─── Notificações (sino) ───
const notifications = ref<ClientNotification[]>([])
const unreadCount = ref(0)
const showNotifications = ref(false)

async function loadNotifications() {
  try {
    const res = await clientPortalService.getNotifications()
    notifications.value = res.items
    unreadCount.value = res.unreadCount
  } catch {
    // Falha silenciosa de propósito: isto é um polling em background, não uma
    // ação do utilizador — um toast a cada 30s por causa de um endpoint em
    // baixo seria mais irritante do que útil. Falha fica só nos logs.
  }
}

function toggleNotifications() {
  showNotifications.value = !showNotifications.value
}

async function markAllRead() {
  try {
    await clientPortalService.markAllNotificationsRead()
    notifications.value = notifications.value.map(n => ({ ...n, isRead: true }))
    unreadCount.value = 0
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}

async function clearAll() {
  try {
    await clientPortalService.clearAllNotifications()
    notifications.value = []
    unreadCount.value = 0
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}

async function openNotification(n: ClientNotification) {
  if (!n.isRead) {
    try {
      await clientPortalService.markNotificationRead(n.id)
      n.isRead = true
      unreadCount.value = Math.max(0, unreadCount.value - 1)
    } catch {
      toast.error(t('errors.saveFailed'))
    }
  }
  showNotifications.value = false
  if (n.clientOrderId) router.push(`/client/orders/${n.clientOrderId}`)
}

let pollTimer: ReturnType<typeof setInterval>
onMounted(() => {
  loadNotifications()
  pollTimer = setInterval(loadNotifications, 30_000)
})
onUnmounted(() => clearInterval(pollTimer))
</script>