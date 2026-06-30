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

    <!-- Filtros -->
    <div class="flex gap-3 mb-6 items-center">

      <!-- Search tag / tipo -->
      <div class="relative w-64 shrink-0">
        <span class="material-symbols-rounded absolute left-3 top-1/2 -translate-y-1/2 text-background-400 text-base pointer-events-none">search</span>
        <input
          v-model="search"
          @input="onSearchInput"
          type="text"
          :placeholder="t('supports.searchPlaceholder')"
          class="w-full pl-9 pr-4 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 bg-background-50 dark:bg-background-800 text-background-900 dark:text-background-50 placeholder-background-400 focus:outline-none focus:border-primary-400"
        />
      </div>

      <!-- Linha -->
      <select v-model="lineFilter" @change="onFilterChange" class="w-44 shrink-0">
        <option value="">{{ t('supports.allLines') }}</option>
        <option v-for="line in lines" :key="line.id" :value="line.id">{{ line.name }}</option>
      </select>

      <!-- Estado -->
      <select v-model="statusFilter" @change="onFilterChange" class="w-40 shrink-0">
        <option value="">{{ t('supports.allStatus') }}</option>
        <option value="occupied">{{ t('supports.occupied') }}</option>
        <option value="free">{{ t('supports.free') }}</option>
      </select>

      <!-- Limpar -->
      <button
        v-if="search || lineFilter || statusFilter"
        @click="clearFilters"
        class="shrink-0 px-3 py-2 text-sm rounded-lg border border-background-300 dark:border-background-700 text-background-500 hover:text-background-700 dark:hover:text-background-300 hover:bg-background-100 dark:hover:bg-background-700 transition-colors"
        title="Limpar filtros"
      >
        <span class="material-symbols-rounded text-base align-middle">close</span>
      </button>

      <!-- Registos por página -->
      <select v-model="pageSize" @change="onPageSizeChange" class="ml-auto w-20 shrink-0">
        <option :value="25">25</option>
        <option :value="50">50</option>
        <option :value="100">100</option>
      </select>
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
            <span class="text-sm text-background-600 dark:text-background-400">{{ support.productionLineName }}</span>
          </div>

          <!-- Estado ocupado/livre -->
          <div class="col-span-2">
            <span
              v-if="support.isOccupied"
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
            <div v-if="support.isOccupied">
              <div class="text-sm font-medium text-background-900 dark:text-background-50">
                {{ support.currentSerialNumber ?? `ID #${support.currentProductId}` }}
              </div>
              <div class="text-xs text-background-400">{{ support.currentModelName }}</div>
            </div>
            <span v-else class="text-xs text-background-400">—</span>
          </div>

          <!-- Ações -->
          <div class="col-span-2 flex justify-end gap-1">
            <button
              v-if="!support.isOccupied"
              @click="openAssociate(support)"
              class="flex items-center gap-1 text-xs px-2 py-1.5 rounded-lg bg-primary-500 hover:bg-primary-600 text-white font-medium transition-colors"
            >
              <span class="material-symbols-rounded text-sm">link</span>
              {{ t('supports.associate') }}
            </button>

            <button
              v-if="support.isOccupied"
              @click="releaseSupport(support)"
              class="flex items-center gap-1 text-xs px-2 py-1.5 rounded-lg border border-warning-500 text-warning-600 hover:bg-warning-50 dark:hover:bg-background-700 font-medium transition-colors"
            >
              <span class="material-symbols-rounded text-sm">link_off</span>
              {{ t('supports.release') }}
            </button>

            <button
              v-if="!support.isOccupied"
              @click="deleteSupport(support)"
              class="p-1.5 rounded-lg text-background-400 hover:text-danger-500 hover:bg-danger-100 dark:hover:bg-background-700 transition-colors"
            >
              <span class="material-symbols-rounded text-base">delete</span>
            </button>
          </div>
        </div>
      </div>

      <!-- Paginação -->
      <div v-if="totalPages > 1" class="flex items-center justify-between mt-4 text-sm text-background-600 dark:text-background-400">
        <span>{{ t('common.showing', { from: (currentPage - 1) * pageSize + 1, to: Math.min(currentPage * pageSize, total), total }) }}</span>
        <div class="flex gap-1">
          <button
            @click="goToPage(currentPage - 1)"
            :disabled="currentPage === 1"
            class="px-3 py-1.5 rounded-lg border border-background-300 dark:border-background-700 hover:bg-background-100 dark:hover:bg-background-700 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          >
            <span class="material-symbols-rounded text-base">chevron_left</span>
          </button>
          <button
            v-for="p in visiblePages"
            :key="p"
            @click="goToPage(p)"
            class="px-3 py-1.5 rounded-lg border transition-colors"
            :class="p === currentPage ? 'bg-primary-500 text-white border-primary-500' : 'border-background-300 dark:border-background-700 hover:bg-background-100 dark:hover:bg-background-700'"
          >
            {{ p }}
          </button>
          <button
            @click="goToPage(currentPage + 1)"
            :disabled="currentPage === totalPages"
            class="px-3 py-1.5 rounded-lg border border-background-300 dark:border-background-700 hover:bg-background-100 dark:hover:bg-background-700 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          >
            <span class="material-symbols-rounded text-base">chevron_right</span>
          </button>
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

          <div class="flex items-center gap-3 p-3 rounded-lg bg-background-100 dark:bg-background-700 border border-background-300 dark:border-background-600">
            <span class="material-symbols-rounded text-primary-500">nfc</span>
            <div>
              <div class="text-sm font-medium text-background-900 dark:text-background-50">
                {{ associateForm.selectedSupport?.rfidTag ?? t('supports.noTag') }}
              </div>
              <div class="text-xs text-background-400">
                {{ associateForm.selectedSupport?.type ?? '—' }} · {{ associateForm.selectedSupport?.productionLineName }}
              </div>
            </div>
          </div>

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
import type { SupportPaged } from '@/services/supportService'
import { productionLineService } from '@/services/productionLineService'
import type { ProductionLine } from '@/services/productionLineService'
import { productService } from '@/services/productService'
import type { Product } from '@/services/productService'
import { toast } from '@/plugins/toast'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

