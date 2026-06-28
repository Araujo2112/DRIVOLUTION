<script setup lang="ts">
import SidebarItem from './SidebarItem.vue'
import { useAuthStore } from '@/stores/authStore'

const auth = useAuthStore()
</script>

<template>
  <div class="h-full min-w-60 max-w-60 py-2">
    <div class="h-full rounded-2xl flex flex-col bg-background-200 dark:bg-background-950 p-4">

      <div class="flex w-full px-4 py-6 items-center overflow-hidden">
        <img src="@/assets/icons/drivolution-logo.png" alt="Drivolution logo" />
      </div>

      <div class="flex flex-col w-full h-full items-center gap-1 text-2xl overflow-y-auto overflow-x-hidden">

        <template v-if="auth.isAdmin">
          <div class="w-full px-2 pt-1 pb-1">
            <span class="text-xs font-semibold uppercase tracking-widest text-background-400 dark:text-background-600">
              {{ $t('sidebar.configuration') }}
            </span>
          </div>
          <SidebarItem icon="directions_car"          path="carModels">{{ $t('carModels.nav') }}</SidebarItem>
          <SidebarItem icon="calculate"               path="eta-simulator">{{ $t('etaSimulation.nav') }}</SidebarItem>
          <SidebarItem icon="precision_manufacturing" path="phases">{{ $t('phases.nav') }}</SidebarItem>
          <SidebarItem icon="conveyor_belt"           path="productionLines">{{ $t('productionLines.nav') }}</SidebarItem>
          <SidebarItem icon="conveyor_belt"           path="supports">{{ $t('supports.nav') }}</SidebarItem>
        </template>

        <template v-if="auth.isAdmin || auth.isManager || auth.isOperator">
          <div class="w-full px-2 pt-3 pb-1">
            <span class="text-xs font-semibold uppercase tracking-widest text-background-400 dark:text-background-600">
              {{ $t('sidebar.orders') }}
            </span>
          </div>
          <SidebarItem
            v-if="auth.isAdmin || auth.isManager"
            icon="shopping_cart"
            path="orders"
          >
            {{ $t('orders.nav') }}
          </SidebarItem>
          <SidebarItem icon="assignment" path="manufacturingOrders">{{ $t('mo.nav') }}</SidebarItem>
          <SidebarItem icon="directions_car" path="products">{{ $t('products.nav') }}</SidebarItem>
        </template>

        <template v-if="auth.isAdmin || auth.isManager || auth.isOperator">
          <div class="w-full px-2 pt-3 pb-1">
            <span class="text-xs font-semibold uppercase tracking-widest text-background-400 dark:text-background-600">
              {{ $t('sidebar.monitoring') }}
            </span>
          </div>
          <SidebarItem icon="monitoring" path="production-line-status">{{ $t('lineStatus.nav') }}</SidebarItem>
          <SidebarItem icon="dashboard" path="wip-dashboard">{{ $t('wip.nav') }}</SidebarItem>
          <SidebarItem icon="timeline" path="product-timeline">{{ $t('timeline.nav') }}</SidebarItem>
          <SidebarItem
            v-if="auth.isAdmin || auth.isManager"
            icon="bar_chart"
            path="analytics"
          >
            {{ $t('analytics.nav') }}
          </SidebarItem>
          <SidebarItem icon="notifications" path="alerts">{{ $t('alertsHistory.nav') }}</SidebarItem>
          <SidebarItem icon="badge" path="presence">{{ $t('presence.nav') }}</SidebarItem>
        </template>

        <template v-if="auth.isAdmin">
          <div class="w-full px-2 pt-3 pb-1">
            <span class="text-xs font-semibold uppercase tracking-widest text-background-400 dark:text-background-600">
              {{ $t('sidebar.admin') }}
            </span>
          </div>
          <SidebarItem icon="group"   path="team">{{ $t('team.nav') }}</SidebarItem>
          <SidebarItem icon="history" path="audit">{{ $t('audit.nav') }}</SidebarItem>
        </template>

      </div>

      <div class="pt-2 border-t border-background-300 dark:border-background-800 flex flex-col gap-1">
        <div class="px-2 py-1 text-sm text-background-500 dark:text-background-400 truncate">
          {{ auth.user?.name }}
          <span class="ml-1 text-xs opacity-60">({{ auth.user?.role }})</span>
        </div>
        <SidebarItem icon="settings" path="settings">{{ $t('settings.nav') }}</SidebarItem>
        <button
          @click="auth.logout()"
          class="flex items-center gap-2 w-full px-3 py-2 rounded-lg text-sm text-background-500 hover:text-danger-500 hover:bg-background-100 dark:hover:bg-background-900 transition-colors"
        >
          <span class="material-symbols-outlined text-xl">logout</span>
          {{ $t('sidebar.logout') }}
        </button>
      </div>

    </div>
  </div>
</template>