import { createApp } from 'vue'
import { createI18n } from 'vue-i18n'
import { messages } from './i18n'
import router from '@/router'
import '@/style.css'
import App from '@/App.vue'
import toastPlugin from '@/plugins/toast.js'
import promptPlugin from '@/plugins/prompt.js'
import '@fortawesome/fontawesome-free/css/all.css'

const i18n = createI18n({
  legacy: false,
  locale: 'pt',
  messages
})

createApp(App)
    .use(router)
    .use(i18n)
    .use(toastPlugin)
    .use(promptPlugin)
    .mount('#app')