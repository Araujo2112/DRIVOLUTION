<template>
  <div class="p-8 w-full">

    <!-- Header -->
    <div class="flex items-start justify-between mb-8">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('supports.title') }}
        </h1>
        <p class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('supports.subtitle') }}
        </p>
      </div>
      <button
        @click="openCreateSupport"
        class="flex items-center gap-2 bg-primary-500 hover:bg-primary-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
      >
        <span class="material-symbols-rounded text-base">add</span>
        {{ t('supports.newSupport') }}
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center gap-2 text-background-500 text-sm py-12">
      <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
      {{ t('common.loading') }}
    </div>

    <div v-else>
      <!-- Empty -->
      <div v-if="supports.length === 0" class="text-center py-16 text-background-500">
        <span class="material-symbols-rounded text-5xl block mb-3">conveyor_belt</span>
        <p class="text-sm">{{ t('supports.empty') }}</p>
      </div>

      <!-- Tabela -->
      <div v-else class="border border-background-300 dark:border-background-700 rounded-xl overflow-hidden">

        <!-- Header tabela -->
        <div class="grid grid-cols-12 px-4 py-3 bg-background-100 dark:bg-background-800 border-b border-background-300 dark:border-background-700">
          <span class="col-span-2 text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('supports.fields.rfid') }}</span>
          <span class="col-span-1 text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('supports.fields.type') }}</span>
          <span class="col-span-2 text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('supports.fields.line') }}</span>
          <span class="col-span-2 text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('supports.fields.status') }}</span>
          <span class="col-span-3 text-xs font-medium text-background-500 uppercase tracking-wider">{{ t('supports.fields.product') }}</span>
          <span class="col-span-2 text-xs font-medium text-background-500 uppercase tracking-wider text-right">{{ t('common.actions') }}</span>
        </div>

        <!-- Linhas -->
        <div
          v-for="support in supports"
          :key="support.id"
          class="grid grid-cols-12 px-4 py-3 border-b border-background-200 dark:border-background-700 last:border-0 bg-background-50 dark:bg-background-800 hover:bg-background-100 dark:hover:bg-background-750 transition-colors items-center"
        >
          <!-- RFID Tag -->
          <div class="col-span-2">
            <span v-if="support.rfidTag" class="flex items-center gap-1.5 text-sm font-mono text-background-800 dark:text-background-100">
              <span class="material-symbols-rounded text-base text-primary-500">nfc</span>
              {{ support.rfidTag }}
            </span>
            <span v-else class="text-xs text-background-400 italic">{{ t('supports.noTag') }}</span>
          </div>

          <!-- Tipo -->
          <div class="col-span-1">
            <span class="text-sm text-background-600 dark:text-background-400">{{ support.type ?? '—' }}</span>
          </div>

          <!-- Linha -->
          <div class="col-span-2">
            <span class="text-sm text-background-600 dark:text-background-400">
              {{ lineNameById(support.productionLineId) }}
            </span>
          </div>

          <!-- Estado ocupado/livre -->
          <div class="col-span-2">
            <div v-if="currentBySupport[support.id] === undefined" class="flex items-center gap-1 text-xs text-background-400">
              <span class="material-symbols-rounded animate-spin text-sm">autorenew</span>
            </div>
            <span
              v-else-if="currentBySupport[support.id]"
              class="inline-flex items-center gap-1 text-xs font-medium px-2 py-1 rounded-full bg-warning-100 text-warning-700"
            >
              <span class="w-1.5 h-1.5 rounded-full bg-warning-500"></span>
              {{ t('supports.occupied') }}
            </span>
            <span v-else class="inline-flex items-center gap-1 text-xs font-medium px-2 py-1 rounded-full bg-success-100 text-success-700">
              <span class="w-1.5 h-1.5 rounded-full bg-success-500"></span>
              {{ t('supports.free') }}
            </span>
          </div>

          <!-- Produto atual -->
          <div class="col-span-3">
            <div v-if="currentBySupport[support.id]">
              <div class="text-sm font-medium text-background-900 dark:text-background-50">
                {{ currentBySupport[support.id]!.serialNumber ?? `ID #${currentBySupport[support.id]!.productId}` }}
              </div>
              <div class="text-xs text-background-400">{{ currentBySupport[support.id]!.modelName }}</div>
            </div>
            <span v-else class="text-xs text-background-400">—</span>
          </div>

          <!-- Ações -->
          <div class="col-span-2 flex justify-end gap-1">
            <!-- Associar produto (se livre) -->
            <button
              v-if="!currentBySupport[support.id]"
              @click="openAssociate(support)"
              class="flex items-center gap-1 text-xs px-2 py-1.5 rounded-lg bg-primary-500 hover:bg-primary-600 text-white font-medium transition-colors"
            >
              <span class="material-symbols-rounded text-sm">link</span>
              {{ t('supports.associate') }}
            </button>

            <!-- Libertar (se ocupado) -->
            <button
              v-if="currentBySupport[support.id]"
              @click="releaseSupport(support.id)"
              class="flex items-center gap-1 text-xs px-2 py-1.5 rounded-lg border border-warning-500 text-warning-600 hover:bg-warning-50 dark:hover:bg-background-700 font-medium transition-colors"
            >
              <span class="material-symbols-rounded text-sm">link_off</span>
              {{ t('supports.release') }}
            </button>

            <!-- Apagar suporte (só se livre) -->
            <button
              v-if="!currentBySupport[support.id]"
              @click="deleteSupport(support)"
              class="p-1.5 rounded-lg text-background-400 hover:text-danger-500 hover:bg-danger-100 dark:hover:bg-background-700 transition-colors"
            >
              <span class="material-symbols-rounded text-base">delete</span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal: Criar Suporte -->
    <div v-if="showCreateModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50" @click.self="showCreateModal = false">
      <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl w-full max-w-md overflow-hidden">
        <div class="flex items-center justify-between px-5 py-4 border-b border-background-300 dark:border-background-700">
          <h2 class="text-base font-medium text-background-900 dark:text-background-50">{{ t('supports.newSupport') }}</h2>
          <button @click="showCreateModal = false" class="text-background-500 hover:text-background-700 dark:hover:text-background-300">
            <span class="material-symbols-rounded">close</span>
          </button>
        </div>
        <div class="px-5 py-4 flex flex-col gap-4">
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('supports.fields.line') }} *</label>
            <select v-model="createForm.productionLineId">
              <option :value="0" disabled>{{ t('supports.fields.linePlaceholder') }}</option>
              <option v-for="line in lines" :key="line.id" :value="line.id">{{ line.name }}</option>
            </select>
          </div>
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('supports.fields.rfid') }}</label>
            <input v-model="createForm.rfidTag" type="text" :placeholder="t('supports.fields.rfidPlaceholder')" />
            <p class="text-xs text-background-400">{{ t('supports.rfidHint') }}</p>
          </div>
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('supports.fields.type') }}</label>
            <input v-model="createForm.type" type="text" :placeholder="t('supports.fields.typePlaceholder')" />
          </div>
        </div>
        <div class="flex justify-end gap-2 px-5 py-4 border-t border-background-300 dark:border-background-700">
          <button @click="showCreateModal = false" class="px-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-600 text-background-700 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors">
            {{ t('common.cancel') }}
          </button>
          <button @click="submitCreateSupport" :disabled="!createForm.productionLineId" class="px-4 py-2 text-sm rounded-lg bg-primary-500 hover:bg-primary-600 disabled:opacity-50 disabled:cursor-not-allowed text-white font-medium transition-colors">
            {{ t('common.save') }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal: Associar Produto -->
    <div v-if="showAssociateModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50" @click.self="showAssociateModal = false">
      <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl w-full max-w-md overflow-hidden">
        <div class="flex items-center justify-between px-5 py-4 border-b border-background-300 dark:border-background-700">
          <h2 class="text-base font-medium text-background-900 dark:text-background-50">{{ t('supports.associateTitle') }}</h2>
          <button @click="showAssociateModal = false" class="text-background-500 hover:text-background-700 dark:hover:text-background-300">
            <span class="material-symbols-rounded">close</span>
          </button>
        </div>
        <div class="px-5 py-4 flex flex-col gap-4">

          <!-- Info do suporte selecionado -->
          <div class="flex items-center gap-3 p-3 rounded-lg bg-background-100 dark:bg-background-700 border border-background-300 dark:border-background-600">
            <span class="material-symbols-rounded text-primary-500">nfc</span>
            <div>
              <div class="text-sm font-medium text-background-900 dark:text-background-50">
                {{ associateForm.selectedSupport?.rfidTag ?? t('supports.noTag') }}
              </div>
              <div class="text-xs text-background-400">
                {{ associateForm.selectedSupport?.type ?? '—' }} · {{ lineNameById(associateForm.selectedSupport?.productionLineId ?? 0) }}
              </div>
            </div>
          </div>

          <!-- Dropdown de produto -->
          <div class="flex flex-col gap-1.5">
            <label class="text-xs font-medium text-background-600 dark:text-background-400">{{ t('supports.fields.product') }} *</label>
            <select v-model="associateForm.productId">
              <option :value="0" disabled>{{ t('supports.fields.productPlaceholder') }}</option>
              <option v-for="product in freeProducts" :key="product.id" :value="product.id">
                {{ product.serialNumber ?? `ID #${product.id}` }} — {{ product.modelName }}
              </option>
            </select>
            <p v-if="freeProducts.length === 0" class="text-xs text-background-400">{{ t('supports.noFreeProducts') }}</p>
          </div>
        </div>
        <div class="flex justify-end gap-2 px-5 py-4 border-t border-background-300 dark:border-background-700">
          <button @click="showAssociateModal = false" class="px-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-600 text-background-700 dark:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors">
            {{ t('common.cancel') }}
          </button>
          <button @click="submitAssociate" :disabled="!associateForm.productId" class="px-4 py-2 text-sm rounded-lg bg-primary-500 hover:bg-primary-600 disabled:opacity-50 disabled:cursor-not-allowed text-white font-medium transition-colors">
            {{ t('supports.associateBtn') }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { supportService, supportedProductService } from '@/services/supportService'
import type { Support, SupportedProduct } from '@/services/supportService'
import { productionLineService } from '@/services/productionLineService'
import type { ProductionLine } from '@/services/productionLineService'
import { productService } from '@/services/productService'
import type { Product } from '@/services/productService'
import { toast } from '@/plugins/toast'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

// ── Estado ─────────────────────────────────────────────────────────────────────
const loading = ref(true)
const supports = ref<Support[]>([])
const lines = ref<ProductionLine[]>([])
const allProducts = ref<Product[]>([])

// current: undefined = ainda a carregar, null = livre, SupportedProduct = ocupado
const currentBySupport = reactive<Record<number, SupportedProduct | null | undefined>>({})

// ── Modais ─────────────────────────────────────────────────────────────────────
const showCreateModal = ref(false)
const showAssociateModal = ref(false)

const createForm = reactive<{ productionLineId: number; rfidTag: string; type: string }>({
  productionLineId: 0,
  rfidTag: '',
  type: '',
})

const associateForm = reactive<{ selectedSupport: Support | null; productId: number }>({
  selectedSupport: null,
  productId: 0,
})

// ── Computed ───────────────────────────────────────────────────────────────────
// Produtos que não estão associados a nenhum suporte ativo
const occupiedProductIds = computed(() =>
  Object.values(currentBySupport)
    .filter(Boolean)
    .map(sp => sp!.productId)
)

const freeProducts = computed(() =>
  allProducts.value.filter(p => !occupiedProductIds.value.includes(p.id))
)

function lineNameById(id: number): string {
  return lines.value.find(l => l.id === id)?.name ?? `Linha #${id}`
}

// ── Lifecycle ──────────────────────────────────────────────────────────────────
onMounted(async () => {
  await Promise.all([loadSupports(), loadLines(), loadProducts()])
})

// ── Loaders ────────────────────────────────────────────────────────────────────
async function loadSupports() {
  loading.value = true
  try {
    const res = await supportService.getAll()
    const raw = res.data as any
    supports.value = raw?.$values ?? res.data ?? []
    // Carregar estado atual de cada suporte em paralelo
    await Promise.all(supports.value.map(s => loadCurrentForSupport(s.id)))
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
  }
}

async function loadCurrentForSupport(supportId: number) {
  currentBySupport[supportId] = undefined // loading
  try {
    const res = await supportedProductService.getCurrent(supportId)
    currentBySupport[supportId] = res.data
  } catch (err: any) {
    // 404 = livre
    if (err?.response?.status === 404) {
      currentBySupport[supportId] = null
    }
  }
}

async function loadLines() {
  try {
    const res = await productionLineService.getAll()
    lines.value = res.data
  } catch {}
}

async function loadProducts() {
  try {
    const res = await productService.getAll()
    const raw = res.data as any
    allProducts.value = raw?.$values ?? res.data ?? []
  } catch {}
}

// ── Criar suporte ──────────────────────────────────────────────────────────────
function openCreateSupport() {
  createForm.productionLineId = 0
  createForm.rfidTag = ''
  createForm.type = ''
  showCreateModal.value = true
}

async function submitCreateSupport() {
  try {
    await supportService.create({
      productionLineId: createForm.productionLineId,
      rfidTag: createForm.rfidTag || null,
      type: createForm.type || null,
    })
    showCreateModal.value = false
    toast.success(t('supports.created'))
    await loadSupports()
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}

async function deleteSupport(support: Support) {
  try {
    await supportService.delete(support.id)
    toast.success(t('supports.deleted'))
    await loadSupports()
  } catch {
    toast.error(t('errors.deleteFailed'))
  }
}

// ── Associar produto ───────────────────────────────────────────────────────────
function openAssociate(support: Support) {
  associateForm.selectedSupport = support
  associateForm.productId = 0
  showAssociateModal.value = true
}

async function submitAssociate() {
  try {
    await supportedProductService.associate({
      supportId: associateForm.selectedSupport!.id,
      productId: associateForm.productId,
    })
    showAssociateModal.value = false
    toast.success(t('supports.associated'))
    await loadCurrentForSupport(associateForm.selectedSupport!.id)
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}

// ── Libertar suporte ───────────────────────────────────────────────────────────
async function releaseSupport(supportId: number) {
  const current = currentBySupport[supportId]
  if (!current) return
  try {
    await supportedProductService.release(current.id)
    toast.success(t('supports.released'))
    await loadCurrentForSupport(supportId)
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}
</script>