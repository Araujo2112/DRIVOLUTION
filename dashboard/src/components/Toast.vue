<script lang="ts">
import {Ref, ref} from 'vue';

interface Toast {
  id: number
  type: string,
  message: string,
}
export default {
  name: 'Toast',
  computed: {
    toast() {
      return toast
    }
  },
  setup() {
    const toasts: Ref<Toast[]> = ref([]);
    const iconMap = ref({
      "success": "check_circle",
      "info": "info",
      "error": "warning",
    })
    let nextId = 0;

    const addToast = (message: string, type: string) => {
      toasts.value.push({ message, type, id: nextId++ });
      setTimeout(() => {
        toasts.value.shift();
      }, 3000);
    };

    const removeToast = (index: number) => {
      toasts.value.splice(index, 1);
    };

    return { toasts, addToast, removeToast, iconMap };
  },
};
</script>

<template>
  <div class="toast-container">
    <TransitionGroup
        name="toast"
        tag="div"
        class="toast-list"
    >
      <div
          v-for="(toast, index) in toasts"
          :key="toast.id"
          class="toast"
          :class="['toast', toast.type]"
          @click="removeToast(index)"
      >
        <div class="flex gap-1 items-center">
          <span class="material-symbols-rounded fill">{{ iconMap[toast.type]}}</span>

          <div class="flex flex-col gap-0">
            <p class="capitalize text-sm leading-none">{{ toast.type }}</p>
            <p class="capitalize text-lg line-clamp-2">{{ toast.message }}</p>
          </div>
        </div>
        <div class="timer"></div>
      </div>
    </TransitionGroup>
  </div>
</template>

<style scoped>
.toast-container {
  @apply fixed flex flex-col bottom-1 right-1 z-[9999];

  .toast-list {
    @apply flex flex-col gap-2;
  }

  .toast {
    @apply relative pt-2 pl-2 pb-4 pr-4 w-60 bg-white font-bold border-2 rounded-md overflow-hidden;

    .timer {
      @apply absolute bottom-0 left-0 w-0 h-2 bg-primary-500;
      animation: timer 3s linear;
    }

    &.info, &.success {
      @apply text-black border-primary-500;
    }

    &.error {
      @apply text-red-500 border-red-500;

      .timer {
        @apply bg-red-500;
      }
    }
  }
}

/* Toast transition animations */
.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}

.toast-enter-from {
  opacity: 0;
  transform: translateX(100%);
}

.toast-leave-to {
  opacity: 0;
  transform: translateX(100%);
}

@keyframes timer {
  0% {
    width: 100%;
  }
  100% {
    width: 0;
  }
}
</style>