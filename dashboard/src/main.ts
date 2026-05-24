import { createApp } from 'vue'
import { createI18n } from 'vue-i18n'
import { messages } from './i18n'
import router from '@/router'
import '@/style.css'
import App from '@/App.vue'
import toastPlugin from '@/plugins/toast.js'
import promptPlugin from '@/plugins/prompt.js'
import '@fortawesome/fontawesome-free/css/all.css'

// Aplicar tema ANTES do mount para evitar FOUC (Flash of Unstyled Content)
const savedTheme = localStorage.getItem('theme')
if (savedTheme === 'dark') {
  document.documentElement.classList.add('dark')
} else {
  document.documentElement.classList.remove('dark')
}

// Locale guardado para persistência da linguagem escolhida pelo usuário 
const savedLocale = localStorage.getItem('locale') ?? 'pt'

const i18n = createI18n({
  legacy: false,
  locale: savedLocale,
  messages
})

createApp(App)
    .use(router)
    .use(i18n)
    .use(toastPlugin)
    .use(promptPlugin)
    .mount('#app')