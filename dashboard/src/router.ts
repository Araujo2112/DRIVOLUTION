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
      { path: '', redirect: '/dashboard/carModels' },
      { path: 'carModels',              name: 'CarModels',            component: () => import('./views/dashboards/CarModels.vue') },
      { path: 'eta-simulator',          name: 'EtaSimulator',         component: () => import('./views/dashboards/EtaSimulator.vue') },
      { path: 'orders',                 name: 'Orders',               component: () => import('./views/dashboards/ClientOrders.vue') },
      { path: 'manufacturingOrders',    name: 'ManufacturingOrders',  component: () => import('./views/dashboards/ManufacturingOrders.vue') },
      { path: 'products',               name: 'Products',             component: () => import('./views/dashboards/Products.vue') },
      { path: 'productionLines',        name: 'ProductionLines',      component: () => import('./views/dashboards/ProductionLines.vue') },
      { path: 'supports',               name: 'Supports',             component: () => import('./views/dashboards/Supports.vue') },
      { path: 'phases',                 name: 'ManufacturingPhases',  component: () => import('./views/dashboards/ManufacturingPhases.vue') },
      { path: 'production-line-status', name: 'ProductionLineStatus', component: () => import('./views/dashboards/ProductionLineStatus.vue') },
      { path: 'wip-dashboard',          name: 'WipDashboard',         component: () => import('./views/dashboards/WipDashboard.vue') },
      { path: 'product-timeline',       name: 'ProductTimeline',      component: () => import('./views/dashboards/ProductTimeline.vue') },
      { path: 'analytics',              name: 'Analytics',            component: () => import('./views/dashboards/Analytics.vue') },
      { path: 'settings',               name: 'Settings',             component: () => import('./views/dashboards/Settings.vue') },
      { path: 'alerts',                 name: 'AlertsHistory',        component: () => import('./views/dashboards/AlertsHistory.vue') },
      { path: 'team',                   name: 'Team',                 component: () => import('./views/dashboards/Team.vue'), meta: { requiresRole: 'admin' } },
    ],
  },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

router.beforeEach((to) => {
  const token = localStorage.getItem('drivolution_token')
  const user  = (() => {
    try { return JSON.parse(localStorage.getItem('drivolution_user') ?? 'null') }
    catch { return null }
  })()

  if (to.meta.requiresAuth && !token) return { path: '/login' }
  if (to.meta.requiresGuest && token) return { path: '/dashboard' }
  if (to.meta.requiresRole && user?.role !== to.meta.requiresRole) return { path: '/dashboard' }

  return true
})

export default router