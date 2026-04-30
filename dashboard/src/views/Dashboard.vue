<script setup lang="ts">
import Sidebar from "@/components/Sidebar.vue";
</script>

<template>
  <div class="flex gap-4 max-w-full w-full h-full overflow-hidden pl-4">
    <Sidebar></Sidebar>
    <div class="flex flex-col h-full w-full gap-4 py-4 pr-4 overflow-x-hidden overflow-y-auto">
      <router-view/>
    </div>
  </div>
</template>

<style scoped>

</style>