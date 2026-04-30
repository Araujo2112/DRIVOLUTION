<template>
  <div class="flex h-screen overflow-hidden">
    <aside class="w-72 bg-gray-50 border-r p-6 space-y-6">
      <h1 class="text-2xl font-bold">Manufacturing Orders</h1>

      <div class="space-y-2">
        <label for="orderSelect" class="block text-sm font-medium text-gray-700">
          Choose an order
        </label>
        <select
            id="orderSelect"
            v-model="selectedOrderId"
            class="block w-full rounded-md border-gray-300 shadow-sm focus:ring-blue-500 focus:border-blue-500"
        >
          <option
              v-for="o in orders"
              :key="o.id"
              :value="o.id"
          >
            {{ o.manufacturingOrderId }}
          </option>
        </select>
        <button @click="loadGraph">
          Load Graph
        </button>
      </div>

      <div v-if="error" class="text-red-600 text-sm">{{ error }}</div>
    </aside>

    <main class="flex-1 relative bg-white">
      <div
          ref="graphContainer"
          class="absolute inset-0 m-4 bg-gray-50 border rounded shadow-lg"
      ></div>
    </main>

    <transition name="fade">
      <div
          v-if="detailVisible"
          class="modal-overlay"
          @click.self="detailVisible = false"
      >
        <div class="modal-content">
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-lg font-semibold">{{ detailTitle }}</h2>
            <button @click="detailVisible = false" class="modal-close-btn">
              ×
            </button>
          </div>
          <div v-if="detailLoading" class="text-center py-6 italic text-gray-500">
            Loading...
          </div>

          <div v-else-if="currentNodeType === 'location'" class="location-details">
            <div class="section-card">
              <h3 class="section-title">Location Information</h3>
              <div class="info-grid">
                <div class="info-item">
                  <span class="info-label">Location ID:</span>
                  <span class="info-value">{{ detailData.locationInfo?.id || 'N/A' }}</span>
                </div>
                <div class="info-item">
                  <span class="info-label">Container ID:</span>
                  <span class="info-value">{{ detailData.locationInfo?.containerId || 'N/A' }}</span>
                </div>
                <div class="info-item">
                  <span class="info-label">Section ID:</span>
                  <span class="info-value">{{ detailData.locationInfo?.sectionId || 'N/A' }}</span>
                </div>
                <div class="info-item">
                  <span class="info-label">Date & Time:</span>
                  <span class="info-value">{{ formatDateTime(detailData.locationInfo?.datetime) }}</span>
                </div>
              </div>
            </div>

            <div class="section-card">
              <h3 class="section-title">Container Contents</h3>
              <div v-if="detailData.summary">
                <div class="bg-green-200 text-green-900 px-4 py-2 mb-3 rounded-md shadow font-bold text-base border border-green-400">
                  {{ detailData.summary.split(':')[0] }}
                </div>
                <div class="flex flex-wrap gap-2">
                  <div
                      v-for="item in detailData.summary.split(':')[1]?.split(',')"
                      :key="item"
                      class="bg-green-100 text-green-800 px-3 py-2 rounded-md shadow-md text-sm font-medium border border-green-300"
                  >
                    {{ item.trim() }}
                  </div>
                </div>
              </div>
              <div v-else class="bg-yellow-100 text-yellow-800 px-4 py-2 rounded font-semibold">
                Nenhum item presente neste contentor nesta localização.
              </div>
            </div>
          </div>

          <div v-else-if="currentNodeType === 'rawLot'" class="rawlot-details">
            <div class="section-card">
              <h3 class="section-title">Lot Information</h3>
              <div class="info-grid">
                <div class="info-item">
                  <span class="info-label">Lot ID:</span>
                  <span class="info-value">{{ detailData.lot?.lotId || 'N/A' }}</span>
                </div>
                <div class="info-item">
                  <span class="info-label">Lot Code:</span>
                  <span class="info-value">{{ detailData.lot?.lotCode || 'N/A' }}</span>
                </div>
              </div>
            </div>
          </div>

          <div v-else class="data-content">
            <div v-for="(value, key) in detailData" :key="key" class="data-item">
              <div class="data-label">{{ formatKey(key) }}</div>
              <div class="data-value">{{ formatValue(value) }}</div>
            </div>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>



