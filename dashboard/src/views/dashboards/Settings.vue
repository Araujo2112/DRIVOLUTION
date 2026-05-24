<script setup lang="ts">
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'

const { t, locale } = useI18n()

const isDark    = ref(document.documentElement.classList.contains('dark'))
const isEnglish = ref(locale.value === 'en')

watch(isDark, (dark) => {
  if (dark) {
    document.documentElement.classList.add('dark')
    localStorage.setItem('theme', 'dark')
  } else {
    document.documentElement.classList.remove('dark')
    localStorage.setItem('theme', 'light')
  }
})

watch(isEnglish, (en) => {
  locale.value = en ? 'en' : 'pt'
  localStorage.setItem('locale', locale.value)
})
</script>

<template>
  <div class="p-8 w-full">

    <!-- Header — igual ao padrão das outras páginas -->
    <div class="flex items-start justify-between mb-8">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('settings.title') }}
        </h1>
        <p class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('settings.subtitle') }}
        </p>
      </div>
    </div>

    <!-- Card: Aparência -->
    <div class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden mb-4">
      <div class="px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
        <span class="text-xs font-medium text-background-500 uppercase tracking-wider">
          {{ t('settings.appearance') }}
        </span>
      </div>

      <div class="flex items-center justify-between px-4 py-4 bg-background-50 dark:bg-background-800">
        <div class="flex items-center gap-3">
          <span class="material-symbols-rounded text-xl text-background-500 dark:text-background-400">
            {{ isDark ? 'dark_mode' : 'light_mode' }}
          </span>
          <div>
            <p class="text-sm font-medium text-background-900 dark:text-background-50">
              {{ t('settings.theme') }}
            </p>
            <p class="text-xs text-background-500 dark:text-background-400 mt-0.5">
              {{ isDark ? t('settings.themeDark') : t('settings.themeLight') }}
            </p>
          </div>
        </div>

        <button
          role="switch"
          :aria-checked="isDark"
          @click="isDark = !isDark"
          class="relative inline-flex h-6 w-11 items-center rounded-full transition-colors duration-200 focus:outline-none"
          :class="isDark ? 'bg-primary-500' : 'bg-background-300 dark:bg-background-600'"
        >
          <span
            class="inline-block h-4 w-4 rounded-full bg-white shadow-sm transform transition-transform duration-200"
            :class="isDark ? 'translate-x-6' : 'translate-x-1'"
          />
        </button>
      </div>
    </div>

    <!-- Card: Idioma -->
    <div class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">
      <div class="px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
        <span class="text-xs font-medium text-background-500 uppercase tracking-wider">
          {{ t('settings.language') }}
        </span>
      </div>

      <div class="px-4 py-4 bg-background-50 dark:bg-background-800 flex flex-col gap-3">
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-3">
            <span class="material-symbols-rounded text-xl text-background-500 dark:text-background-400">
              language
            </span>
            <div>
              <p class="text-sm font-medium text-background-900 dark:text-background-50">
                {{ t('settings.languageLabel') }}
              </p>
              <p class="text-xs text-background-500 dark:text-background-400 mt-0.5">
                {{ isEnglish ? 'English' : 'Português' }}
              </p>
            </div>
          </div>

          <button
            role="switch"
            :aria-checked="isEnglish"
            @click="isEnglish = !isEnglish"
            class="relative inline-flex h-6 w-11 items-center rounded-full transition-colors duration-200 focus:outline-none"
            :class="isEnglish ? 'bg-primary-500' : 'bg-background-300 dark:bg-background-600'"
          >
            <span
              class="inline-block h-4 w-4 rounded-full bg-white shadow-sm transform transition-transform duration-200"
              :class="isEnglish ? 'translate-x-6' : 'translate-x-1'"
            />
          </button>
        </div>

        <!-- Botões PT / EN -->
        <div class="flex gap-3">
          <button
            @click="isEnglish = false"
            class="flex-1 py-2 rounded-lg text-sm font-medium border transition-colors duration-150"
            :class="!isEnglish
              ? 'border-primary-500 bg-primary-500/10 text-primary-500'
              : 'border-background-300 dark:border-background-600 text-background-500 dark:text-background-400 hover:border-background-400'"
          >
            🇵🇹 Português
          </button>
          <button
            @click="isEnglish = true"
            class="flex-1 py-2 rounded-lg text-sm font-medium border transition-colors duration-150"
            :class="isEnglish
              ? 'border-primary-500 bg-primary-500/10 text-primary-500'
              : 'border-background-300 dark:border-background-600 text-background-500 dark:text-background-400 hover:border-background-400'"
          >
            🇬🇧 English
          </button>
        </div>
      </div>
    </div>

  </div>
</template>
