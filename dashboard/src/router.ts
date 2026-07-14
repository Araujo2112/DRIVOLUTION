import { createRouter, createWebHistory } from 'vue-router'
import Dashboard from './views/Dashboard.vue'
import Login from './views/Login.vue'
import ChangePassword from './views/ChangePassword.vue'
import ClientLayout from './views/client/ClientLayout.vue'

// Lista com todas as rotas/páginas disponíveis na aplicação
const routes = [
  // Quando o utilizador entra na raiz "/", é enviado para o login
  { path: '/', redirect: '/login' },
  {
    path: '/login',
    name: 'Login',
    component: Login, // Componente Vue apresentado nesta rota
    meta: { requiresGuest: true }, // Indica que esta página é apenas para utilizadores não autenticados
  },
  // Página obrigatória para alteração da password temporária
  {
    path: '/change-password',
    name: 'ChangePassword',
    component: ChangePassword,
    meta: { requiresAuth: true }, // Para aceder, o utilizador tem de estar autenticado
  },
  // Área principal dos administradores, managers e operadores
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: Dashboard,
    meta: { requiresAuth: true },
    // Rotas filhas apresentadas dentro do layout Dashboard
    children: [
      // Ao entrar apenas em /dashboard, redireciona para o estado das linhas de produção
      { path: '', redirect: '/dashboard/production-line-status' },

      // Página de gestão dos modelos de carros, apenas administradores podem aceder
      { path: 'carModels',              name: 'CarModels',            component: () => import('./views/dashboards/CarModels.vue'),              meta: { roles: ['admin'] } },
      // Página de gestão das fases de fabrico
      { path: 'phases',                 name: 'ManufacturingPhases',  component: () => import('./views/dashboards/ManufacturingPhases.vue'),    meta: { roles: ['admin'] } },
      // Página de gestão das linhas de produção
      { path: 'productionLines',        name: 'ProductionLines',      component: () => import('./views/dashboards/ProductionLines.vue'),        meta: { roles: ['admin'] } },
      // Página de gestão dos suportes RFID
      { path: 'supports',               name: 'Supports',             component: () => import('./views/dashboards/Supports.vue'),               meta: { roles: ['admin'] } },

      // Página de gestão da equipa/utilizadores
      { path: 'team',                   name: 'Team',                 component: () => import('./views/dashboards/Team.vue'),                   meta: { roles: ['admin'] } },
      // Página com o histórico de auditoria
      { path: 'audit',                  name: 'AuditLog',             component: () => import('./views/dashboards/AuditLog.vue'),               meta: { roles: ['admin'] } },

      // Página das encomendas dos clientes
      { path: 'orders',                 name: 'Orders',               component: () => import('./views/dashboards/ClientOrders.vue'),           meta: { roles: ['admin', 'manager'] } },
      // Página das ordens de fabrico
      { path: 'manufacturingOrders',    name: 'ManufacturingOrders',  component: () => import('./views/dashboards/ManufacturingOrders.vue'),    meta: { roles: ['admin', 'manager', 'operator'] } },

      // Página dos produtos
      { path: 'products',               name: 'Products',             component: () => import('./views/dashboards/Products.vue'),               meta: { roles: ['admin', 'manager', 'operator'] } },
      // Página com o estado atual das linhas de produção
      { path: 'production-line-status', name: 'ProductionLineStatus', component: () => import('./views/dashboards/ProductionLineStatus.vue'),   meta: { roles: ['admin', 'manager', 'operator'] } },
      // Dashboard dos produtos em produção (Work in Progress)
      { path: 'wip-dashboard',          name: 'WipDashboard',         component: () => import('./views/dashboards/WipDashboard.vue'),           meta: { roles: ['admin', 'manager', 'operator'] } },
      // Página que apresenta a timeline de um produto
      { path: 'product-timeline',       name: 'ProductTimeline',      component: () => import('./views/dashboards/ProductTimeline.vue'),        meta: { roles: ['admin', 'manager', 'operator'] } },
      // Página de configurações
      { path: 'settings',               name: 'Settings',             component: () => import('./views/dashboards/Settings.vue'),               meta: { roles: ['admin', 'manager', 'operator'] } },
      // Página com o histórico de alertas
      { path: 'alerts',                 name: 'AlertsHistory',        component: () => import('./views/dashboards/AlertsHistory.vue'),          meta: { roles: ['admin', 'manager', 'operator'] } },

      // Página de análises e estatísticas
      { path: 'analytics',              name: 'Analytics',            component: () => import('./views/dashboards/Analytics.vue'),              meta: { roles: ['admin', 'manager'] } },

      // Simulador de ETA
      { path: 'eta-simulator',          name: 'EtaSimulator',         component: () => import('./views/dashboards/EtaSimulator.vue'),           meta: { roles: ['admin'] } },

      // Página de presença dos operadores nas workstations
      { path: 'presence',               name: 'WorkstationPresence',  component: () => import('./views/dashboards/WorkstationPresence.vue'),    meta: { roles: ['admin', 'manager', 'operator'] } },

      // Página de gestão dos clientes
      { path: 'clients',                name: 'Clients',              component: () => import('./views/dashboards/Clients.vue'),                meta: { roles: ['admin', 'manager'] } },
    ],
  },

  // ─── Portal do Cliente (role: client) ──────────────────────
  {
    // URL base do portal do cliente
    path: '/client',
    name: 'ClientLayout',
    component: ClientLayout,
    meta: { requiresAuth: true, roles: ['client'] },
    children: [
      // Ao entrar em /client, envia para o dashboard do cliente
      { path: '', redirect: '/client/dashboard' },
      // Dashboard principal do cliente
      { path: 'dashboard', name: 'ClientDashboard', component: () => import('./views/client/ClientDashboard.vue') },
      // Lista de encomendas do cliente
      { path: 'orders', name: 'ClientOrdersPortal', component: () => import('./views/client/ClientOrders.vue') },
      // Detalhes de uma encomenda específica
      { path: 'orders/:id', name: 'ClientOrderDetail', component: () => import('./views/client/ClientOrderDetail.vue') },
      // Página para escolher um modelo para uma nova encomenda
      { path: 'new-order', name: 'ClientVehicleSelection', component: () => import('./views/client/ClientVehicleSelection.vue') },
      // Configurador do veículo escolhido
      { path: 'new-order/:modelId', name: 'ClientVehicleConfigurator', component: () => import('./views/client/ClientVehicleConfigurator.vue') },
      // Definições da conta do cliente
      { path: 'settings', name: 'ClientSettings', component: () => import('./views/client/ClientSettings.vue') },
    ],
  },
]

