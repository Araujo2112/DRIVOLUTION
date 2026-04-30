<template>
  <table class="w-full">
    <thead>
    <tr class="bg-gray-500">
      <th class="p-2 text-left">Item Code</th>
      <th class="p-2 text-left">Container</th>
      <th class="p-2 text-left">Entry</th>
      <th class="p-2 text-left">Exit</th>
      <th class="p-2 text-right">Actions</th>
    </tr>
    </thead>
    <tbody>
    <tr
        v-for="item in items"
        :key="item.itemCode"
        class="border-b"
    >
      <td class="p-2">{{ item.itemCode }}</td>
      <td class="p-2">
        {{ containers.find(c => c.containerId === item.containerId)?.containerName || 'N/A' }}
      </td>
      <td class="p-2">{{ new Date(item.dateTimeIn).toLocaleString() }}</td>
      <td class="p-2">{{ item.dateTimeOut ? new Date(item.dateTimeOut).toLocaleString() : '-' }}</td>
      <td class="p-2 flex gap-2 justify-end">

        <!--
        <Button
            icon="edit" variant="ghost"
            @click="$emit('edit', item)"
        />
        -->

      </td>
    </tr>
    </tbody>
  </table>
</template>

<script lang="ts" setup>
import type {Containers, ItemInContainer} from "@/types/Interfaces";
import Button from "@/components/Button.vue";

defineProps<{
  items: ItemInContainer[];
  containers: Containers[];
}>();

defineEmits(["edit", "delete"]);
</script>
