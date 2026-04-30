import {createApp} from 'vue'
import router from '@/router'
import '@/style.css'
import App from '@/App.vue'
import toastPlugin from '@/plugins/toast.js'
import promptPlugin from '@/plugins/prompt.js'
import '@fortawesome/fontawesome-free/css/all.css'

createApp(App)
    .use(router)
    .use(toastPlugin)
    .use(promptPlugin)
    .mount('#app')