// Cria o router da aplicação
const router = createRouter({
  // Usa URLs normais do navegador
  history: createWebHistory(import.meta.env.BASE_URL),
  // Regista a lista de rotas definida anteriormente
  routes,
})

// Decide qual é a página inicial adequada para cada role
function getDefaultRouteByRole(role?: string) {
  switch (role) {
    // O administrador é enviado para a gestão dos modelos
    case 'admin':
      return '/dashboard/carModels'
    // O manager é enviado para as encomendas
    case 'manager':
      return '/dashboard/orders'
    // O operador é enviado para o estado das linhas
    case 'operator':
      return '/dashboard/production-line-status'
    // O cliente é enviado para o seu portal
    case 'client':
      return '/client'
    // Se a role não existir ou não for reconhecida, envia para o login
    default:
      return '/login'
  }
}

// Este código é executado antes de cada mudança de página
router.beforeEach((to) => {
  // Obtém o token JWT guardado no navegador
  const token = localStorage.getItem('drivolution_token')
  // Tenta obter e converter os dados do utilizador
  const user = (() => {
    try {
      return JSON.parse(localStorage.getItem('drivolution_user') ?? 'null')
    } catch {
      // Se o JSON estiver inválido, considera que não existe utilizador
      return null
    }
  })()

  // Se a página exige autenticação e não existe token, redireciona para o login
  if (to.meta.requiresAuth && !token) {
    return { path: '/login' }
  }

  // Se a página é apenas para visitantes, mas o utilizador já tem sessão iniciada, redireciona-o para a sua página principal
  if (to.meta.requiresGuest && token) {
    return { path: getDefaultRouteByRole(user?.role) }
  }

  // Enquanto a conta tiver password temporária, a única página acessível é a de troca.
  if (token && user?.mustChangePassword && to.name !== 'ChangePassword') {
    return { path: '/change-password' }
  }

  // Obtém as roles autorizadas para a rota atual
  const allowedRoles = to.meta.roles as string[] | undefined

  // Se a rota tiver restrições de role e o utilizador não tiver uma role autorizada, redireciona-o para a sua página principal
  if (allowedRoles && !allowedRoles.includes(user?.role)) {
    return { path: getDefaultRouteByRole(user?.role) }
  }

  // Permite a navegação
  return true
})

// Exporta o router para ser usado pela aplicação Vue
export default router