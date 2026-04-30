<script lang="ts" setup>
import { defineEmits, defineProps, ref, watch } from 'vue';
import type { LotRawMaterial, RawMaterial } from "@/types/Interfaces.ts";
import Button from "@/components/Button.vue"

const emit = defineEmits(['submit', 'close']);

const props = defineProps({
  isOpen: {
    type: Boolean,
    required: true
  },
  lotRawMaterial: {
    type: Object as () => LotRawMaterial | null,
    required: false
  },
  rawMaterials: {
    type: Array as () => RawMaterial[],
    required: true
  }
});

const localItem = ref<Partial<LotRawMaterial>>({
  lotCode: "",
  lotNumber: "",
  lotQuantity: 0,
  lotUnit: "Kilograms",
  rawMaterialId: undefined
});

watch(() => props.lotRawMaterial, (newVal) => {
  if (newVal) {
    localItem.value = { ...newVal };
  } else {
    localItem.value = {
      lotCode: "",
      lotNumber: "",
      lotQuantity: 0,
      lotUnit: "Kilograms",
      rawMaterialId: undefined
    };
  }
});

function handleSubmit() {

  localItem.value.lotCode = String(localItem.value.lotCode);
  localItem.value.lotNumber = String(localItem.value.lotNumber);

  if (!localItem.value.lotCode || !localItem.value.lotNumber) {
    alert("Please fill in all required fields!");
    return;
  }

  emit('submit', localItem.value);
  emit('close');
}
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black/50 flex items-center justify-center">
    <div class="bg-white p-6 rounded-lg w-full max-w-md">
      <h2 class="text-xl font-bold mb-4">
        {{ lotRawMaterial ? "Edit Lot" : "New Lot" }}
      </h2>
      <form @submit.prevent="handleSubmit">
        <div class="space-y-4">
          <div>
            <label class="block mb-1">Lot Code <span class="text-red-500">*</span></label>
            <input
                v-model="localItem.lotCode"
                class="w-full p-2 border rounded"
                :disabled="!!lotRawMaterial"
                type="text"
            />
          </div>

          <div>
            <label class="block mb-1">Lot Number <span class="text-red-500">*</span></label>
            <input
                v-model="localItem.lotNumber"
                class="w-full p-2 border rounded"
                :disabled="!!lotRawMaterial"
                type="text"
            />
          </div>

          <div>
            <label class="block mb-1">Quantity <span class="text-red-500">*</span></label>
            <input
                v-model="localItem.lotQuantity"
                class="w-full p-2 border rounded"
                required
                type="number"
                min="0"
            />
          </div>

          <div>
            <label class="block mb-1">Unit <span class="text-red-500">*</span></label>
            <select
                v-model="localItem.lotUnit"
                class="w-full p-2 border rounded"
                required
            >
              <option value="Kilograms">Kilograms</option>
              <option value="Liters">Liters</option>
              <option value="Units">Units</option>
            </select>
          </div>

          <div>
            <label class="block mb-1">Raw Material <span class="text-red-500">*</span></label>
            <select
                v-model="localItem.rawMaterialId"
                class="w-full p-2 border rounded"
                required
            >
              <option
                  v-for="material in rawMaterials"
                  :key="material.rawId"
                  :value="material.rawId"
              >
                {{ material.name }}
              </option>
            </select>
          </div>
        </div>
        <div class="flex justify-end gap-2 mt-6">
          <Button
              type="button"
              @click="$emit('close')"
          >
            Cancel
          </Button>
          <Button
              type="submit"
          >
            Save
          </Button>
        </div>
      </form>
    </div>
  </div>
</template>
