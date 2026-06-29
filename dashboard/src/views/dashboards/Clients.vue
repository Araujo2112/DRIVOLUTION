<template>
  <div class="p-6">
    <!-- Header -->
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold text-background-900 dark:text-background-50">{{ t('clients.title') }}</h1>
        <p class="text-sm text-background-500 dark:text-background-400 mt-1">{{ t('clients.subtitle') }}</p>
      </div>
      <button
        class="flex items-center gap-2 bg-primary-500 hover:bg-primary-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
        @click="showModal = true"
      >
        <span class="material-icons text-base">person_add</span>
        {{ t('clients.new') }}
      </button>
    </div>

    <!-- Tabela -->
    <div class="bg-background-0 dark:bg-background-900 rounded-xl border border-background-200 dark:border-background-700 overflow-hidden">
      <div v-if="loading" class="flex justify-center py-12">
        <div class="animate-spin rounded-full h-8 w-8 border-t-2 border-primary-500" />
      </div>

      <div v-else-if="clients.length === 0" class="text-center py-12 text-background-400 dark:text-background-500 text-sm">
        {{ t('clients.empty') }}
      </div>

      <table v-else class="w-full text-sm">
        <thead>
          <tr class="border-b border-background-200 dark:border-background-700 text-left text-xs text-background-400 dark:text-background-500 uppercase tracking-wide">
            <th class="px-4 py-3">{{ t('clients.fields.name') }}</th>
            <th class="px-4 py-3">{{ t('clients.fields.email') }}</th>
            <th class="px-4 py-3">{{ t('clients.fields.status') }}</th>
            <th class="px-4 py-3">{{ t('clients.fields.createdAt') }}</th>
            <th class="px-4 py-3" />
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="client in clients"
            :key="client.id"
            class="border-b border-background-100 dark:border-background-800 last:border-0 hover:bg-background-50 dark:hover:bg-background-800/50"
          >
            <td class="px-4 py-3 font-medium text-background-900 dark:text-background-50">{{ client.name }}</td>
            <td class="px-4 py-3 text-background-500 dark:text-background-400">{{ client.email }}</td>
            <td class="px-4 py-3">
              <span
                :class="[
                  'text-xs font-semibold px-2 py-0.5 rounded-full',
                  client.status === 'active'
                    ? 'bg-success-100 dark:bg-success-900/30 text-success-700 dark:text-success-400'
                    : 'bg-background-100 dark:bg-background-800 text-background-500 dark:text-background-400'
                ]"
              >
                {{ client.status === 'active' ? t('clients.statusActive') : t('clients.statusInactive') }}
              </span>
            </td>
            <td class="px-4 py-3 text-background-500 dark:text-background-400">
              {{ new Date(client.createdAt).toLocaleDateString('pt-PT') }}
            </td>
            <td class="px-4 py-3">
              <div class="flex items-center gap-2 justify-end">
                <button
                  class="text-xs text-primary-500 hover:text-primary-400 font-medium"
                  @click="openEdit(client)"
                >
                  {{ t('clients.edit') }}
                </button>
                <button
                  class="text-xs text-warning-500 hover:text-warning-400 font-medium"
                  @click="openResetPassword(client)"
                >
                  {{ t('clients.resetPassword') }}
                </button>
                <button
                  :class="['text-xs font-medium', client.status === 'active' ? 'text-danger-500 hover:text-danger-400' : 'text-success-500 hover:text-success-400']"
                  @click="toggleStatus(client)"
                >
                  {{ client.status === 'active' ? t('clients.deactivate') : t('clients.activate') }}
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal — Criar cliente -->
    <div v-if="showModal" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div class="bg-background-0 dark:bg-background-900 rounded-xl border border-background-200 dark:border-background-700 w-full max-w-md p-6">
        <h2 class="text-lg font-bold text-background-900 dark:text-background-50 mb-4">{{ t('clients.new') }}</h2>
        <div class="space-y-3">
          <input v-model="form.name" :placeholder="t('clients.placeholders.name')" class="input w-full" />
          <input v-model="form.email" type="email" :placeholder="t('clients.placeholders.email')" class="input w-full" />
        </div>
        <p v-if="formError" class="text-danger-500 text-xs mt-2">{{ formError }}</p>
        <div class="flex justify-end gap-3 mt-5">
          <button class="btn-secondary" @click="closeModal">{{ t('clients.cancel') }}</button>
          <button class="btn-primary" :disabled="saving" @click="createClient">
            {{ saving ? t('clients.saving') : t('clients.create') }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal — Cliente criado: mostrar password temporária -->
    <div v-if="createdPassword" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div class="bg-background-0 dark:bg-background-900 rounded-xl border border-background-200 dark:border-background-700 w-full max-w-md p-6">
        <h2 class="text-lg font-bold text-background-900 dark:text-background-50 mb-4">{{ t('clients.create') }}</h2>
        <p class="text-sm text-background-500 dark:text-background-400 mb-2">{{ t('clients.temporaryPasswordNotice') }}</p>
        <div class="bg-background-100 dark:bg-background-800 rounded-lg px-3 py-2 font-mono text-sm text-center select-all">
          {{ createdPassword }}
        </div>
        <div class="flex justify-end mt-5">
          <button class="btn-primary" @click="createdPassword = ''">{{ t('common.close') }}</button>
        </div>
      </div>
    </div>

    <!-- Modal — Editar cliente -->
    <div v-if="editTarget" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div class="bg-background-0 dark:bg-background-900 rounded-xl border border-background-200 dark:border-background-700 w-full max-w-md p-6">
        <h2 class="text-lg font-bold text-background-900 dark:text-background-50 mb-4">{{ t('clients.editTitle') }}</h2>
        <div class="space-y-3">
          <input v-model="editForm.name" :placeholder="t('clients.placeholders.name')" class="input w-full" />
          <input v-model="editForm.email" type="email" :placeholder="t('clients.placeholders.email')" class="input w-full" />
        </div>
        <div class="flex justify-end gap-3 mt-5">
          <button class="btn-secondary" @click="editTarget = null">{{ t('clients.cancel') }}</button>
          <button class="btn-primary" :disabled="saving" @click="saveEdit">
            {{ saving ? t('clients.saving') : t('clients.save') }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal — Repor password -->
    <div v-if="resetTarget" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div class="bg-background-0 dark:bg-background-900 rounded-xl border border-background-200 dark:border-background-700 w-full max-w-md p-6">
        <h2 class="text-lg font-bold text-background-900 dark:text-background-50 mb-4">{{ t('clients.resetPasswordTitle') }}</h2>

        <div v-if="!generatedPassword">
          <p class="text-sm text-background-500 dark:text-background-400">{{ t('clients.resetPasswordConfirm') }}</p>
          <div class="flex justify-end gap-3 mt-5">
            <button class="btn-secondary" @click="resetTarget = null">{{ t('clients.cancel') }}</button>
            <button class="btn-primary" :disabled="saving" @click="doResetPassword">
              {{ saving ? t('clients.saving') : t('clients.reset') }}
            </button>
          </div>
        </div>

        <div v-else>
          <p class="text-sm text-background-500 dark:text-background-400 mb-2">{{ t('clients.temporaryPasswordNotice') }}</p>
          <div class="bg-background-100 dark:bg-background-800 rounded-lg px-3 py-2 font-mono text-sm text-center select-all">
            {{ generatedPassword }}
          </div>
          <div class="flex justify-end mt-5">
            <button class="btn-primary" @click="closeResetModal">{{ t('common.close') }}</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { clientPortalService, type ClientAccount } from '@/services/clientPortalService'

const { t } = useI18n()

const clients = ref<ClientAccount[]>([])
const loading = ref(true)
const saving = ref(false)
const showModal = ref(false)
const formError = ref('')
const form = ref({ name: '', email: '' })
const createdPassword = ref('')

const editTarget = ref<ClientAccount | null>(null)
const editForm = ref({ name: '', email: '' })

const resetTarget = ref<ClientAccount | null>(null)
const generatedPassword = ref('')

onMounted(load)

async function load() {
  loading.value = true
  try { clients.value = await clientPortalService.getClients() }
  finally { loading.value = false }
}

function closeModal() {
  showModal.value = false
  form.value = { name: '', email: '' }
  formError.value = ''
}

async function createClient() {
  if (!form.value.name || !form.value.email) {
    formError.value = t('clients.validationRequired')
    return
  }
  saving.value = true
  try {
    const result = await clientPortalService.createClient(form.value)
    createdPassword.value = result.temporaryPassword
    closeModal()
    await load()
  } catch (e: any) {
    formError.value = e?.response?.data || t('clients.errorCreate')
  } finally {
    saving.value = false
  }
}

function openEdit(client: ClientAccount) {
  editTarget.value = client
  editForm.value = { name: client.name, email: client.email }
}

async function saveEdit() {
  if (!editTarget.value) return
  saving.value = true
  try {
    await clientPortalService.updateClient(editTarget.value.id, editForm.value)
    editTarget.value = null
    await load()
  } finally {
    saving.value = false
  }
}

async function toggleStatus(client: ClientAccount) {
  await clientPortalService.toggleStatus(client.id)
  await load()
}

function openResetPassword(client: ClientAccount) {
  resetTarget.value = client
  generatedPassword.value = ''
}

async function doResetPassword() {
  if (!resetTarget.value) return
  saving.value = true
  try {
    const result = await clientPortalService.resetPassword(resetTarget.value.id)
    generatedPassword.value = result.temporaryPassword
  } finally {
    saving.value = false
  }
}

function closeResetModal() {
  resetTarget.value = null
  generatedPassword.value = ''
}
</script>