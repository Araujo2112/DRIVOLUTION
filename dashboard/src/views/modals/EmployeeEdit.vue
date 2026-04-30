<script setup lang="ts">
import {ref, watch} from 'vue'
import Modal from '@/components/Modal.vue'
import Button from '@/components/Button.vue'
import {Employee} from '@/models/Employee.ts'
import axios from '@/axios.ts'
import {toast} from "@/plugins/toast.ts";

const props = defineProps({
  visible: {
    type: Boolean,
    required: true,
  },
  employee: {
    type: Object as () => Employee | null,
    default: null,
  },
})

const emit = defineEmits(['close', 'save'])

const form = ref({
  firstName: '',
  lastName: '',
  username: '',
  password: '',
  watchId: ''
})

watch(
    () => props.employee,
    (newEmployee) => {
      if (newEmployee) {
        form.value.firstName = newEmployee.firstName || ''
        form.value.lastName = newEmployee.lastName || ''
        form.value.username = newEmployee.username || ''
        form.value.watchId = newEmployee.watchId || ''
      } else {
        form.value.firstName = ''
        form.value.lastName = ''
        form.value.username = ''
        form.value.watchId = ''
      }
    },
    { immediate: true }
)

const closeModal = () => {
  emit('close')
  form.value = {
    firstName: '',
    lastName: '',
    username: '',
    password: '',
    watchId: ''
  }
}

const saveEmployee = async () => {
  try {
    const token = localStorage.getItem('texpact_token')

    if (props.employee) {
      await axios.put(
          import.meta.env.VITE_API_URL + `/Employee/${props.employee.id}`,
          {
            firstName: form.value.firstName,
            lastName: form.value.lastName,
            username: form.value.username,
            watchId: form.value.watchId
          },
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
      )

      emit('save')
      toast.success("Employee edited")
    } else {
      await axios.post(
          import.meta.env.VITE_API_URL + '/Employee/',
          {
            firstName: form.value.firstName,
            lastName: form.value.lastName,
            username: form.value.username,
            password: form.value.password,
            watchId: form.value.watchId
          },
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
      )

      emit('save')
      toast.success("Employee created")
    }
    closeModal()
  } catch (e) {
    console.error(e)
    toast.error("Error creating employee")
  }
}
</script>

<template>
  <Modal :visible="props.visible" :title="employee ? 'Editar funcionário' : 'Novo funcionário'" @close="closeModal">
    <form class="flex flex-col gap-4">
      <div class="form-group">
        <label for="firstName" class="block text-sm font-medium">Primeiro nome:</label>
        <input
            id="firstName"
            type="text"
            v-model="form.firstName"
        />
      </div>

      <div class="form-group">
        <label for="lastName" class="block text-sm font-medium">Último nome:</label>
        <input
            id="lastName"
            type="text"
            v-model="form.lastName"
        />
      </div>

      <div class="form-group">
        <label for="username" class="block text-sm font-medium">Nome de utilizador:</label>
        <input
            id="username"
            type="text"
            v-model="form.username"
        />
      </div>

      <div class="form-group" v-if="!props.employee">
        <label for="password" class="block text-sm font-medium">Palavra-passe:</label>
        <input
            id="password"
            type="password"
            v-model="form.password"
        />
      </div>

      <div class="form-group">
        <label for="watchId" class="block text-sm font-medium">ID Smartwatch:</label>
        <input
            id="watchId"
            type="text"
            v-model="form.watchId"
        />
      </div>
    </form>

    <template v-slot:actions>
      <Button class="empty" @click="closeModal" type="button">Cancelar</Button>
      <Button @click="saveEmployee" type="button">Guardar</Button>
    </template>
  </Modal>
</template>

<style scoped>
</style>
