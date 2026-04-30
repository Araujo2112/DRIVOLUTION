<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4">
    <div class="bg-white rounded-lg p-6 w-full max-w-2xl">
      <h2 class="text-xl font-bold mb-4">{{ editing ? 'Editar Associação' : 'Nova Associação' }}</h2>

      <form @submit.prevent="handleSubmit">
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium mb-1">Contentor</label>
            <select v-model="form.containerCode" @change="updateAvailableVolume" required
                    class="w-full px-3 py-2 border rounded-md">
              <option v-for="container in containers" :key="container.containerCode"
                      :value="container.containerCode">
                {{ container.containerName }} ({{ container.containerVolume }}m³)
              </option>
            </select>
            <div class="mt-2 text-sm">
              Volume disponível: {{ availableVolume.toFixed(2) }}m³
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium mb-1">Materiais</label>
            <div v-for="(material, index) in form.materials" :key="index" class="mb-2">
              <div class="flex gap-2">
                <select v-model="material.materialId" @change="updateMaterialInfo(index)" required
                        class="flex-1 px-3 py-2 border rounded-md">
                  <option v-for="m in materials" :key="m.materialId" :value="m.materialId">
                    {{ m.materialName }} ({{ m.unitVolume }}m³/un)
                  </option>
                </select>
                <input v-model.number="material.quantity" type="number" min="1" required
                       class="w-24 px-3 py-2 border rounded-md">
                <button type="button" @click="removeMaterial(index)"
                        class="px-2 text-red-600 hover:text-red-800">
                  ×
                </button>
              </div>
              <div class="text-xs mt-1">
                Total: {{ (material.quantity * (materials.find(m => m.materialId === material.materialId)?.unitVolume || 0)).toFixed(2) }}m³
              </div>
            </div>
            <button type="button" @click="addMaterial"
                    class="mt-2 text-sm text-blue-600 hover:text-blue-800">
              + Adicionar Material
            </button>
          </div>
        </div>

        <div class="mt-6 flex justify-end gap-2">
          <button type="button" @click="close"
                  class="px-4 py-2 text-gray-600 hover:text-gray-800">
            Cancelar
          </button>
          <button type="submit" :disabled="totalVolume > availableVolume"
                  class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50">
            Salvar
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';

const props = defineProps<{
  isOpen: boolean;
  item?: any;
  containers: any[];
  materials: any[];
  usedVolume: (containerCode: string) => number;
}>();

const emit = defineEmits(['close', 'submit']);

const editing = ref(false);
const form = ref({
  containerCode: '',
  materials: [] as Array<{ materialId: string; quantity: number }>
});

const availableVolume = computed(() => {
  if (!form.value.containerCode) return 0;
  const container = props.containers.find(c => c.containerCode === form.value.containerCode);
  return container ? container.containerVolume - props.usedVolume(form.value.containerCode) : 0;
});

const totalVolume = computed(() => {
  return form.value.materials.reduce((acc, material) => {
    const mat = props.materials.find(m => m.materialId === material.materialId);
    return acc + (material.quantity * (mat?.unitVolume || 0));
  }, 0);
});

watch(() => props.item, (newVal) => {
  if (newVal) {
    editing.value = true;
    form.value = {
      containerCode: newVal.containerCode,
      materials: [{
        materialId: newVal.materialId,
        quantity: newVal.quantity
      }]
    };
  } else {
    resetForm();
  }
});

function updateAvailableVolume() {
  form.value.materials = [];
}

function updateMaterialInfo(index: number) {
}

function addMaterial() {
  form.value.materials.push({ materialId: '', quantity: 1 });
}

function removeMaterial(index: number) {
  form.value.materials.splice(index, 1);
}

function handleSubmit() {
  emit('submit', form.value);
  close();
}

function close() {
  emit('close');
  resetForm();
}

function resetForm() {
  form.value = {
    containerCode: '',
    materials: []
  };
  editing.value = false;
}
</script>