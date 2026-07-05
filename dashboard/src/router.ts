import { createRouter, createWebHistory } from 'vue-router'
import Dashboard from './views/Dashboard.vue'
import Login from './views/Login.vue'
import ChangePassword from './views/ChangePassword.vue'
import ClientLayout from './views/client/ClientLayout.vue'

const routes = [
  { path: '/', redirect: '/login' },
  {
    path: '/login',
    name: 'Login',
    component: Login,
    meta: { requiresGuest: true },
  },
  {
    path: '/change-password',
    name: 'ChangePassword',
    component: ChangePassword,
    meta: { requiresAuth: true },
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: Dashboard,
    meta: { requiresAuth: true },
    children: [
      { path: '', redirect: '/dashboard/production-line-status' },

      { path: 'carModels',              name: 'CarModels',            component: () => import('./views/dashboards/CarModels.vue'),              meta: { roles: ['admin'] } },
      { path: 'phases',                 name: 'ManufacturingPhases',  component: () => import('./views/dashboards/ManufacturingPhases.vue'),    meta: { roles: ['admin'] } },
      { path: 'productionLines',        name: 'ProductionLines',      component: () => import('./views/dashboards/ProductionLines.vue'),        meta: { roles: ['admin'] } },
      { path: 'supports',               name: 'Supports',             component: () => import('./views/dashboards/Supports.vue'),               meta: { roles: ['admin'] } },

      { path: 'team',                   name: 'Team',                 component: () => import('./views/dashboards/Team.vue'),                   meta: { roles: ['admin'] } },
      { path: 'audit',                  name: 'AuditLog',             component: () => import('./views/dashboards/AuditLog.vue'),               meta: { roles: ['admin'] } },

      { path: 'orders',                 name: 'Orders',               component: () => import('./views/dashboards/ClientOrders.vue'),           meta: { roles: ['admin', 'manager'] } },
      { path: 'manufacturingOrders',    name: 'ManufacturingOrders',  component: () => import('./views/dashboards/ManufacturingOrders.vue'),    meta: { roles: ['admin', 'manager', 'operator'] } },

      { path: 'products',               name: 'Products',             component: () => import('./views/dashboards/Products.vue'),               meta: { roles: ['admin', 'manager', 'operator'] } },
      { path: 'production-line-status', name: 'ProductionLineStatus', component: () => import('./views/dashboards/ProductionLineStatus.vue'),   meta: { roles: ['admin', 'manager', 'operator'] } },
      { path: 'wip-dashboard',          name: 'WipDashboard',         component: () => import('./views/dashboards/WipDashboard.vue'),           meta: { roles: ['admin', 'manager', 'operator'] } },
      { path: 'product-timeline',       name: 'ProductTimeline',      component: () => import('./views/dashboards/ProductTimeline.vue'),        meta: { roles: ['admin', 'manager', 'operator'] } },
      { path: 'settings',               name: 'Settings',             component: () => import('./views/dashboards/Settings.vue'),               meta: { roles: ['admin', 'manager', 'operator'] } },
      { path: 'alerts',                 name: 'AlertsHistory',        component: () => import('./views/dashboards/AlertsHistory.vue'),          meta: { roles: ['admin', 'manager', 'operator'] } },

      { path: 'analytics',              name: 'Analytics',            component: () => import('./views/dashboards/Analytics.vue'),              meta: { roles: ['admin', 'manager'] } },

      { path: 'eta-simulator',          name: 'EtaSimulator',         component: () => import('./views/dashboards/EtaSimulator.vue'),           meta: { roles: ['admin'] } },

      { path: 'presence',               name: 'WorkstationPresence',  component: () => import('./views/dashboards/WorkstationPresence.vue'),    meta: { roles: ['admin', 'manager', 'operator'] } },

      { path: 'clients',                name: 'Clients',              component: () => import('./views/dashboards/Clients.vue'),                meta: { roles: ['admin', 'manager'] } },
    ],
  },

  // ─── Portal do Cliente (role: client) ──────────────────────
  {
    path: '/client',
    name: 'ClientLayout',
    component: ClientLayout,
    meta: { requiresAuth: true, roles: ['client'] },
    children: [
      { path: '', redirect: '/client/dashboard' },
      { path: 'dashboard', name: 'ClientDashboard', component: () => import('./views/client/ClientDashboard.vue') },
      { path: 'orders', name: 'ClientOrdersPortal', component: () => import('./views/client/ClientOrders.vue') },
      { path: 'orders/:id', name: 'ClientOrderDetail', component: () => import('./views/client/ClientOrderDetail.vue') },
      { path: 'new-order', name: 'ClientVehicleSelection', component: () => import('./views/client/ClientVehicleSelection.vue') },
      { path: 'new-order/:modelId', name: 'ClientVehicleConfigurator', component: () => import('./views/client/ClientVehicleConfigurator.vue') },
      { path: 'settings', name: 'ClientSettings', component: () => import('./views/client/ClientSettings.vue') },
    ],
  },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

function getDefaultRouteByRole(role?: string) {
  switch (role) {
    case 'admin':
      return '/dashboard/carModels'
    case 'manager':
      return '/dashboard/orders'
    case 'operator':
      return '/dashboard/production-line-status'
    case 'client':
      return '/client'
    default:
      return '/login'
  }
}

router.beforeEach((to) => {
  const token = localStorage.getItem('drivolution_token')
  const user = (() => {
    try {
      return JSON.parse(localStorage.getItem('drivolution_user') ?? 'null')
    } catch {
      return null
    }
  })()

  if (to.meta.requiresAuth && !token) {
    return { path: '/login' }
  }

  if (to.meta.requiresGuest && token) {
    return { path: getDefaultRouteByRole(user?.role) }
  }

  // Enquanto a conta tiver password temporária, a única página acessível é a de troca.
  if (token && user?.mustChangePassword && to.name !== 'ChangePassword') {
    return { path: '/change-password' }
  }

  const allowedRoles = to.meta.roles as string[] | undefined

  if (allowedRoles && !allowedRoles.includes(user?.role)) {
    return { path: getDefaultRouteByRole(user?.role) }
  }

  return true
})

export default router