// ── Estado ─────────────────────────────────────────────────────────────────────
const loading = ref(true)
const supports = ref<SupportPaged[]>([])
const lines = ref<ProductionLine[]>([])
const allProducts = ref<Product[]>([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(25)

// Filtros
const search = ref('')
const lineFilter = ref<number | ''>('')
const statusFilter = ref('')

// ── Modais ─────────────────────────────────────────────────────────────────────
const showCreateModal = ref(false)
const showAssociateModal = ref(false)

const createForm = reactive<{ productionLineId: number; rfidTag: string; type: string }>({
  productionLineId: 0,
  rfidTag: '',
  type: '',
})

const associateForm = reactive<{ selectedSupport: SupportPaged | null; productId: number }>({
  selectedSupport: null,
  productId: 0,
})

// ── Computed ───────────────────────────────────────────────────────────────────
const totalPages = computed(() => Math.ceil(total.value / pageSize.value))
const visiblePages = computed(() => {
  const pages: number[] = []
  const start = Math.max(1, currentPage.value - 2)
  const end = Math.min(totalPages.value, currentPage.value + 2)
  for (let i = start; i <= end; i++) pages.push(i)
  return pages
})

// Produtos livres = todos menos os já ocupados na página atual.
// Nota: como a listagem é paginada, isto reflete apenas os ocupados visíveis;
// para um filtro 100% exato seria preciso um endpoint dedicado de produtos livres.
const occupiedProductIds = computed(() =>
  supports.value.filter(s => s.isOccupied).map(s => s.currentProductId)
)

const freeProducts = computed(() =>
  allProducts.value.filter(p => !occupiedProductIds.value.includes(p.id))
)

// ── Lifecycle ──────────────────────────────────────────────────────────────────
let debounceTimer: ReturnType<typeof setTimeout>
function onSearchInput() {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    currentPage.value = 1
    loadSupports()
  }, 300)
}

onMounted(async () => {
  await Promise.all([loadSupports(), loadLines(), loadProducts()])
})

// ── Loaders ────────────────────────────────────────────────────────────────────
async function loadSupports() {
  loading.value = true
  try {
    const res = await supportService.getPaged({
      page: currentPage.value,
      pageSize: pageSize.value,
      search: search.value || undefined,
      productionLineId: lineFilter.value || undefined,
      occupied: statusFilter.value === 'occupied' ? true : statusFilter.value === 'free' ? false : undefined,
    })
    supports.value = res.data.data
    total.value = res.data.total
  } catch {
    toast.error(t('errors.loadFailed'))
  } finally {
    loading.value = false
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
    const res = await productService.getPaged({ page: 1, pageSize: 1000 })
    allProducts.value = res.data.data
  } catch {}
}

function onFilterChange() {
  currentPage.value = 1
  loadSupports()
}

function onPageSizeChange() {
  currentPage.value = 1
  loadSupports()
}

function goToPage(page: number) {
  if (page < 1 || page > totalPages.value) return
  currentPage.value = page
  loadSupports()
}

function clearFilters() {
  search.value = ''
  lineFilter.value = ''
  statusFilter.value = ''
  currentPage.value = 1
  loadSupports()
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

async function deleteSupport(support: SupportPaged) {
  try {
    await supportService.delete(support.id)
    toast.success(t('supports.deleted'))
    await loadSupports()
  } catch {
    toast.error(t('errors.deleteFailed'))
  }
}

// ── Associar produto ───────────────────────────────────────────────────────────
function openAssociate(support: SupportPaged) {
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
    await loadSupports()
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}

// ── Libertar suporte ───────────────────────────────────────────────────────────
async function releaseSupport(support: SupportPaged) {
  try {
    // getCurrent ainda é necessário aqui: a listagem paginada não devolve o id
    // do registo SupportedProduct (só os dados do produto), e é esse id que a
    // rota de libertar precisa.
    const res = await supportedProductService.getCurrent(support.id)
    await supportedProductService.release(res.data.id)
    toast.success(t('supports.released'))
    await loadSupports()
  } catch {
    toast.error(t('errors.saveFailed'))
  }
}
</script>