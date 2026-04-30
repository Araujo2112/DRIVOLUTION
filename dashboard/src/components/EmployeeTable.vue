<script setup lang="ts">
import {ref, watch} from 'vue'
import Button from "@/components/Button.vue"
import {Employee} from "@/models/Employee.ts"
import EmployeeEdit from "@/views/modals/EmployeeEdit.vue"
import {prompt} from "@/plugins/prompt.ts"
import axios from "@/axios.ts"
import {toast} from "@/plugins/toast.ts";
import {getActiveTime, getRecentHeartrate, getStepsToday} from "@/services/healthService"

const props = defineProps({
  employees: {
    type: Array as () => Employee[]
  },
  controls: {
    type: Boolean,
    default: false
  }
})

const showModal = ref<boolean>(false)
const modalData = ref(null)

const emit = defineEmits(['delete', 'save'])

const openModal = (data: any = null) => {
  showModal.value = true
  modalData.value = data
}

const closeModal = () => {
  showModal.value = false
  modalData.value = null
}

const deleteEmployee = async (employeeId: string) => {
  const response = await prompt.confirm("Are you sure?")
  if(!response) return
  
  try {
    const token = localStorage.getItem('texpact_token')

    await axios.delete(import.meta.env.VITE_API_URL + `/Employee/${employeeId}`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    })

    toast.success("Employee deleted")
    emit('delete')
  } catch (e) {
    console.error(e)
    toast.error("Error deleting employee")
  }
}

const saveEmployee = () => {
  closeModal()
  emit('save')
}

const employeeHealth = ref<Record<string, { bpm: number, steps: number, activeTime: number }>>({})

const fetchHealthData = async () => {
  const token = localStorage.getItem('texpact_token')

  for (const employee of props.employees) {
    if (!employee.watchId) continue

    const heartRateData = await getRecentHeartrate(employee.watchId, token)
    const steps = await getStepsToday(employee.watchId, token)
    const activeTime = await getActiveTime(employee.watchId, token)

    employeeHealth.value[employee.id] = {
      bpm: heartRateData.length > 0 ? heartRateData.slice(-1)[0].heartRate : 0,
      steps: steps || 0,
      activeTime: activeTime || 0
    }
  }
}

watch(
    () => props.employees,
    async (newEmployees) => {
      if (newEmployees.length > 0) {
        await fetchHealthData();
      }
    },
    { immediate: true }
);
</script>

<template>
  <div class="w-full flex flex-col gap-2 items-end">
    <Button v-if="controls" icon="add" @click="openModal()">Novo Funcionário</Button>

    <table>
      <tbody>
        <tr v-for="employee in props.employees" :key="employee.id">
          <td>
            <div>
              <span class="material-symbols-rounded">account_circle</span>
              {{ employee.firstName }} {{ employee.lastName }}
            </div>
          </td>

          <td>
            <div v-if="employee.watchId">
              <span class="material-symbols-rounded">cardiology</span>
              {{ employeeHealth[employee.id]?.bpm || 0 }} bpm
            </div>
          </td>

          <td>
            <div v-if="employee.watchId">
              <span class="material-symbols-rounded">footprint</span>
              {{ employeeHealth[employee.id]?.steps || 0 }} passos
            </div>
          </td>

          <td>
            <div v-if="employee.watchId">
              <span class="material-symbols-rounded">schedule</span>
              {{ employeeHealth[employee.id]?.activeTime || 0 }} mins
            </div>
          </td>

          <td v-if="props.controls" class="controls">
            <div>
              <Button icon="edit" @click="openModal(employee)"></Button>
              <Button icon="visibility" :path="`funcionarios/${employee.id}`"></Button>
              <Button icon="delete" class="danger" @click="deleteEmployee(employee.id)"></Button>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  </div>

  <EmployeeEdit
      v-if="controls"
      :visible="showModal"
      :employee="modalData"
      @close="closeModal"
      @save="saveEmployee"/>
</template>

<style scoped>

</style>