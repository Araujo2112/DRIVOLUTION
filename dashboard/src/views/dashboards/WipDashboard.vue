<script setup lang="ts">
import { onMounted, ref } from "vue";
import axios from "@/axios";

const loading = ref(false);
const errorMessage = ref("");

const summary = ref({
  totalProducts: 0,
  inProgress: 0,
  completed: 0,
});

const items = ref<any[]>([]);

async function loadWip() {
  try {
    loading.value = true;
    errorMessage.value = "";

    const response = await axios.get("/production-lines/wip");

    summary.value = {
      totalProducts: response.data.totalProducts ?? 0,
      inProgress: response.data.inProgress ?? 0,
      completed: response.data.completed ?? 0,
    };

    items.value = response.data.items?.$values ?? response.data.items ?? [];
  } catch (error: any) {
    console.error(error);
    errorMessage.value =
      error?.response?.data || "Erro ao carregar dashboard WIP.";
  } finally {
    loading.value = false;
  }
}

function formatDate(date: string | null) {
  if (!date) return "-";
  return new Date(date).toLocaleString("pt-PT");
}

function formatDuration(seconds: number | null) {
  if (seconds === null || seconds === undefined) return "-";

  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const remainingSeconds = seconds % 60;

  if (hours > 0) return `${hours}h ${minutes}m`;
  if (minutes > 0) return `${minutes}m ${remainingSeconds}s`;

  return `${remainingSeconds}s`;
}

function statusLabel(status: string | null) {
  if (status === "in_progress") return "Em produção";
  if (status === "completed") return "Concluído";
  return status || "-";
}
</script>

<template>
  <div class="p-6">
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold text-gray-800">
          Dashboard WIP
        </h1>
        <p class="text-sm text-gray-500 mt-1">
          Produtos atualmente em curso na linha de produção.
        </p>
      </div>

      <button
        class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
        @click="loadWip"
      >
        Atualizar
      </button>
    </div>

    <div v-if="loading" class="text-gray-500">
      A carregar dashboard...
    </div>

    <div v-else-if="errorMessage" class="bg-red-100 text-red-700 p-4 rounded-lg">
      {{ errorMessage }}
    </div>

    <div v-else>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div class="bg-white rounded-xl shadow p-5">
          <p class="text-sm text-gray-500">Total de Produtos</p>
          <p class="text-3xl font-bold text-gray-800 mt-2">
            {{ summary.totalProducts }}
          </p>
        </div>

        <div class="bg-white rounded-xl shadow p-5">
          <p class="text-sm text-gray-500">Em Produção</p>
          <p class="text-3xl font-bold text-blue-600 mt-2">
            {{ summary.inProgress }}
          </p>
        </div>

        <div class="bg-white rounded-xl shadow p-5">
          <p class="text-sm text-gray-500">Concluídos</p>
          <p class="text-3xl font-bold text-green-600 mt-2">
            {{ summary.completed }}
          </p>
        </div>
      </div>

      <div class="bg-white rounded-xl shadow overflow-hidden">
        <table class="w-full text-sm">
          <thead class="bg-slate-700 text-white">
            <tr>
              <th class="px-4 py-3 text-left">Produto</th>
              <th class="px-4 py-3 text-left">Linha</th>
              <th class="px-4 py-3 text-left">Workstation</th>
              <th class="px-4 py-3 text-left">Fase Atual</th>
              <th class="px-4 py-3 text-left">Estado</th>
              <th class="px-4 py-3 text-left">Início</th>
              <th class="px-4 py-3 text-left">Tempo Atual</th>
            </tr>
          </thead>

          <tbody>
            <tr
              v-for="item in items"
              :key="item.productId + '-' + item.currentPhase"
              class="border-b hover:bg-gray-50"
            >
              <td class="px-4 py-3 font-semibold">
                {{ item.serialNumber }}
              </td>

              <td class="px-4 py-3">
                {{ item.productionLineName }}
              </td>

              <td class="px-4 py-3">
                {{ item.workstation }}
              </td>

              <td class="px-4 py-3">
                {{ item.currentPhase }}
              </td>

              <td class="px-4 py-3">
                <span
                  class="px-2 py-1 rounded-full text-xs font-medium"
                  :class="item.wipStatus === 'completed'
                    ? 'bg-green-100 text-green-700'
                    : 'bg-yellow-100 text-yellow-700'"
                >
                  {{ statusLabel(item.wipStatus) }}
                </span>
              </td>

              <td class="px-4 py-3">
                {{ formatDate(item.startedAt) }}
              </td>

              <td class="px-4 py-3">
                {{ formatDuration(item.elapsedSeconds) }}
              </td>
            </tr>
          </tbody>
        </table>

        <div v-if="items.length === 0" class="p-4 text-gray-500">
          Não existem produtos em produção.
        </div>
      </div>
    </div>
  </div>
</template>