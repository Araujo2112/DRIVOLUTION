<script lang="ts" setup>
import {useRoute} from "vue-router";
import {nextTick, onMounted, ref} from "vue";
import router from "@/router";

const route = useRoute();

const paths = route.path.split(/\/dashboard|\//gm).filter(Boolean);

const buildPath = (index: number) => {
  router.push("/dashboard/" + paths.slice(0, index + 1).join("/"))
};

const slotTitle = ref<string | null>(null);
const titleRef = ref<HTMLElement | null>(null);

onMounted(() => {
  nextTick(() => {
    if (titleRef.value) {
      slotTitle.value = titleRef.value.textContent || null;
    }
  });
});

</script>

<template>
  <div class="flex flex-col">
    <div v-if="paths.length > 1" class="flex gap-2 items-center">
      <template v-for="(path, index) in paths">
        <div
            v-if="index < paths.length - 1"
            @click="buildPath(index)"
            class="text-primary-500 hover:underline cursor-pointer capitalize"
        >
          {{ path }}
        </div>
        <span v-else>
          {{ slotTitle || path }}
        </span>
        <span v-if="index < paths.length - 1" class="material-symbols-rounded">chevron_right</span>
      </template>
    </div>

    <h1 ref="titleRef" class="text-3xl">
      <slot></slot>
    </h1>
  </div>
</template>

<style scoped>

</style>