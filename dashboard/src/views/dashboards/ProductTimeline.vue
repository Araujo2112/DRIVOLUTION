<script setup lang="ts">
import { onMounted, ref } from "vue";
import axios from "@/axios";

const loading = ref(false);
const errorMessage = ref("");

const productId = ref(2);
const timeline = ref<any[]>([]);
const product = ref<any>(null);

async function loadTimeline() {
    try {
        loading.value = true;
        errorMessage.value = "";
        timeline.value = [];
        product.value = null;

        const response = await axios.get(`/products/${productId.value}/timeline`);

        product.value = {
            id: response.data.productId,
            serialNumber: response.data.serialNumber,
            status: response.data.status
        };

        timeline.value = response.data.phases?.$values ?? response.data.phases ?? [];
    } catch (error: any) {
        console.error(error);
        errorMessage.value =
            error?.response?.data || "Erro ao carregar timeline do produto.";
    } finally {
        loading.value = false;
    }
}

function formatDate(date: string | null) {
    if (!date) return "-";
    return new Date(date).toLocaleString("pt-PT");
}

function formatDuration(seconds: number | null) {
    if (seconds === null || seconds === undefined) return "Em curso";

    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;

    return `${minutes}m ${remainingSeconds}s`;
}

onMounted(() => {
    loadTimeline();
});
</script>

<template>
    <div class="p-6">
        <div class="flex items-center justify-between mb-6">
            <div>
                <h1 class="text-2xl font-bold text-gray-800">
                    Timeline de Produção
                </h1>

                <p v-if="product" class="text-sm text-gray-600 mt-1">
                    Produto:
                    <strong>{{ product.serialNumber }}</strong>
                    <span class="ml-2 px-2 py-1 rounded-full text-xs bg-blue-100 text-blue-700">
                        {{ product.status }}
                    </span>
                </p>
            </div>

            <div class="flex items-center gap-2">
                <input
                    v-model.number="productId"
                    type="number"
                    min="1"
                    class="border rounded-lg px-3 py-2 w-28"
                    placeholder="ID"
                />

                <button
                    class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
                    @click="loadTimeline"
                >
                    Atualizar
                </button>
            </div>
        </div>

        <div v-if="loading" class="text-gray-500">
            A carregar timeline...
        </div>

        <div v-else-if="errorMessage" class="bg-red-100 text-red-700 p-4 rounded-lg">
            {{ errorMessage }}
        </div>

        <div v-else class="bg-white rounded-xl shadow overflow-hidden">
            <table class="w-full text-sm">
                <thead class="bg-slate-700 text-white">
                    <tr>
                        <th class="px-4 py-3 text-left">Fase</th>
                        <th class="px-4 py-3 text-left">Workstation</th>
                        <th class="px-4 py-3 text-left">Início</th>
                        <th class="px-4 py-3 text-left">Fim</th>
                        <th class="px-4 py-3 text-left">Duração</th>
                        <th class="px-4 py-3 text-left">Resultado</th>
                        <th class="px-4 py-3 text-left">Notas</th>
                    </tr>
                </thead>

                <tbody>
                    <tr
                        v-for="phase in timeline"
                        :key="phase.productPhaseId"
                        class="border-b hover:bg-gray-50"
                    >
                        <td class="px-4 py-3 font-semibold">
                            {{ phase.phaseName }}
                        </td>

                        <td class="px-4 py-3">
                            {{ phase.workstation }}
                        </td>

                        <td class="px-4 py-3">
                            {{ formatDate(phase.startedAt) }}
                        </td>

                        <td class="px-4 py-3">
                            {{ formatDate(phase.endedAt) }}
                        </td>

                        <td class="px-4 py-3">
                            {{ formatDuration(phase.durationSeconds) }}
                        </td>

                        <td class="px-4 py-3">
                            <span
                                class="px-2 py-1 rounded-full text-xs font-medium"
                                :class="phase.result === 'completed'
                                    ? 'bg-green-100 text-green-700'
                                    : 'bg-yellow-100 text-yellow-700'"
                            >
                                {{ phase.result }}
                            </span>
                        </td>

                        <td class="px-4 py-3">
                            {{ phase.notes || "-" }}
                        </td>
                    </tr>
                </tbody>
            </table>

            <div v-if="timeline.length === 0" class="p-4 text-gray-500">
                Este produto ainda não tem fases registadas.
            </div>
        </div>
    </div>
</template>