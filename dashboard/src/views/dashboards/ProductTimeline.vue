<script setup lang="ts">
import { onMounted, ref } from "vue";
import axios from "@/axios";

const loading = ref(false);

const timeline = ref<any[]>([]);
const product = ref<any>(null);

async function loadTimeline() {
    try {
        loading.value = true;

        const response = await axios.get("/products/1/timeline");

        product.value = {
            serialNumber: response.data.serialNumber
        };

        timeline.value = response.data.phases.$values ?? response.data.phases;
    }
    catch (error) {
        console.error(error);
    }
    finally {
        loading.value = false;
    }
}

function formatDate(date: string | null) {
    if (!date) return "-";

    return new Date(date).toLocaleString("pt-PT");
}

function formatDuration(seconds: number | null) {
    if (!seconds) return "Em curso";

    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;

    return `${minutes}m ${remainingSeconds}s`;
}

onMounted(() => {
    loadTimeline();
});
</script>

<template>
    <div class="p-4">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <div>
                <h3>Timeline de Produção</h3>

                <div v-if="product">
                    <strong>Produto:</strong> {{ product.serialNumber }}
                </div>
            </div>

            <button
                class="btn btn-primary"
                @click="loadTimeline"
            >
                Atualizar
            </button>
        </div>

        <div v-if="loading">
            A carregar timeline...
        </div>

        <div
            v-else
            class="card"
        >
            <div class="card-body">
                <table class="table table-hover align-middle">
                    <thead>
                    <tr>
                        <th>Fase</th>
                        <th>Workstation</th>
                        <th>Início</th>
                        <th>Fim</th>
                        <th>Duração</th>
                        <th>Resultado</th>
                        <th>Notas</th>
                    </tr>
                    </thead>

                    <tbody>
                    <tr
                        v-for="phase in timeline"
                        :key="phase.productPhaseId"
                    >
                        <td>
                            <strong>{{ phase.phaseName }}</strong>
                        </td>

                        <td>
                            {{ phase.workstation }}
                        </td>

                        <td>
                            {{ formatDate(phase.startedAt) }}
                        </td>

                        <td>
                            {{ formatDate(phase.endedAt) }}
                        </td>

                        <td>
                            {{ formatDuration(phase.durationSeconds) }}
                        </td>

                        <td>
                <span
                    class="badge"
                    :class="phase.result === 'completed'
                        ? 'bg-success'
                        : 'bg-warning text-dark'"
                >
                  {{ phase.result }}
                </span>
                        </td>

                        <td>
                            {{ phase.notes || "-" }}
                        </td>
                    </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>