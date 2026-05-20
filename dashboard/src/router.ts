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
                component: () => import('./views/dashboards/ProductionLines.vue') 
            }




        ],
    },
    { path: '/login', component: Login },
    { path: '/register', component: Register },
];

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes,
});

export default router;