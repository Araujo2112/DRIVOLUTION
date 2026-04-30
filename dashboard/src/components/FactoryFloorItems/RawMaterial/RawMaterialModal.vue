<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black/50 flex items-center justify-center">
    <div class="bg-white p-6 rounded-lg w-full max-w-md">
      <h2 class="text-xl font-bold mb-4">
        {{ rawMaterial ? "Edit Raw Material" : "New Raw Material" }}
      </h2>

      <form @submit.prevent="handleSubmit">
        <div class="space-y-4">
          <div v-if="rawMaterial">
            <label class="block mb-1">rawId</label>
            <input
                v-model="localItem.rawId"
                class="w-full p-2 border rounded bg-gray-200 cursor-not-allowed"
                disabled
                type="text"
            />
          </div>

          <div>
            <label class="block mb-1">Name</label>
            <input
                v-model="localItem.name"
                class="w-full p-2 border rounded"
                required
                type="text"
            />
          </div>

          <div>
            <label class="block mb-1">Information</label>
            <input
                v-model="localItem.info"
                class="w-full p-2 border rounded"
                required
                type="text"
            />
          </div>
        </div>

        <div class="flex justify-end gap-2 mt-6">
          <Button type="button" @click="$emit('close')">Cancel</Button>
          <Button type="submit">Save</Button>
        </div>
      </form>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {ref, watch} from "vue";
import Button from "@/components/Button.vue"

const props = defineProps({
  isOpen: Boolean,
  rawMaterial: Object
});

const emit = defineEmits(["close", "submit"]);

const localItem = ref({
  rawId: 0,
  name: "",
  info: ""
});

watch(
    () => props.rawMaterial,
    (newVal) => {
      if (newVal) {
        localItem.value = {...newVal};
      } else {
        resetForm();
      }
    },
    {deep: true}
);

function handleSubmit() {
  emit("submit", {
    id: localItem.value.rawId,
    name: localItem.value.name,
    info: localItem.value.info
  });
  resetForm();
}

function resetForm() {
  localItem.value = {
    rawId: 0,
    name: "",
    info: ""
  };
}
</script>
