<template>
  <table class="w-full">
    <thead>
    <tr class="bg-gray-500">
      <th class="text-left p-2">Código</th>
      <th class="text-left p-2">Quantidade</th>
      <th class="text-left p-2">Unidade</th>
      <th class="text-left p-2">Lote</th>
      <th class="text-left p-2">Container</th>
      <th class="text-right p-2">Ações</th>
    </tr>
    </thead>
    <tbody>
    <tr
        v-for="item in items"
        :key="item.itemRawId"
        class="border-b"
    >
      <td class="p-2">{{ item.itemCode }}</td>
      <td class="p-2">{{ item.quantity }}</td>
      <td class="p-2">{{ item.unit }}</td>
      <td class="p-2">{{ item.lotOfRawMaterialId }}</td>
      <td class="p-2">{{ item.itemInContainerId }}</td>
      <td class="p-2 flex gap-2 justify-end">
        <Button
            icon="edit"
            variant="ghost"
            @click="$emit('edit', item)"
        />
        <Button
            icon="delete"
            variant="ghost"
            @click="$emit('delete', item.itemCode)"
        />
      </td>
    </tr>
    </tbody>
  </table>
</template>

<script lang="ts" setup>
import Button from "@/components/Button.vue";
import type { ItemOfRawMaterial } from "@/types/Interfaces.ts";

const props = defineProps<{
  items: ItemOfRawMaterial[];
}>();

console.log('Tabela recebeu items:', props.items);
</script>