<script lang="ts" setup>
import { ref, onMounted } from 'vue'
import cytoscape, { Core, ElementDefinition } from 'cytoscape'
import dagre from 'cytoscape-dagre'

cytoscape.use(dagre)

interface ManufacturingOrderDTO { id: number; manufacturingOrderId: string }
interface GraphNodeDTO {
  id: string;
  label: string;
  type: string;
  group: number;
  additionalData?: any;
}
interface GraphEdgeDTO { source: string; target: string; label: string; sequence: number }
interface GraphEdgeDTO { source: string; target: string; label: string; sequence: number }

const orders = ref<ManufacturingOrderDTO[]>([])
const selectedOrderId = ref<number|null>(null)
const error = ref<string|null>(null)
const graphContainer = ref<HTMLElement|null>(null)
let cy: Core|null = null

const detailVisible = ref(false)
const detailLoading = ref(false)
const detailTitle = ref('')
const detailData = ref<any>({})
const currentNodeType = ref<string>('')


function formatDateTime(dateString: string | undefined): string {
  if (!dateString) return 'N/A'
  try {
    return new Date(dateString).toLocaleString('pt-PT', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    })
  } catch {
    return dateString
  }
}


function formatKey(key: string): string {
  return key.replace(/([A-Z])/g, ' $1')
      .replace(/^./, str => str.toUpperCase())
      .trim()
}


function formatValue(value: any): string {
  if (value === null || value === undefined) return 'N/A'

  if (typeof value === 'object' && !Array.isArray(value)) {

    if ('name' in value && typeof value.name === 'string') {
      return value.name
    }

    if ('value' in value) {
      return String(value.value)
    }

    return JSON.stringify(value, null, 2)
  }

  if (typeof value === 'boolean') return value ? 'Yes' : 'No'

  if (typeof value === 'string' && value.match(/^\d{4}-\d{2}-\d{2}T/)) {
    return new Date(value).toLocaleString('pt-PT')
  }

  return String(value)
}


const endpointMap: Record<string,string> = {
  order: 'ManufacturingOrder',
  client: 'Client',
  productLot: 'ProductLot',
  process: 'ManufacturingProcess',
  processPhase: 'ManufacturingPhase',
  section: 'PlantFloorSection',
  orderPhaseInstance: 'ManufacturingOrderPhase',
  rawLot: 'LotOfRawMaterial',
  container: 'Container',
  location: 'ContainerLocalizationHistory',
}

const typeLayers: Record<string, number> = {
  order: 1,
  client: 2,
  productLot: 3,
  process: 4,
  processPhase: 5,
  section: 6,
  orderPhaseInstance: 7,
  rawLot: 8,
  container: 9,
  location: 10,
}

const typeIcons: Record<string, string> = {
  order: '/icons/box.svg',
  client: '/icons/user.svg',
  productLot: '/icons/list.svg',
  process: '/icons/settings.svg',
  processPhase: '/icons/tools.svg',
  section: '/icons/factory.svg',
  orderPhaseInstance: '/icons/loop.svg',
  rawLot: '/icons/block.svg',
  container: '/icons/container.svg',
  location: '/icons/marker.svg',
}

const typeLabels: Record<string, string> = {
  order: 'Order',
  client: 'Client',
  productLot: 'Product Lot',
  process: 'Process',
  processPhase: 'Process Phase',
  section: 'Section',
  orderPhaseInstance: 'Order Phase',
  rawLot: 'Raw Lot',
  container: 'Container',
  location: 'Location',
}

