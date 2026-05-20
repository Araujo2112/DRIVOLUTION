<script setup lang="ts">
import { onMounted, ref } from "vue";
import axios from "@/axios";

const loading = ref(false);

const summary = ref({
    totalProducts: 0,
    inProgress: 0,
    completed: 0
});

const items = ref<any[]>([]);

async function loadWip() {
    try {
        loading.value = true;

        const response = await axios.get("/production-lines/wip");

        summary.value = {
            totalProducts: response.data.totalProducts,
            inProgress: response.data.inProgress,
            completed: response.data.completed
        };

        items.value = response.data.items.$values ?? response.data.items;
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

function formatDuration(seconds: number) {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;

    return `${minutes}m ${remainingSeconds}s`;
}

onMounted(() => {
    loadWip();
});
</script>

<template>
    <div class="p-4">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h3>Dashboard WIP (Work-In-Progress)</h3>

            <button
                class="btn btn-primary"
                @click="loadWip"
            >
                Atualizar
            </button>
        </div>

        <div class="row mb-4">
            <div class="col">
                <div class="card shadow-sm">
                    <div class="card-body">
                        <h6>Total Produtos</h6>
                        <h3>{{ summary.totalProducts }}</h3>
                    </div>
                </div>
            </div>

            <div class="col">
                <div class="card shadow-sm">
                    <div class="card-body">
                        <h6>Em Produção</h6>
                        <h3 class="text-warning">
                            {{ summary.inProgress }}
                        </h3>
                    </div>
                </div>
            </div>

            <div class="col">
                <div class="card shadow-sm">
                    <div class="card-body">
                        <h6>Concluídos</h6>
                        <h3 class="text-success">
                            {{ summary.completed }}
                        </h3>
                    </div>
                </div>
            </div>
        </div>

        <div
            v-if="loading"
            class="alert alert-info"
        >
            A carregar WIP...
        </div>

        <div
            v-else
            class="card shadow-sm"
        >
            <div class="card-body">
                <table class="table table-hover align-middle">
                    <thead>
                    <tr>
                        <th>Produto</th>
                        <th>Linha</th>
                        <th>Workstation</th>
                        <th>Fase Atual</th>
                        <th>Estado</th>
                        <th>Início</th>
                        <th>Tempo Atual</th>
                    </tr>
                    </thead>

                    <tbody>
                    <tr
                        v-for="item in items"
                        :key="item.productId + item.currentPhase"
                    >
                        <td>
                            <strong>{{ item.serialNumber }}</strong>
                        </td>

                        <td>
                            {{ item.productionLineName }}
                        </td>

                        <td>
                            {{ item.workstation }}
                        </td>

                        <td>
                            {{ item.currentPhase }}
                        </td>

                        <td>
                <span
                    class="badge"
                    :class="item.wipStatus === 'in_progress'
                        ? 'bg-warning text-dark'
                        : 'bg-success'"
                >
                  {{ item.wipStatus }}
                </span>
                        </td>

                        <td>
                            {{ formatDate(item.startedAt) }}
                        </td>

                        <td>
                            {{ formatDuration(item.elapsedSeconds) }}
                        </td>
                    </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>