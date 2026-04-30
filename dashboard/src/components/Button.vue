<script setup lang="ts">
import { useRouter } from 'vue-router';
import { useSlots } from "vue";

const props = defineProps({
  icon: {
    type: String,
  },
  path: {
    type: String,
  },
  round: {
    type: Boolean
  }
});

const router = useRouter();
const slots = useSlots();
const handleClick = () => {
  if (props.path) {
    router.push(props.path);
  }
};
</script>

<template>
  <button
      class="flex gap-1 w-fit transition-colors duration-150 ease-in-out"
      :class="[props.round ? 'rounded-full p-2' : !slots.default ? 'rounded-lg p-2' : 'rounded-lg px-2 py-1']"
      @click="handleClick">
    <span v-if="props.icon" class="material-symbols-rounded">{{ props.icon }}</span>
    <slot></slot>
  </button>
</template>

<style scoped>
button {
  @apply bg-primary-500 text-white hover:bg-primary-400;
}

button.danger {
  @apply bg-red-500 text-white hover:bg-red-600;
}

button.empty {
  @apply bg-transparent text-black hover:bg-black/20;
}
</style>