const typeColors: Record<string, string> = {
  order: '#FF6B6B',
  client: '#4ECDC4',
  productLot: '#45B7D1',
  process: '#96CEB4',
  processPhase: '#FFEAA7',
  section: '#DDA0DD',
  orderPhaseInstance: '#98D8C8',
  rawLot: '#F7DC6F',
  container: '#BB8FCE',
  location: '#85C1E9',
}

function getRawMaterialNode(rawId: number): any | undefined {
  return cy?.nodes().find(n =>
      n.data('type') === 'rawLot' &&
      n.data('additionalData')?.raw?.rawId === rawId
  )?.data('additionalData')
}

function getRawMaterialName(rawId: number): string {
  return getRawMaterialNode(rawId)?.raw?.name || `Raw #${rawId}`
}

function getRawMaterialInfo(rawId: number): string {
  return getRawMaterialNode(rawId)?.raw?.info || 'No description available'
}

function getRawMaterialSummary(rawId: number): string {
  return getRawMaterialNode(rawId)?.summary || 'No summary available'
}

function findConnectedContainerId(locationId: string): string {
  const edge = cy?.edges().toArray().find(e => e.data('target') === locationId && e.data('label') === 'localização')
  return edge?.data('source') || 'N/A'
}


function findConnectedSectionId(locationId: string): string {
  const edge = cy?.edges().toArray().find(e => e.data('source') === locationId && e.data('label') === 'secção')
  return edge?.data('target') || 'N/A'
}


async function fetchOrders() {
  try {
    const res = await fetch('http://localhost:5181/api/ManufacturingOrder')
    if (!res.ok) throw new Error()
    orders.value = await res.json()
    if (orders.value.length) selectedOrderId.value = orders.value[0].id
  } catch {
    error.value = 'Failed to fetch orders.'
  }
}

