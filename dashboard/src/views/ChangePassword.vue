<script setup lang="ts">
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/authStore'
import router from '@/router'
import Button from '@/components/Button.vue'

const { t }   = useI18n()
const auth    = useAuthStore()

const currentPassword = ref('')
const newPassword     = ref('')
const confirmPassword = ref('')
const error           = ref<string | null>(null)
const loading         = ref(false)

async function submit() {
  error.value = null

  if (!currentPassword.value || !newPassword.value || !confirmPassword.value) {
    error.value = t('auth.changePassword.validationRequired')
    return
  }

  if (newPassword.value.length < 8) {
    error.value = t('auth.changePassword.tooShort')
    return
  }

  if (newPassword.value !== confirmPassword.value) {
    error.value = t('auth.changePassword.mismatch')
    return
  }

  loading.value = true
  try {
    await auth.changePassword(currentPassword.value, newPassword.value)
    router.push('/dashboard')
  } catch (e: any) {
    error.value = e.response?.status === 401
      ? t('auth.changePassword.currentIncorrect')
      : (e.response?.data ?? t('auth.changePassword.genericError'))
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

        <div class="px-4 mb-4">
          <h1 class="text-background-50 text-lg font-medium">{{ t('auth.changePassword.title') }}</h1>
          <p class="text-background-400 text-sm mt-1">{{ t('auth.changePassword.subtitle') }}</p>
        </div>

        <div class="flex flex-col items-center gap-2">
          <input
            type="password"
            :placeholder="t('auth.changePassword.currentPassword')"
            v-model="currentPassword"
            @keyup.enter="submit"
          />
          <input
            type="password"
            :placeholder="t('auth.changePassword.newPassword')"
            v-model="newPassword"
            @keyup.enter="submit"
          />
          <input
            type="password"
            :placeholder="t('auth.changePassword.confirmPassword')"
            v-model="confirmPassword"
            @keyup.enter="submit"
          />
          <p v-if="error" class="text-danger-500 text-sm text-center">{{ error }}</p>
          <Button @click="submit" :disabled="loading">
            {{ loading ? t('auth.changePassword.submitting') : t('auth.changePassword.submit') }}
          </Button>
        </div>
      </div>
    </div>
  </div>
</template>
