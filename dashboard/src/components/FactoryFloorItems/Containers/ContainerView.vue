<template>
  <div class="dashboard-container">
    <div class="content">
      <div v-if="loading" class="loading-container">
        <div class="loading-spinner">
          <div class="spinner-ring"></div>
          <div class="spinner-core"></div>
        </div>
        <p class="loading-text">Carregando dados do contentor...</p>
      </div>

      <div v-if="error" class="error-container">
        <div class="error-icon">
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-alert-circle"><circle cx="12" cy="12" r="10"/><line x1="12" x2="12" y1="8" y2="12"/><line x1="12" x2="12.01" y1="16" y2="16"/></svg>
        </div>
        <p>{{ error }}</p>
      </div>

      <div v-if="selectedContainer" class="container-details">

        <div class="container-header">
          <div class="container-title-wrapper">
            <h1 class="container-title">{{ selectedContainer.container?.containerName?.name || 'Unknown' }}</h1>
            <p class="container-subtitle">Detalhes e conteúdo do contentor</p>
          </div>
          <div class="container-badge" :class="{ 'active': selectedContainer.container?.activate }">
            {{ selectedContainer.container?.activate ? 'Ativo' : 'Inativo' }}
          </div>
        </div>

        <div class="info-cards">
          <div class="info-card">
            <div class="card-header">
              <div class="card-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-cube"><path d="m21 7.5-9-5.25L3 7.5m18 0v9l-9 5.25m9-14.25-9 5.25m0 0L3 7.5m9 5.25v9"/></svg>
              </div>
              <h3 class="card-title">ID do Contentor</h3>
            </div>
            <div class="card-content">
              <p class="card-value">{{ selectedContainer.container?.containerId || 'N/A' }}</p>
              <p class="card-description">Identificador único</p>
            </div>
          </div>

          <div class="info-card">
            <div class="card-header">
              <div class="card-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-container"><path d="M22 7.7c0-.6-.4-1.2-.8-1.5l-6.3-3.9a1.72 1.72 0 0 0-1.7 0l-10.3 6c-.5.2-.9.8-.9 1.4v6.6c0 .5.4 1.2.8 1.5l6.3 3.9a1.72 1.72 0 0 0 1.7 0l10.3-6c.5-.3.9-.9.9-1.4Z"/><path d="M10 21.9V14"/><path d="M2 8.4l10 6.1"/><path d="m12 14.5 10-6.1"/></svg>
              </div>
              <h3 class="card-title">Volume Total</h3>
            </div>
            <div class="card-content">
              <div class="card-value-with-unit">
                <span class="card-value">{{ selectedContainer.container?.containerVolume ?? 'N/A' }}</span>
                <span class="card-unit">L</span>
              </div>
              <p class="card-description">Capacidade máxima</p>
            </div>
          </div>

          <div class="info-card">
            <div class="card-header">
              <div class="card-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-check-circle"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
              </div>
              <h3 class="card-title">Estado</h3>
            </div>
            <div class="card-content">
              <div class="utilization">
                <div class="utilization-header">
                  <span class="utilization-text">78% Utilizado</span>
                  <span class="utilization-total">{{ getTotalQuantity() }} {{ getUnitType() }}</span>
                </div>
                <div class="progress-bar">
                  <div class="progress-value" :style="{ width: '78%' }"></div>
                </div>
              </div>
              <p class="card-description">{{ selectedContainer.container?.activate ? 'Contentor ativo' : 'Contentor inativo' }}</p>
            </div>
          </div>
        </div>

        <div class="section-container">
          <div class="section-header">
            <h2 class="section-title">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-map-pin"><path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/><circle cx="12" cy="10" r="3"/></svg>
              Última Secção
            </h2>
            <p class="section-subtitle">Localização atual</p>
          </div>

          <div v-if="lastHistory" class="section-card">
            <div class="section-info">
              <h3 class="section-value">Secção {{ lastHistory.sectionId }}</h3>
              <div class="section-details">
                <div class="detail-row">
                  <span class="detail-label">Data:</span>
                  <span class="detail-value">{{ formatDate(lastHistory.datetime) }}</span>
                </div>
                <div class="detail-row">
                  <span class="detail-label">ID:</span>
                  <span class="detail-value">{{ lastHistory.id }}</span>
                </div>
              </div>
            </div>
          </div>
          <div v-else class="empty-section">
            <div class="empty-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-map-pin"><path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/><circle cx="12" cy="10" r="3"/></svg>
            </div>
            <p>Nenhuma secção registrada</p>
          </div>
        </div>

        <!-- Current content -->
        <div class="content-container">
          <div class="content-header">
            <div>
              <h2 class="section-title">
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-package"><path d="M16.5 9.4 7.55 4.24"/><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/><polyline points="3.29 7 12 12 20.71 7"/><line x1="12" x2="12" y1="22" y2="12"/></svg>
                Conteúdo Atual
              </h2>
              <p class="section-subtitle">Materiais armazenados</p>
            </div>
            <div class="items-badge">{{ itemLocalizationData.length }} itens</div>
          </div>

          <div v-if="itemLocalizationData.length > 0" class="materials-list">
            <div v-for="(item, index) in itemLocalizationData" :key="item.code + index" class="material-item">
              <div v-if="index > 0" class="separator"></div>
              <div class="material-header">
                <h3 class="material-title">
                  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-cog"><path d="M12 20a8 8 0 1 0 0-16 8 8 0 0 0 0 16Z"/><path d="M12 14a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z"/><path d="M12 2v2"/><path d="M12 22v-2"/><path d="m17 20.66-1-1.73"/><path d="M11 10.27 7 3.34"/><path d="m20.66 17-1.73-1"/><path d="m3.34 7 1.73 1"/><path d="M14 12h8"/><path d="M2 12h2"/><path d="m20.66 7-1.73 1"/><path d="m3.34 17 1.73-1"/><path d="m17 3.34-1 1.73"/><path d="m7 20.66 1-1.73"/></svg>
                  {{ item.rawMaterial }}
                </h3>
                <div class="material-code">{{ item.code }}</div>
              </div>
              <div class="material-details">
                <div class="material-detail">
                  <span class="detail-label">Quantidade</span>
                  <span class="detail-value">{{ item.quantity }} {{ item.unit }}</span>
                </div>
                <div class="material-detail">
                  <span class="detail-label">Lote</span>
                  <span class="detail-value">{{ item.lotInfo }}</span>
                </div>
                <div class="material-detail">
                  <span class="detail-label">ID do Lote</span>
                  <span class="detail-value">{{ item.lotId }}</span>
                </div>
              </div>
            </div>
          </div>
          <div v-else class="empty-content">
            <div class="empty-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-package"><path d="M16.5 9.4 7.55 4.24"/><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/><polyline points="3.29 7 12 12 20.71 7"/><line x1="12" x2="12" y1="22" y2="12"/></svg>
            </div>
            <p>Este contentor não contém nenhum item na sua última localização.</p>
          </div>

          <div class="content-footer">
            <span class="footer-label">Total de materiais:</span>
            <span class="footer-value">{{ getTotalQuantity() }} {{ getUnitType() }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { computed, onMounted, ref, watch } from "vue";
import { useRoute } from "vue-router";
import { ContainerDetails } from "@/types/ContainerDetails";

const containers = ref(new Map<number, ContainerDetails>());
const selectedContainerId = ref<number | null>(null);
const itemLocalizationData = ref<any[]>([]);
const loading = ref(false);
const error = ref('');
const route = useRoute();

async function fetchContainerDetails(containerId: number) {
  loading.value = true;
  error.value = '';
  try {
    const response = await fetch("http://localhost:5181/api/ContainerLocalizationHistory");
    const allHistories = await response.json();

    const containerHistories = allHistories
        .filter((h: any) => h.containerId === containerId)
        .sort((a: any, b: any) => new Date(a.datetime).getTime() - new Date(b.datetime).getTime());

    if (containerHistories.length > 0) {
      containers.value.set(containerId, {
        container: {
          containerId: containerId,
          containerName: { name: `Container ${containerId}` },
          containerVolume: 100,
          activate: true,
          localizationHistories: containerHistories
        }
      });
    }
  } catch (err) {
    console.error("Erro ao buscar detalhes do container:", err);
    error.value = "Erro ao buscar detalhes do container.";
  } finally {
    loading.value = false;
  }
}

async function fetchItemLocalizationsForContainerLocalization(containerLocalizationId: number) {
  try {
    const response = await fetch("http://localhost:5181/api/ItemLocalization");
    const all = await response.json();

    const filtered = all.filter((item: any) => item.containerLocalizationId === containerLocalizationId);
    const result = [];

    for (const item of filtered) {
      const itemRawId = item.itemRawId;
      if (!itemRawId) continue;

      try {
        const itemRes = await fetch(`http://localhost:5181/api/ItemOfRawMaterial/${itemRawId}`);
        const itemDetails = await itemRes.json();
        console.log("Item details:", itemDetails);

        const lotId = itemDetails.lotOfRawMaterialId;
        console.log("lotId:", lotId);
        let lotDetails = null;
        let rawMaterialDetails = null;

        if (lotId) {
          try {
            const lotRes = await fetch(`http://localhost:5181/api/LotOfRawMaterial/${lotId}`);
            lotDetails = await lotRes.json();

            console.log("Lot details:", lotDetails);

            const rawMaterialId = lotDetails?.rawMaterialId;

            if (rawMaterialId) {
              try {
                const rawMatRes = await fetch(`http://localhost:5181/api/RawMaterial/${rawMaterialId}`);
                rawMaterialDetails = await rawMatRes.json();
                console.log("Raw material details:", rawMaterialDetails);
              } catch (rawErr) {
                console.warn(`Erro ao buscar rawMaterial com id ${rawMaterialId}:`, rawErr);
              }
            }
          } catch (lotErr) {
            console.warn(`Erro ao buscar lote com id ${lotId}:`, lotErr);
          }
        }

        result.push({
          code: itemDetails.code?.name || 'N/A',
          quantity: itemDetails.quantity ?? 0,
          unit: itemDetails.unit || 'N/A',
          rawMaterial: rawMaterialDetails?.name || 'N/A',
          lotInfo: `${lotDetails?.lotQuantity ?? 0} ${lotDetails?.lotUnit || ''}`,
          lotId: lotId ?? 'N/A'
        });
      } catch (err) {
        console.warn(`Erro ao buscar dados de itemRawId ${itemRawId}:`, err);
      }
    }

    return result;
  } catch (error) {
    console.error("Erro ao buscar ItemLocalization:", error);
    return [];
  }
}

const selectedContainer = computed(() => {
  return selectedContainerId.value ? containers.value.get(selectedContainerId.value) ?? null : null;
});

const lastHistory = computed(() => {
  const localizationHistories = selectedContainer.value?.container?.localizationHistories;
  return localizationHistories ? localizationHistories[localizationHistories.length - 1] : null;
});

// Format date function
function formatDate(dateString: string) {
  if (!dateString) return "N/A";

  const date = new Date(dateString);
  return new Intl.DateTimeFormat("pt-PT", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(date);
}

function getTotalQuantity() {
  return itemLocalizationData.value.reduce((sum, item) => sum + item.quantity, 0);
}

function getUnitType() {
  return itemLocalizationData.value.length > 0 ? itemLocalizationData.value[0].unit : 'unidades';
}

watch(lastHistory, async (newHistory) => {
  if (newHistory?.id) {
    itemLocalizationData.value = await fetchItemLocalizationsForContainerLocalization(newHistory.id);
  }
});

onMounted(async () => {
  const containerId = Number(route.params.id);
  if (containerId) {
    selectedContainerId.value = containerId;
    await fetchContainerDetails(containerId);
  }
});
</script>

<style scoped>
/* Base styles */
.dashboard-container {
  display: flex;
  justify-content: center;
  padding: 20px;
  font-family: system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  color: #333;
  background: linear-gradient(to bottom, #ffffff, #f5f7fa);
  min-height: 100vh;
}

.content {
  flex-grow: 1;
  padding: 20px;
  max-width: 1200px;
  width: 100%;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
}

.loading-spinner {
  position: relative;
  width: 64px;
  height: 64px;
}

.spinner-ring {
  position: absolute;
  width: 100%;
  height: 100%;
  border-radius: 50%;
  border: 4px solid #f3f4f6;
  animation: pulse 1.5s ease-in-out infinite;
}

.spinner-core {
  position: absolute;
  top: 50%;
  left: 50%;
  width: 32px;
  height: 32px;
  margin-top: -16px;
  margin-left: -16px;
  border-radius: 50%;
  border: 4px solid transparent;
  border-top-color: #506D7C;
  animation: spin 1s linear infinite;
}

.loading-text {
  margin-top: 16px;
  color: #6b7280;
  font-size: 14px;
}

@keyframes pulse {
  0% {
    transform: scale(0.95);
    opacity: 0.7;
  }
  50% {
    transform: scale(1);
    opacity: 1;
  }
  100% {
    transform: scale(0.95);
    opacity: 0.7;
  }
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}

/* Error state */
.error-container {
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #fee2e2;
  color: #b91c1c;
  padding: 16px;
  border-radius: 8px;
  margin: 32px auto;
  max-width: 500px;
}

.error-icon {
  margin-right: 12px;
}

/* Container details */
.container-details {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
  overflow: hidden;
}

/* Header */
.container-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  padding: 24px 32px;
  border-bottom: 1px solid #e5e7eb;
}

.container-title {
  font-size: 28px;
  font-weight: 700;
  margin: 0;
  background: linear-gradient(to right, #506D7C, #57768a);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.container-subtitle {
  color: #6b7280;
  margin: 4px 0 0 0;
  font-size: 14px;
}

.container-badge {
  padding: 6px 12px;
  border-radius: 9999px;
  font-size: 14px;
  font-weight: 500;
  background-color: #e5e7eb;
  color: #4b5563;
}

.container-badge.active {
  background-color: #506D7C;
  color: white;
}

/* Info cards */
.info-cards {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 24px;
  padding: 32px;
}

.info-card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
  overflow: hidden;
  border-top: 4px solid #506D7C;
}

.card-header {
  display: flex;
  align-items: center;
  padding: 12px 16px;
  background-color: #f9fafb;
}

.card-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 8px;
  color: #506D7C;
}

.card-title {
  font-size: 14px;
  font-weight: 500;
  margin: 0;
  color: #4b5563;
}

.card-content {
  padding: 16px;
}

.card-value {
  font-size: 24px;
  font-weight: 600;
  margin: 0;
  color: #111827;
}

.card-value-with-unit {
  display: flex;
  align-items: baseline;
}

.card-unit {
  font-size: 14px;
  margin-left: 4px;
  color: #6b7280;
}

.card-description {
  font-size: 12px;
  color: #6b7280;
  margin: 4px 0 0 0;
}

/* Utilization */
.utilization {
  margin-bottom: 8px;
}

.utilization-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
}

