<script lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAlertStore } from '@/stores/alertStore'

export default {
  name: 'AlertStack',
  setup() {
    const alertStore = useAlertStore()
    const { t } = useI18n()

    const typeLabel = (type: string) => {
      return type === 'time_exceeded' ? t('alerts.timeExceeded') : t('alerts.wrongSequence')
    }

    const typeIcon = (type: string) => {
      return type === 'time_exceeded' ? 'schedule' : 'alt_route'
    }

    const noteText = (alert: any) => {
      if (alert.type === 'time_exceeded') {
        return t('alerts.notes.timeExceeded', {
          product: alert.productSerial,
          pct: alert.thresholdPct,
          phase: alert.phaseName,
          estimated: alert.estimatedDuration,
        })
      }
      return t('alerts.notes.wrongSequence', {
        product: alert.productSerial,
        from: alert.orderFrom,
        to: alert.orderTo,
      })
    }

    const close = (id: number) => {
      alertStore.acknowledge(id)
    }

    return {
      alerts: computed(() => alertStore.activeAlerts),
      typeLabel,
      typeIcon,
      noteText,
      close,
      t,
    }
  },
}
</script>

<template>
  <div class="alert-stack-container">
    <TransitionGroup name="alert" tag="div" class="alert-stack-list">
      <div
          v-for="alert in alerts"
          :key="alert.id"
          class="alert-card"
      >
        <button class="close-btn" @click="close(alert.id)">
          <span class="material-symbols-rounded">close</span>
        </button>

        <div class="flex gap-2 items-start">
          <span class="material-symbols-rounded fill alert-icon">{{ typeIcon(alert.type) }}</span>

          <div class="flex flex-col gap-0.5">
            <p class="text-xs uppercase font-bold opacity-70">{{ t('alerts.title') }}</p>
            <p class="text-sm font-bold">{{ typeLabel(alert.type) }}</p>
            <p class="text-xs opacity-80 line-clamp-3">{{ noteText(alert) }}</p>
          </div>
        </div>
      </div>
    </TransitionGroup>
  </div>
</template>

<style scoped>
.alert-stack-container {
  @apply fixed flex flex-col top-4 right-4 z-[9999];

  .alert-stack-list {
    @apply flex flex-col gap-2;
  }

  .alert-card {
    @apply relative pt-3 pl-3 pb-3 pr-8 w-72 bg-white border-2 border-red-500 rounded-md shadow-lg;

    .alert-icon {
      @apply text-red-500;
    }

    .close-btn {
      @apply absolute top-2 right-2 text-gray-500 hover:text-red-500;
    }
  }
}

.alert-enter-active,
.alert-leave-active {
  transition: all 0.3s ease;
}

.alert-enter-from {
  opacity: 0;
  transform: translateY(-20px);
}

.alert-leave-to {
  opacity: 0;
  transform: translateX(100%);
}
</style>