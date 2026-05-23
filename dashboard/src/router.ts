import { createRouter, createWebHistory } from 'vue-router';
import Dashboard from './views/Dashboard.vue';
import Login from './views/Login.vue';
import Register from './views/Register.vue';

const routes = [
    {
        path: '/',
        redirect: '/login',
    },
    {
        path: '/dashboard',
        name: 'Dashboard',
        component: Dashboard,
        meta: { requiresAuth: true },
        children: [
            {
                path: '',
                redirect: '/dashboard/carModels',
            },
            {
                path: 'carModels',
                name: 'CarModels',
                component: () => import('./views/dashboards/CarModels.vue'),
            },
            {
                path: 'orders',
                name: 'Orders',
                component: () => import('./views/dashboards/ClientOrders.vue'),
            },
            {
                path: 'manufacturingOrders',
                name: 'ManufacturingOrders',
                component: () => import('./views/dashboards/ManufacturingOrders.vue'),
            },
            {
                path: 'productionLines',
                name: 'ProductionLines',
                component: () => import('./views/dashboards/ProductionLines.vue'),
            },
            {
                path: 'production-line-status',
                name: 'ProductionLineStatus',
                component: () => import('./views/dashboards/ProductionLineStatus.vue'),
            },
            {
                path: 'product-timeline',
                name: 'ProductTimeline',
                component: () => import('./views/dashboards/ProductTimeline.vue'),
            },
            {
                path: 'wip-dashboard',
                name: 'WipDashboard',
                component: () => import('./views/dashboards/WipDashboard.vue'),
            },
            { path: 'products', name: 'Products', component: () => import('./views/dashboards/Products.vue') }
        ],
    },
    {
        path: '/login',
        component: Login,
    },
    {
        path: '/register',
        component: Register,
    },
];

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes,
});

export default router;