async function loadGraph() {
  if (!selectedOrderId.value) return
  error.value = null

  try {
    const res = await fetch(`http://localhost:5181/api/ManufacturingOrder/${selectedOrderId.value}/graph`)
    if (!res.ok) throw new Error()
    const { nodes: rawNodes, edges: rawEdges } = await res.json() as { nodes: GraphNodeDTO[], edges: GraphEdgeDTO[] }

    const nodes = rawNodes.filter(n => n.type !== 'itemLocation')

    const itemLocationIds = new Set(rawNodes.filter(n => n.type === 'itemLocation').map(n => n.id))
    const edges = rawEdges.filter(e =>
        !itemLocationIds.has(e.source) && !itemLocationIds.has(e.target)
    )

    const elements: ElementDefinition[] = []
    const layerMap: Record<string, string[]> = {}



    for (const node of nodes) {
      const group = typeLayers[node.type] || 0
      const label = `${node.label}\n(${typeLabels[node.type] || node.type})`

      elements.push({
        data: {
          id: node.id,
          label,
          type: node.type,
          group,
          icon: typeIcons[node.type] || '',
          additionalData: node.additionalData
        }
      })

      if (!layerMap[node.type]) layerMap[node.type] = []
      console.log('[DEBUG] location node additionalData:', node.additionalData)
      layerMap[node.type].push(node.id)
    }

    for (const edge of edges) {
      elements.push({ data: {
          id: `e${edge.sequence}`,
          source: edge.source,
          target: edge.target,
          label: edge.label
        }})
    }

    for (const [type, ids] of Object.entries(layerMap)) {
      const label = typeLabels[type] || type
      elements.push({
        data: {
          id: `layer-${type}`,
          label,
        },
        classes: 'layer-box'
      })
      for (const id of ids) {
        const el = elements.find(e => e.data.id === id)
        if (el) el.data.parent = `layer-${type}`
      }
    }

    if (cy) cy.destroy()
    cy = cytoscape({
      container: graphContainer.value!,
      elements,
      style: [
        {
          selector: 'node',
          style: {
            'shape': 'roundrectangle',
            'width': 180,
            'height': 180,
            'background-color': ele => typeColors[ele.data('type')] || '#ccc',
            'border-width': 3,
            'border-color': '#333',

            'background-image': ele => ele.data('icon') || 'none',
            'background-fit': 'contain',
            'background-opacity': 0.15,
            'background-position-x': '50%',
            'background-position-y': '50%',
            'background-clip': 'node',

            'label': 'data(label)',
            'font-size': '34px',
            'font-weight': 'bold',
            'text-wrap': 'wrap',
            'text-valign': 'center',
            'text-halign': 'center',
            'color': '#111',
            'text-outline-width': 4,
            'text-outline-color': '#fff',
            'text-max-width': '170px'
          }
        },
        {
          selector: 'edge',
          style: {
            'curve-style': 'bezier',
            'mid-target-arrow-shape': 'triangle',
            'mid-target-arrow-color': '#555',
            'line-color': '#bbb',
            'label': 'data(label)',
            'font-size': '22px',
            'font-weight': 'bold',
            'text-rotation': 'autorotate',
            'text-margin-y': -10,
            'color': '#333'
          }
        },
        {
          selector: '$node > node',
          style: {
            'background-color': '#f3f4f6',
            'border-color': '#999',
            'border-width': 2,
            'shape': 'roundrectangle',
            'padding': '35px',
            'label': 'data(label)',
            'text-valign': 'top',
            'text-halign': 'center',
            'font-size': '30px',
            'font-weight': 'bold',
            'text-margin-y': -25,
            'background-image': 'none',
            'color': '#333'
          }
        }
      ],
      layout: {
        name: 'dagre',
        rankDir: 'LR',
        nodeSep: 120,
        edgeSep: 60,
        rankSep: 320
      }
    })

    cy.on('tap', 'node', async evt => {
      const d = evt.target.data() as any

      if (!d.id) return
      if (d.id.startsWith('layer-')) return
      if (d.type === 'history') return

      const nodeType = d.type
      currentNodeType.value = nodeType

      let numericId = d.id
      if (d.id.includes('-')) {
        const parts = d.id.split('-')
        numericId = parts[parts.length - 1]
      }

      detailTitle.value = d.label
      detailVisible.value = true
      detailLoading.value = true

      try {
        if (nodeType === 'location') {

          detailData.value = {
            locationInfo: {
              id: d.id,
              containerId: findConnectedContainerId(d.id),
              sectionId: findConnectedSectionId(d.id),
              datetime: d.label
            },
            summary: d.additionalData?.summary || ''
          }
        } else if (nodeType === 'rawLot') {
          detailData.value = d.additionalData || { error: 'No additional data available for this node' }
        } else {
          const ep = endpointMap[nodeType]

          if (!ep) {
            console.log('No endpoint found for type:', nodeType)
            detailData.value = { error: 'No endpoint configured for this type' }
            return
          }

          const url = `http://localhost:5181/api/${ep}/${numericId}`
          const r = await fetch(url)

          if (!r.ok) {
            throw new Error(`HTTP ${r.status}`)
          }

          detailData.value = await r.json()
        }
      } catch (error) {
        console.error('Error loading details:', error)
        detailData.value = { error: 'Failed to load details' }
      } finally {
        detailLoading.value = false
      }
    })

  } catch {
    error.value = 'Failed to load graph.'
  }
}

onMounted(async () => {
  await fetchOrders()

  const params = new URLSearchParams(window.location.search)
  const orderIdFromUrl = params.get('orderId')
  if (orderIdFromUrl) {
    selectedOrderId.value = parseInt(orderIdFromUrl)
    await loadGraph()
  }
})

</script>

<style scoped>
button {
  @apply w-full flex items-center justify-center gap-1 rounded-lg px-4 py-3 text-base text-center bg-primary-500 text-white hover:bg-primary-400 transition-colors duration-150 ease-in-out;
}

button.danger {
  @apply bg-red-500 text-white hover:bg-red-600;
}

button.empty {
  @apply bg-transparent text-black hover:bg-black/20;
}

