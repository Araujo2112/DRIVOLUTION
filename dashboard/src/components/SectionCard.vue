<script setup lang="ts">

const props = defineProps({
  title: {
    type: String
  },
  icon: {
    type: String
  }
})

</script>

<template>
  <div class="w-full h-full rounded-lg flex flex-col bg-background-900 p-4 gap-4">
    <div class="flex gap-2" v-if="title || icon">
      <span class="material-symbols-rounded">{{icon}}</span>
      {{ title }}
    </div>

    <slot>Card Slot</slot>
  </div>
</template>

<style scoped>

</style>