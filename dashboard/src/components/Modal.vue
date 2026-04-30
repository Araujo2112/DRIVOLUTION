<script setup lang="ts">
import Button from "@/components/Button.vue";

const props = defineProps({
  visible: {
    type: Boolean,
    required: true
  },
  title: {
    type: String
  },
  close: {
    type: Boolean,
    default: true
  }
});

const emit = defineEmits();

const closeModal = () => {
  emit('close');
};
</script>

<template>
  <div v-if="props.visible" class="modal" @click="closeModal">
    <div class="content" @click.stop>
      <div v-if="close" class="w-full flex items-center"
           :class="[ props.title ? 'justify-between gap-6' : 'justify-end']">
        <h1 class="text-3xl">{{props.title}}</h1>

        <Button class="empty" icon="close" :round="true" @click="closeModal"></Button>
      </div>


      <div class="slot">
        <slot></slot>
      </div>


      <div v-if="$slots.actions" class="flex justify-end w-full p-2 gap-2">
        <slot name="actions"></slot>
      </div>
    </div>
  </div>
</template>


<style>

.modal{
  @apply fixed top-0 left-0 w-screen h-screen p-6 bg-black/20 flex justify-center items-center z-50;

  .content{
    @apply bg-background-950 py-6 px-4 rounded-xl flex flex-col gap-2 max-h-full overflow-auto;

    .slot{
      @apply flex flex-col gap-2 px-2 py-0.5 max-h-full h-fit overflow-auto;
    }
  }
}

</style>