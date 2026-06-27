import { createRouter, createWebHistory } from 'vue-router'
import Dashboard from './views/Dashboard.vue'
import Login from './views/Login.vue'

const routes = [
  { path: '/', redirect: '/login' },
  {
    path: '/login',
    name: 'Login',
    component: Login,
    meta: { requiresGuest: true },
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

      { path: 'orders',                 name: 'Orders',               component: () => import('./views/dashboards/ClientOrders.vue'),            meta: { roles: ['admin', 'manager'] } },
      { path: 'manufacturingOrders',    name: 'ManufacturingOrders',  component: () => import('./views/dashboards/ManufacturingOrders.vue'),     meta: { roles: ['admin', 'manager', 'operator'] } },

      { path: 'products',               name: 'Products',             component: () => import('./views/dashboards/Products.vue'),               meta: { roles: ['admin', 'manager', 'operator'] } },
      { path: 'production-line-status', name: 'ProductionLineStatus', component: () => import('./views/dashboards/ProductionLineStatus.vue'),  meta: { roles: ['admin', 'manager', 'operator'] } },
      { path: 'wip-dashboard',          name: 'WipDashboard',         component: () => import('./views/dashboards/WipDashboard.vue'),           meta: { roles: ['admin', 'manager', 'operator'] } },
      { path: 'product-timeline',       name: 'ProductTimeline',      component: () => import('./views/dashboards/ProductTimeline.vue'),        meta: { roles: ['admin', 'manager', 'operator'] } },
      { path: 'settings',               name: 'Settings',             component: () => import('./views/dashboards/Settings.vue'),               meta: { roles: ['admin', 'manager', 'operator'] } },
      { path: 'alerts',                 name: 'AlertsHistory',        component: () => import('./views/dashboards/AlertsHistory.vue'),          meta: { roles: ['admin', 'manager', 'operator'] } },

      { path: 'analytics',              name: 'Analytics',            component: () => import('./views/dashboards/Analytics.vue'),              meta: { roles: ['admin', 'manager'] } },

      { path: 'eta-simulator',          name: 'EtaSimulator',         component: () => import('./views/dashboards/EtaSimulator.vue'),           meta: { roles: ['admin'] } },
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

  const allowedRoles = to.meta.roles as string[] | undefined

  if (allowedRoles && !allowedRoles.includes(user?.role)) {
    return { path: getDefaultRouteByRole(user?.role) }
  }

  return true
})

export default router