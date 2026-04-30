<script setup lang="ts">

import {defineEmits, defineProps, ref, watch} from "vue";

import Button from "@/components/Button.vue";

interface Container {
  containerCode: string;
  containerName: string;
  containerVolume: number;
  containerDescription: string;
}

const props = defineProps({
  container: {
    type: Object as () => Container,
    required: true,
  },
});

const emit = defineEmits(["updateContainer", "closeModal"]);

const editedContainer = ref<Container>({
  containerCode: "",
  containerName: "",
  containerVolume: 0,
  containerDescription: "",
});

watch(
    () => props.container,
    (newVal) => {
      if (newVal) {
        editedContainer.value = {...newVal};
      }
    },
    {immediate: true}
);

const saveChanges = async () => {
  try {
    const formData = new FormData();
    formData.append("containerName", editedContainer.value.containerName);
    formData.append("containerVolume", editedContainer.value.containerVolume.toString());
    formData.append("containerDescription", editedContainer.value.containerDescription);

    const response = await fetch(`http://localhost:5181/api/Container/${editedContainer.value.containerCode}`, {
      method: "PUT",
      headers: {
        "accept": "*/*",
      },
      body: formData,
    });

    if (!response.ok) {
      throw new Error(`Erro HTTP: ${response.status}`);
    }

    emit("updateContainer", editedContainer.value);
  } catch (err) {
    console.error("Erro ao atualizar o container:", err);
  }
};

const close = () => {
  emit("closeModal");
};
</script>

<template>
  <div class="modal-content">
    <h2>Editar Contentor</h2>
    <form @submit.prevent="saveChanges">
      <div>
        <label for="containerName">Nome</label>
        <input
            id="containerName"
            v-model="editedContainer.containerName"
            required
            type="text"
        />
      </div>
      <div>
        <label for="containerVolume">Volume</label>
        <input
            id="containerVolume"
            v-model.number="editedContainer.containerVolume"
            required
            type="number"
        />
      </div>
      <div>
        <label for="containerDescription">Descrição</label>
        <textarea
            id="containerDescription"
            v-model="editedContainer.containerDescription"
            required
        ></textarea>
      </div>
      <div class="actions">
        <Button icon="save" type="submit">Guardar</Button>
        <Button icon="close" type="button" @click="close">Cancelar</Button>
      </div>
    </form>
  </div>
</template>
