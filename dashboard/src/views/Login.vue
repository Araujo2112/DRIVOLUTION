<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/authStore'
import Button from '@/components/Button.vue'

const auth     = useAuthStore()
const email    = ref('')
const password = ref('')
const error    = ref<string | null>(null)
const loading  = ref(false)

async function login() {
  error.value   = null
  loading.value = true
  try {
    await auth.login(email.value, password.value)
  } catch (e: any) {
    error.value = e.response?.status === 401
      ? 'Email ou password incorretos.'
      : 'Erro ao ligar ao servidor.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="flex w-full h-full justify-center items-center">
    <div class="w-96 py-2 px-4">
      <div class="h-full rounded-2xl flex flex-col bg-background-900 p-4">
        <div class="flex w-full px-4 py-6 items-center overflow-hidden">
          <img src="@/assets/icons/drivolution-logo.png" alt="Drivolution" />
        </div>
        <div class="flex flex-col items-center gap-2">
          <input type="email" placeholder="Email" v-model="email" @keyup.enter="login" />
          <input type="password" placeholder="Palavra-passe" v-model="password" @keyup.enter="login" />
          <p v-if="error" class="text-danger-500 text-sm text-center">{{ error }}</p>
          <Button @click="login" :disabled="loading">
            {{ loading ? 'A entrar...' : 'Entrar' }}
          </Button>
        </div>
      </div>
    </div>
  </div>
</template>