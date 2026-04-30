<script setup lang="ts">
import Title from "@/components/Title.vue";
import EmployeeTable from "@/components/EmployeeTable.vue";
import Button from "@/components/Button.vue";
import SectionCard from "@/components/SectionCard.vue";
import {onMounted, ref} from "vue";
import {Employee} from "@/models/Employee.ts";
import axios from "@/axios";

const employees = ref<Employee[]>([]);
const error = ref<string | null>(null);

async function fetchallEmployees() {
  try {
    const token = localStorage.getItem('texpact_token');

    const response = await axios.get(import.meta.env.VITE_API_URL + '/Employee/', {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    employees.value = response.data;
  } catch (err: unknown) {
    if (err instanceof Error) {
      error.value = err.message;
    } else {
      error.value = "An unknown error occurred.";
    }
  }
}

onMounted( async () => {
  await fetchallEmployees()
});
</script>

<template>
  <Title>Dashboard</Title>

  <div class="grid grid-cols-1 grid-rows-1 gap-4">
    <SectionCard icon="table_eye" title="Contentores">
      <div class="flex flex-col justify-center items-center gap-2">
        <Button icon="multimodal_hand_eye" path="dashboard/funcionarios">Ver todos</Button>
      </div>
    </SectionCard>
  </div>

  <div class="grid grid-cols-3 grid-rows-2 gap-4">
    <SectionCard icon="table_eye" title="Estado Funcionários" class="row-span-2 col-span-2">
      <div class="flex flex-col justify-center items-center gap-2">
        <EmployeeTable :employees="employees.slice(0, 5)" />
        <Button icon="multimodal_hand_eye" path="dashboard/funcionarios">Ver todos</Button>
      </div>
    </SectionCard>

    <SectionCard icon="devices" title="Dispositivos">
      <div class="flex flex-col gap-2">
        <p class="text-2xl font-bold">200</p>
      </div>
    </SectionCard>

    <SectionCard icon="mood" title="Stress Médio">
      <div class="flex flex-col gap-2">
        <p class="text-2xl font-bold">20%</p>
      </div>
    </SectionCard>
  </div>
</template>

<style scoped>

</style>