

<script setup lang="ts">
import { ref, watch } from 'vue';
import type { Container } from '@/views/ContainersView.vue';

const props = defineProps<{
  isOpen: boolean;
  container?: Container | null;
}>();

const emit = defineEmits(['close', 'submit']);

const editing = ref(false);
const form = ref({
  containerCode: '',
  containerName: '',
  containerVolume: 0,
  containerDescription: ''
});

watch(() => props.container, (newVal) => {
  if (newVal) {
    editing.value = true;
    form.value = { ...newVal };
  } else {
    resetForm();
  }
});

const resetForm = () => {
  form.value = {
    containerCode: '',
    containerName: '',
    containerVolume: 0,
    containerDescription: ''
  };
  editing.value = false;
};

const handleSubmit = () => {
  emit('submit', form.value);
  close();
};

const close = () => {
  emit('close');
  resetForm();
};
</script>


<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4">
    <div class="bg-white rounded-lg p-6 w-full max-w-md">
      <h2 class="text-xl font-bold mb-4">{{ editing ? 'Editar Contentor' : 'Novo Contentor' }}</h2>

      <form @submit.prevent="handleSubmit">
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium mb-1">Código</label>
            <input v-model="form.containerCode" type="text" required
                   class="w-full px-3 py-2 border rounded-md">
          </div>

          <div>
            <label class="block text-sm font-medium mb-1">Nome</label>
            <input v-model="form.containerName" type="text" required
                   class="w-full px-3 py-2 border rounded-md">
          </div>

          <div>
            <label class="block text-sm font-medium mb-1">Volume</label>
            <input v-model="form.containerVolume" type="number" required
                   class="w-full px-3 py-2 border rounded-md">
          </div>

          <div>
            <label class="block text-sm font-medium mb-1">Descrição</label>
            <textarea v-model="form.containerDescription"
                      class="w-full px-3 py-2 border rounded-md"></textarea>
          </div>
        </div>

        <div class="mt-6 flex justify-end gap-2">
          <button type="button" @click="close"
                  class="px-4 py-2 text-gray-600 hover:text-gray-800">
            Cancelar
          </button>
          <button type="submit"
                  class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">
            Salvar
          </button>
        </div>
      </form>
    </div>
  </div>
</template>