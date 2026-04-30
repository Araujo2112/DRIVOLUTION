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
                name: 'Index',
                component: () => import('./views/dashboards/Index.vue'),
            },
            {
                path: 'conta',
                name: 'Account',
                component: () => import('./views/dashboards/Employee.vue'),
                beforeEnter: (to, from, next) => {
                    const userData = JSON.parse(localStorage.getItem('texpact_user_data'));
                    if (userData && userData.id) {
                        to.params.id = userData.id;
                    }
                    next();
                },
            },
            {
                path: 'settings',
                name: 'Settings',
                component: () => import('./views/dashboards/Settings.vue'),
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
                path: 'contentores',
                name: 'Containers',
                component: () => import('./views/dashboards/Containers.vue'),
            },
            {
                path: 'contentores/:id',
                name: 'Container',
                component: () => import('./components/FactoryFloorItems/Containers/ContainerView.vue'),
            },
            {
                path: 'historicoContentor',
                name: 'ContainerHistory',
                component: () => import('./views/dashboards/ContainerHistory.vue'),
            },
            {
                path: 'itens',
                name: 'ItemInContainer',
                component: () => import('./views/dashboards/ItemInContainer.vue'),
            },
            {
                path: 'itens/historico',
                name: 'ItemHistory',
                component: () => import('./views/dashboards/ItemHistory.vue'),
            },
            {
                path: 'lote',
                name: 'LotOfRawMaterial',
                component: () => import('./views/dashboards/LotOfRawMaterial.vue'),
            },
            {
                path: 'lote/material',
                name: 'RawMaterial',
                component: () => import('./views/dashboards/RawMaterial.vue'),
            },
            {
                path: 'itemMaterialNaoProcessado',
                name: 'ItemOfRawMaterial',
                component: () => import('./views/dashboards/ItemOfRawMaterial.vue'),
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
                path:'manufacturingOrderHistory',
                name: 'ManufacturingOrderHistory',
                component: () => import('./views/dashboards/ManufacturingOrderHistory.vue'),
            },
            {
                path: 'clientes',
                name: 'Clientes',
                component: () => import('./views/dashboards/Client.vue')
            },
            {
                path: 'manufacturingProcess',
                name:"ManufacturingProcess",
                component: () => import('./views/dashboards/ManufacturingProcess.vue')
            },
            {
                path: 'manufacturingPhase',
                name:"ManufacturingPhase",
                component: () => import('./views/dashboards/ManufacturingPhase.vue')
            },
            {
                path: 'product',
                name:"Product",
                component: () => import('./views/dashboards/Product.vue')
            },
            {
                path: 'lotproduct',
                name:"ProductLot",
                component: () => import('./views/dashboards/ProductLot.vue')
            },
            {
                path:'ordensManufatura',
                name: 'OrdensManufatura',
                component: () => import('./views/dashboards/ManufacturingOrder.vue'),
            },
            {
                path:'manufacturingOrdersGraph',
                name: 'manufacturingOrdersGraph',
                component: () => import('./views/dashboards/ManufacturingOrderGraphVisualization.vue'),
            },
            {
                path: 'chatbot',
                name: 'ChatBot',
                component: () => import('@/views/dashboards/ChatBot.vue')
            },
            {
                path: 'Controlpanel',
                name: 'ControlPanel',
                component: () => import('@/views/dashboards/ControlPanel.vue')
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