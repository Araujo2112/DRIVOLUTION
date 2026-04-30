<script lang="ts" setup>
import {useRoute} from 'vue-router';
import {computed, nextTick, ref, useSlots, watch} from "vue";
import 'material-symbols';
import router from "@/router";


const slots = useSlots();
const containerRef = ref<HTMLDivElement | null>(null);
const props = defineProps({
  icon: {
    type: String,
    required: true
  },
  path: {
    type: String,
  },
  danger: {
    type: Boolean,
    default: false
  }
});

const route = useRoute();
const open = ref(false);

const isActive = computed(() => {
  return props.path === "/" && route.path === "/dashboard" || route.path.includes(`/dashboard/${props.path}`)
});

watch(isActive, (newValue) => {
  if (newValue) {
    open.value = true;
  }
}, { immediate: true });

const action = () => {
  if(slots.children){
    open.value = !open.value;
  }

  const fullPath = props.path === '/' ? props.path : `/dashboard/${props.path}`;
  router.push(fullPath);
};
</script>

<template>
  <div
      ref="containerRef"
      tabindex="0"
      class="flex flex-col w-full max-w-96 h-fit transition-all"
      :class="{
          'gap-2': $slots.children && open,
          'min-w-fit w-fit max-w-fit': !$slots.default
        }"
  >
    <div
        class="sidebar-item"
        :class="{
             'active': isActive,
             'danger': danger,
             'w-full': $slots.default,
             'min-w-fit w-fit max-w-fit': !$slots.default
          }"
        @click="action"
    >
      <div class="flex items-center justify-center">
        <span class="material-symbols-rounded">{{ icon }}</span>
      </div>

      <p v-if="$slots.default" class="whitespace-nowrap truncate flex-1">
        <slot></slot>
      </p>

      <div v-if="$slots.children" class="flex transition-transform" :class="{ '-rotate-180': open }">
        <span class="material-symbols-rounded">arrow_drop_down</span>
      </div>
    </div>

    <div
        v-if="$slots.children"
        class="flex flex-col gap-2 pl-4 overflow-hidden transition-all focus:max-h-96 focus-within:max-h-96"
        :class="{
          'max-h-0': !open,
          'max-h-96': open
        }"
    >
      <slot name="children"></slot>
    </div>
  </div>
</template>

<style scoped>

.sidebar-item{
  @apply flex items-center rounded-xl p-2 text-primary-400 text-lg truncate gap-2 cursor-pointer select-none
  hover:bg-primary-400 hover:text-white transition-colors duration-200;

  &.active{
    @apply bg-primary-400 text-white;
  }

  &.danger{
    @apply hover:bg-red-500
  }
}

</style>