.utilization-text {
  font-size: 14px;
  font-weight: 500;
}

.utilization-total {
  font-size: 12px;
  color: #6b7280;
}

.progress-bar {
  height: 8px;
  background-color: #e5e7eb;
  border-radius: 4px;
  overflow: hidden;
}

.progress-value {
  height: 100%;
  background-color: #506D7C;
  border-radius: 4px;
  transition: width 0.3s ease;
}

/* Section container */
.section-container, .content-container {
  padding: 0 32px 32px;
}

.section-header, .content-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 16px;
}

.section-title {
  font-size: 20px;
  font-weight: 600;
  margin: 0;
  display: flex;
  align-items: center;
  color: #111827;
}

.section-title svg {
  margin-right: 8px;
  color: #506D7C;
}

.section-subtitle {
  font-size: 14px;
  color: #6b7280;
  margin: 4px 0 0 0;
}

.items-badge {
  background-color: #f3f4f6;
  color: #4b5563;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
}

/* Section card */
.section-card {
  background-color: #f9fafb;
  border-radius: 8px;
  padding: 16px;
}

.section-info {
  display: flex;
  flex-direction: column;
}

.section-value {
  font-size: 18px;
  font-weight: 600;
  margin: 0 0 12px 0;
  color: #111827;
}

.section-details {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.detail-row {
  display: flex;
  font-size: 14px;
}

.detail-label {
  width: 80px;
  color: #6b7280;
}

.detail-value {
  font-weight: 500;
  color: #4b5563;
}

/* Empty states */
.empty-section, .empty-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background-color: #f9fafb;
  border-radius: 8px;
  padding: 32px;
  text-align: center;
  color: #6b7280;
}

