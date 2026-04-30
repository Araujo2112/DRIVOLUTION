<script setup lang="ts">
import Title from "@/components/Title.vue";
import EmployeeTable from "@/components/EmployeeTable.vue";
import {onMounted, ref} from "vue";
import {Employee} from "@/models/Employee.ts";
import axios from '@/axios.ts';

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

onMounted(fetchallEmployees);
</script>

<template>
  <Title>Funcionários</Title>

  <EmployeeTable
      :employees="employees"
      @save="fetchallEmployees"
      @delete="fetchallEmployees"
      controls
  />
</template>

<style scoped>

</style>