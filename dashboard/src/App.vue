<template>
  <AlertStack v-if="authStore.isAuthenticated" />
  <router-view/>
</template>

<script>
import AlertStack from '@/components/AlertStack.vue'
import { useAlertStore } from '@/stores/alertStore'
import { useAuthStore } from '@/stores/authStore'
import { onMounted, onUnmounted } from 'vue'

export default {
  name: 'App',
  components: { AlertStack },
  setup() {
    const alertStore = useAlertStore()
    const authStore = useAuthStore()

    onMounted(() => {
      alertStore.startPolling()
    })

    onUnmounted(() => {
      alertStore.stopPolling()
    })

    return { authStore }
  },
}
</script>