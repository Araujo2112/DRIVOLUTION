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
    <div class="h-full rounded-2xl flex flex-col bg-background-100 dark:bg-background-900 p-4">
      <div class="flex w-full px-4 py-6 object-center object-contain items-center overflow-hidden gap-0">
        <img src="@/assets/icons/drivolution-logo.png" alt="Eco build icon"/>
      </div>

      <div class="flex flex-col w-full h-full items-center gap-2 text-2xl overflow-y-auto overflow-x-hidden">
        <SidebarItem icon="directions_car" path="carModels">Car Models</SidebarItem>
        <SidebarItem icon="shopping_cart" path="orders">Encomendas</SidebarItem>
        <SidebarItem icon="assignment" path="manufacturingOrders">Manuf. Orders</SidebarItem>
        <SidebarItem icon="conveyor_belt" path="productionLines">Prod. Lines</SidebarItem>



        
        <SidebarItem icon="space_dashboard" path="/">Dashboard</SidebarItem>
        <SidebarItem icon="dashboard" path="Controlpanel">Overview</SidebarItem>

        <SidebarItem icon="factory" path="plantaFabrica">
          Sections and Checkpoints
          <template v-slot:children>
            <SidebarItem icon="factory" path="plantaFabrica">Factory Floor</SidebarItem>
            <SidebarItem icon="gate" path="portico">Gate</SidebarItem>
          </template>
        </SidebarItem>
      </div>

      <div class="flex w-full gap-1">
        <SidebarItem icon="account_circle" path="conta">{{ userData ? userData.firstName : "" }}</SidebarItem>
        <SidebarItem icon="settings" path="settings" />
        <SidebarItem icon="logout" @click="logout()" danger/>
      </div>
    </div>

  </div>
</template>

<style scoped>

</style>
