import { reactive, createApp } from 'vue'
import Prompt from '@/components/Prompt.vue'
import router from '@/router'

const promptState = reactive<{
    type: 'ask' | 'confirm' | null
    message: string | null
    resolvePrompt: ((value: string | boolean | null) => void) | null
    visible: boolean
}>({
    type: null,
    message: null,
    resolvePrompt: null,
    visible: false,
})

export const prompt = {
    async ask(message: string): Promise<unknown> {
        return new Promise((resolve) => {
            if (promptState.resolvePrompt) {
                console.error('A prompt is already ativa.')
                resolve(null)
            } else {
                promptState.type = 'ask'
                promptState.message = message
                promptState.visible = true
                promptState.resolvePrompt = resolve
            }
        })
    },

    async confirm(message: string): Promise<unknown> {
        return new Promise((resolve) => {
            if (promptState.resolvePrompt) {
                console.error('A prompt is already ativa.')
                resolve(false)
            } else {
                promptState.type = 'confirm'
                promptState.message = message
                promptState.visible = true
                promptState.resolvePrompt = resolve
            }
        })
    },
}

export default {
    install(app: any) {
        const container = document.createElement('div')
        document.body.appendChild(container)

        const promptApp = createApp(Prompt, { state: promptState })
        promptApp.use(router)
        promptApp.mount(container)

        app.config.globalProperties.$prompt = prompt
    },
}