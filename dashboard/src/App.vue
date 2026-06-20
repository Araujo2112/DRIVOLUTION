<template>
  <AlertStack />
  <router-view/>
</template>

<script>
import AlertStack from '@/components/AlertStack.vue'
import { useAlertStore } from '@/stores/alertStore'
import { onMounted, onUnmounted } from 'vue'

export default {
  name: 'App',
  components: { AlertStack },
  setup() {
    const alertStore = useAlertStore()

    onMounted(() => {
      alertStore.startPolling()
    })

    onUnmounted(() => {
      alertStore.stopPolling()
    })
  },
}
</script>