<script lang="ts">
import {defineComponent, ref} from 'vue'
import Modal from "@/components/Modal.vue";
import Button from "@/components/Button.vue";

export default defineComponent({
  components: {Button, Modal},
  props: {
    state: {
      type: Object,
      required: true,
    },
  },
  setup(props) {
    const input = ref("")

    const submit = () => {
      if (props.state.type === 'ask') {
        props.state.resolvePrompt(input.value || null)
      } else if (props.state.type === 'confirm') {
        props.state.resolvePrompt(true)
      }
      reset()
    }

    const cancel = () => {
      if (props.state.type === 'ask') {
        props.state.resolvePrompt(null)
      } else if (props.state.type === 'confirm') {
        props.state.resolvePrompt(false)
      }
      reset()
    }

    const reset = () => {
      props.state.visible = false
      props.state.resolvePrompt = null
      props.state.type = null
      props.state.message = null
      input.value = ''
    }

    return {
      input,
      submit,
      cancel,
    }
  },
})
</script>

<template>
  <Modal :visible="state.visible" @close="cancel" :close="false">
    <div class="flex flex-col gap-2">
      <p class="text-lg">{{ state.message }}</p>
      <template v-if="state.type === 'ask'">
        <input v-model="input" placeholder="Type your response..." />
      </template>
    </div>

    <template v-slot:actions>
      <Button @click="cancel" class="danger secondary">{{ state.type === 'ask' ? 'Cancel' : 'No' }}</Button>
      <Button @click="submit">{{ state.type === 'ask' ? 'Submit' : 'Yes' }}</Button>
    </template>
  </Modal>
</template>

<style>

</style>
