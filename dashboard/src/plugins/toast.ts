import {createApp, reactive} from 'vue'
import Toast from '@/components/Toast.vue'

const toastState = reactive<{
    addToast: ((message: string, type: string) => void) | null
}>({
    addToast: null,
})

export const toast = {
    success(message: string) {
        if (toastState.addToast) {
            toastState.addToast(message, 'success')
        } else {
            console.error('Toast system not initialized.')
        }
    },
    info(message: string) {
        if (toastState.addToast) {
            toastState.addToast(message, 'info')
        } else {
            console.error('Toast system not initialized.')
        }
    },
    error(message: string) {
        if (toastState.addToast) {
            toastState.addToast(message, 'error')
        } else {
            console.error('Toast system not initialized.')
        }
    },
}

export default {
    install(app: any) {
        const container = document.createElement('div')
        document.body.appendChild(container)

        const toastApp = createApp(Toast)
        const instance = toastApp.mount(container)

        if ('addToast' in instance) {
            toastState.addToast = (instance as any).addToast
            app.config.globalProperties.$toast = toast
        } else {
            console.error('Toast component does not have an addToast method.')
        }
    },
}
