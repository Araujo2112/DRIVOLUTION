<script setup lang="ts">
import { useRoute } from 'vue-router'
import { computed } from 'vue'
import 'material-symbols'

const props = defineProps<{
  to: string
  icon: string
}>()

const route = useRoute()
const isActive = computed(() => route.path === props.to || route.path.startsWith(props.to + '/'))
</script>

<template>
  <!-- Estilo alinhado com SidebarItem.vue (admin): texto/ícone a primary-400
       por defeito, hover e ativo preenchem a solid primary-400 + texto branco.
       NOTA: primary-400 != primary-500 ("cor oficial Drivolution" no
       tailwind.config.js). Escolhido primary-400 para bater certo com o
       admin — sinalizar ao design system se isto for um problema. -->
  <RouterLink
    :to="to"
    class="flex items-center gap-2 w-full p-2 rounded-xl text-sm truncate transition-colors duration-200"
    :class="isActive
      ? 'bg-primary-400 text-white'
      : 'text-primary-400 hover:bg-primary-400 hover:text-white'"
  >
    <span class="material-symbols-rounded text-2xl">{{ icon }}</span>
    <span class="truncate flex-1"><slot /></span>
  </RouterLink>
</template>