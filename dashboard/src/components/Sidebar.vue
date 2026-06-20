<script setup lang="ts">
import SidebarItem from './SidebarItem.vue'
import { onMounted, Ref, ref } from "vue"
import router from "@/router.ts"
import { Employee } from "@/models/Employee";

const userData: Ref<Employee | null> = ref(null)

onMounted(() => {
  try {
    const data = JSON.parse(localStorage.getItem("texpact_user_data")!)
    userData.value = {
      id: data.id,
      firstName: data.firstName,
      lastName: data.lastName,
      username: data.username,
      watchId: data.watchId
    }
  } catch (_) {
    userData.value = {
      id: -1,
      firstName: "Unknown",
      lastName: "",
      username: "unknown",
      watchId: null
    }
  }
})

function logout() {
  localStorage.removeItem("texpact_token")
  localStorage.removeItem("texpact_user_data")
  router.push("/login")
}
</script>

<template>
  <div class="h-full min-w-60 max-w-60 py-2">
    <div class="h-full rounded-2xl flex flex-col bg-background-200 dark:bg-background-950 p-4">

      <!-- Logo -->
      <div class="flex w-full px-4 py-6 items-center overflow-hidden">
        <img src="@/assets/icons/drivolution-logo.png" alt="Drivolution logo" />
      </div>

      <!-- Navegação -->
      <div class="flex flex-col w-full h-full items-center gap-1 text-2xl overflow-y-auto overflow-x-hidden">

        <!-- Configuração -->
        <div class="w-full px-2 pt-1 pb-1">
          <span class="text-xs font-semibold uppercase tracking-widest text-background-400 dark:text-background-600">
            Configuração
          </span>
        </div>
        <SidebarItem icon="directions_car" path="carModels">{{ $t('carModels.nav') }}</SidebarItem>
        <SidebarItem icon="precision_manufacturing" path="phases">{{ $t('phases.nav') }}</SidebarItem>
        <SidebarItem icon="conveyor_belt" path="productionLines">{{ $t('productionLines.nav') }}</SidebarItem>
        <SidebarItem icon="conveyor_belt" path="supports">{{ $t('supports.nav') }}</SidebarItem>

        <!-- Encomendas -->
        <div class="w-full px-2 pt-3 pb-1">
          <span class="text-xs font-semibold uppercase tracking-widest text-background-400 dark:text-background-600">
            Encomendas
          </span>
        </div>
        <SidebarItem icon="shopping_cart" path="orders">{{ $t('orders.nav') }}</SidebarItem>
        <SidebarItem icon="assignment" path="manufacturingOrders">{{ $t('mo.nav') }}</SidebarItem>
        <SidebarItem icon="directions_car" path="products">{{ $t('products.nav') }}</SidebarItem>

        <!-- Monitorização -->
        <div class="w-full px-2 pt-3 pb-1">
          <span class="text-xs font-semibold uppercase tracking-widest text-background-400 dark:text-background-600">
            Monitorização
          </span>
        </div>
        <SidebarItem icon="monitoring" path="production-line-status">{{ $t('lineStatus.nav') }}</SidebarItem>
        <SidebarItem icon="dashboard" path="wip-dashboard">{{ $t('wip.nav') }}</SidebarItem>
        <SidebarItem icon="timeline" path="product-timeline">{{ $t('timeline.nav') }}</SidebarItem>
        <SidebarItem icon="notifications" path="alerts">{{ $t('alertsHistory.nav') }}</SidebarItem>
      </div>

      <!-- Rodapé -->
      <div class="flex w-full gap-1">
        <SidebarItem icon="logout" @click="logout()" danger />
        <SidebarItem icon="settings" path="settings" />
      </div>

    </div>
  </div>
</template>