.empty-icon {
  color: #d1d5db;
  margin-bottom: 12px;
}

/* Materials list */
.materials-list {
  background-color: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
}

.material-item {
  padding: 16px;
  position: relative;
  transition: background-color 0.2s;
}

.material-item:hover {
  background-color: #f9fafb;
}

.separator {
  position: absolute;
  top: 0;
  left: 16px;
  right: 16px;
  height: 1px;
  background-color: #e5e7eb;
}

.material-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.material-title {
  font-size: 16px;
  font-weight: 600;
  margin: 0;
  display: flex;
  align-items: center;
  color: #111827;
}

.material-title svg {
  margin-right: 8px;
  color: #1e1f22;
}

.material-code {
  background-color: #e5e7eb;
  color: #4b5563;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
}

.material-details {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.material-detail {
  display: flex;
  flex-direction: column;
}

/* Footer */
.content-footer {
  display: flex;
  justify-content: space-between;
  padding: 12px 16px;
  background-color: #f9fafb;
  border-radius: 0 0 8px 8px;
  margin-top: 16px;
  font-size: 14px;
}

.footer-label {
  color: #6b7280;
}

.footer-value {
  font-weight: 500;
  color: #4b5563;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .info-cards {
    grid-template-columns: 1fr;
  }

  .container-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .container-badge {
    margin-top: 12px;
  }

  .material-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .material-code {
    margin-top: 8px;
  }

  .material-details {
    grid-template-columns: 1fr;
  }
}
</style>