.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from, .fade-leave-to {
  opacity: 0;
}


.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 0.5rem;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.3);
  padding: 1.5rem;
  max-width: 80vw;
  max-height: 80vh;
  overflow: auto;
  margin: 20px;
  width: 100%;
  min-width: 500px;
}

.modal-close-btn {
  background: none !important;
  border: none;
  font-size: 24px;
  cursor: pointer;
  color: #666;
  padding: 0 !important;
  width: auto !important;
  height: auto !important;
  min-height: auto !important;
  line-height: 1;
}

.modal-close-btn:hover {
  color: #333;
  background: none !important;
}

.location-details, .rawlot-details {
  max-height: 60vh;
  overflow-y: auto;
  space-y: 1rem;
}

.section-card {
  background: #f8f9fa;
  border: 1px solid #e9ecef;
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 1rem;
}

.section-title {
  font-size: 1.1rem;
  font-weight: 600;
  color: #495057;
  margin-bottom: 0.75rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.info-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #6c757d;
}

.info-value {
  font-size: 0.95rem;
  color: #212529;
  font-weight: 500;
}

.summary-badge {
  display: inline-block;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  font-size: 0.875rem;
  font-weight: 500;
  margin-bottom: 1rem;
}

.badge-success {
  background: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

.badge-warning {
  background: #fff3cd;
  color: #856404;
  border: 1px solid #ffeaa7;
}

.items-list {
  space-y: 0.75rem;
}

.item-card {
  background: white;
  border: 1px solid #dee2e6;
  border-radius: 6px;
  padding: 0.75rem;
}

.item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.item-id {
  font-weight: 600;
  color: #495057;
}

.item-quantity {
  background: #e9ecef;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.875rem;
  font-weight: 500;
}

.item-details {
  space-y: 0.25rem;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.875rem;
}

.detail-label {
  color: #6c757d;
  font-weight: 500;
}

.detail-value {
  color: #212529;
  font-family: monospace;
}

.empty-state {
  text-align: center;
  padding: 2rem;
  color: #6c757d;
}

.empty-message {
  font-style: italic;
}


.raw-material-card {
  background: white;
  border: 1px solid #dee2e6;
  border-radius: 6px;
  padding: 1rem;
}

.raw-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.raw-name {
  font-size: 1.1rem;
  font-weight: 600;
  color: #495057;
  margin: 0;
}

.raw-id {
  background: #e9ecef;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.875rem;
  font-family: monospace;
}

.raw-info {
  margin-top: 0.5rem;
}

.raw-description {
  color: #6c757d;
  font-style: italic;
  margin: 0;
}

.summary-text {
  font-size: 1rem;
  color: #495057;
  margin: 0;
  font-weight: 500;
}


.data-content {
  max-height: 60vh;
  overflow-y: auto;
  padding: 0.5rem;
}

.data-item {
  display: flex;
  padding: 0.75rem 0;
  border-bottom: 1px solid #e5e7eb;
}

.data-item:last-child {
  border-bottom: none;
}

.data-label {
  font-weight: 600;
  color: #374151;
  min-width: 150px;
  flex-shrink: 0;
  margin-right: 1rem;
}

.data-value {
  color: #6b7280;
  word-break: break-word;
  flex: 1;
}

.data-content::-webkit-scrollbar,
.location-details::-webkit-scrollbar,
.rawlot-details::-webkit-scrollbar {
  width: 8px;
}

.data-content::-webkit-scrollbar-track,
.location-details::-webkit-scrollbar-track,
.rawlot-details::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 4px;
}§

.data-content::-webkit-scrollbar-thumb,
.location-details::-webkit-scrollbar-thumb,
.rawlot-details::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 4px;
}

.data-content::-webkit-scrollbar-thumb:hover,
.location-details::-webkit-scrollbar-thumb:hover,
.rawlot-details::-webkit-scrollbar-thumb:hover {
  background: #a1a1a1;
}
</style>
