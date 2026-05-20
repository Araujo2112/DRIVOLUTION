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
                component: () => import('./views/dashboards/ProductionLines.vue'),
            },
            {
                path: 'production-line-status',
                name: 'ProductionLineStatus',
                component: () => import('./views/dashboards/ProductionLineStatus.vue'),
            },
            {
                path: 'plantaFabrica',
                name: 'PlantFloorSection',
                component: () => import('./views/dashboards/PlantFloorSection.vue'),
            },
            {
                path: 'portico',
                name: 'Checkpoint',
                component: () => import('./views/dashboards/Checkpoint.vue'),
            },
            {
                path: 'clientes',
                name: 'Clientes',
                component: () => import('./views/dashboards/Client.vue'),
            },
            {
                path: 'funcionarios',
                name: 'Employees',
                component: () => import('./views/dashboards/Employees.vue'),
            },
            {
                path: 'funcionarios/:id',
                name: 'Employee',
                component: () => import('./views/dashboards/Employee.vue'),
            },
            {
                path: 'manufacturingOrderHistory',
                name: 'ManufacturingOrderHistory',
                component: () => import('./views/dashboards/ManufacturingOrderHistory.vue'),
            },
            {
                path: 'manufacturingProcess',
                name: 'ManufacturingProcess',
                component: () => import('./views/dashboards/ManufacturingProcess.vue'),
            },
            {
                path: 'manufacturingPhase',
                name: 'ManufacturingPhase',
                component: () => import('./views/dashboards/ManufacturingPhase.vue'),
            },
            {
                path: 'product',
                name: 'Product',
                component: () => import('./views/dashboards/Product.vue'),
            },
            {
                path: 'lotproduct',
                name: 'ProductLot',
                component: () => import('./views/dashboards/ProductLot.vue'),
            },
            {
                path: 'ordensManufatura',
                name: 'OrdensManufatura',
                component: () => import('./views/dashboards/ManufacturingOrder.vue'),
            },
            {
                path: 'manufacturingOrdersGraph',
                name: 'ManufacturingOrdersGraph',
                component: () => import('./views/dashboards/ManufacturingOrderGraphVisualization.vue'),
            },
            {
                path: 'chatbot',
                name: 'ChatBot',
                component: () => import('./views/dashboards/ChatBot.vue'),
            },
            {
                path: 'Controlpanel',
                name: 'ControlPanel',
                component: () => import('./views/dashboards/ControlPanel.vue'),
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