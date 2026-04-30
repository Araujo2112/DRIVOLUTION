<template>
  <div class="container">
    <h3 class="title">Container Location History</h3>

    <div class="content">
      <div class="timeline">
        <div v-for="(entry, index) in visibleHistory" :key="index" class="timeline-item">
          <div v-if="index < visibleHistory.length - 1" :class="['timeline-line', getLineColor(entry.sectionId, visibleHistory[index + 1]?.sectionId)]"></div>
          <div class="timeline-content">
            <h4>Section {{ entry.sectionId }}</h4>
            <p class="datetime">{{ new Date(entry.datetime).toLocaleString() }}</p>
          </div>
        </div>
      </div>

      <div class="status">
        <h4>Current Location: <span>{{ currentLocation }}</span></h4>
      </div>
    </div>

    <div class="legend mt-6 p-4 bg-gray-100 rounded-lg shadow">
      <h4 class="text-lg font-semibold mb-3">Legend:</h4>
      <ul class="space-y-2">
        <li class="flex items-center">
          <span class="w-4 h-4 bg-green-500 rounded mr-2"></span>
          <span>Green: The container transitioned to the next section in the correct order.</span>
        </li>
        <li class="flex items-center">
          <span class="w-4 h-4 bg-yellow-500 rounded mr-2"></span>
          <span>Yellow: The container had to remain in the same section.</span>
        </li>
        <li class="flex items-center">
          <span class="w-4 h-4 bg-red-500 rounded mr-2"></span>
          <span>Red: The container skipped one section.</span>
        </li>
        <li class="flex items-center">
          <span class="w-4 h-4 bg-purple-500 rounded mr-2"></span>
          <span>Purple: The container skipped multiple sections.</span>
        </li>
      </ul>
    </div>

    <div v-if="showMoreButton" class="show-more-button">
      <button @click="toggleHistory">{{ isExpanded ? 'Ver menos' : 'Ver mais' }}</button>
    </div>
  </div>
</template>


<script setup lang="ts">
import { defineProps, ref, computed } from 'vue'

const props = defineProps({
  containerDetails: {
    type: Object,
    required: true,
  },
})

const history = ref(props.containerDetails?.container?.localizationHistories || [])
const validHistory = computed(() => history.value.filter(entry => entry && entry.sectionId !== null && entry.sectionId !== undefined))


const initialLimit = 5
const visibleHistory = ref(validHistory.value.slice(-initialLimit).reverse())

const isExpanded = ref(false)

const showMoreButton = computed(() => validHistory.value.length > visibleHistory.value.length)

const toggleHistory = () => {
  if (isExpanded.value) {
    visibleHistory.value = validHistory.value.slice(-initialLimit).reverse() 
  } else {
    visibleHistory.value = validHistory.value.slice().reverse()
  }
  isExpanded.value = !isExpanded.value
}

const lastSectionId = computed(() => validHistory.value.length > 0 ? validHistory.value[validHistory.value.length - 1].sectionId : null)
const currentLocation = computed(() => lastSectionId.value !== null ? `Section ${lastSectionId.value}` : 'Unknown')


const getLineColor = (currentSectionId: number, nextSectionId: number) => {
  if (nextSectionId === undefined) return ''; 

  if (Math.abs(currentSectionId - nextSectionId) > 1) return 'purple'
  
  if (currentSectionId < nextSectionId) return 'red'    
  if (currentSectionId === nextSectionId) return 'yellow'  
  return 'green'  
}
</script>

<style scoped>
.container {
  max-width: 1000px;  
  margin: 0 auto;
  padding: 20px;
  background-color: #fff;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  border-radius: 10px;
}

.title {
  text-align: center;
  font-size: 24px;
  font-weight: 600;
  margin-bottom: 20px;
  color: #333;
}


.content {
  display: flex;
  flex-direction: row; 
  gap: 20px;
  justify-content: space-between;
}

.timeline {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: start;
  max-height: 600px;
  overflow-y: auto; 
}

.timeline-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: 20px;
  position: relative;
}

.timeline-line {
  width: 100%;
  height: 5px;
  margin-top: 10px;
  border-radius: 5px;
  position: absolute;
  bottom: -10px;
  left: 0;
}

.status h4 span {
  color: #63899C;
  font-weight: bold;
}

.timeline-line.red {
  background-color: #dc3545;
}

.timeline-line.yellow {
  background-color: #ffc107;
}

.timeline-line.green {
  background-color: #28a745;
}

.timeline-line.purple {
  background-color: #6f42c1;
}

.timeline-content {
  background: #f4f4f4;
  padding: 15px;
  border-radius: 8px;
  position: relative;
  z-index: 1;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
  width: 100%;
}

.status {
  flex: 0 0 250px; 
  padding: 15px;
  background-color: #f4f4f4;
  border-radius: 8px;
  text-align: center;
  font-size: 20px;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
}

.show-more-button {
  display: flex;
  justify-content: center;
  margin-top: 20px;
}

.show-more-button button {
  padding: 12px 25px;
  background-color: #63899C;
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.show-more-button button:hover {
  background-color: #4f7280;
}

.datetime {
  font-size: 14px;
  color: #888;
}
</style>
