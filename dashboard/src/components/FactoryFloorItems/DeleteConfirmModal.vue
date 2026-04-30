<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4">
    <div class="bg-white rounded-lg p-6 max-w-md w-full">
      <h2 class="text-xl font-bold mb-4">Confirm Deletion</h2>
      <p class="mb-6">Are you sure you want to delete this container?</p>

      <div class="flex justify-end gap-2">
        <Button class="px-4 py-2 text-gray-600 hover:text-gray-800" @click="close">
          Cancel
        </Button>
        <Button class="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700" @click="confirmDelete">
          Delete
        </Button>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
defineProps<{
  isOpen: boolean;
}>();

const emit = defineEmits(['close', 'confirm']);

const close = () => emit('close');
const confirmDelete = () => emit('confirm');
</script>