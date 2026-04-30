<script setup lang="ts">
import Title from "@/components/Title.vue"
import axios from "@/axios.ts"
import {computed, onBeforeUnmount, onMounted, ref} from "vue"
import {useRoute} from "vue-router"
import SectionCard from "@/components/SectionCard.vue"
import {getActiveTime, getRecentHeartrate, getStepsToday} from "@/services/healthService"
import LineChart from '@/components/Graph.vue'
import {Employee} from "@/models/Employee";

const route = useRoute()
interface HeartRateDataPoint {
  timeIndex: string
  heartRate: number
}

const token = localStorage.getItem('texpact_token')

const recentHeartRate = ref<HeartRateDataPoint[]>([])
const currentBPM = ref<Number>(0)
const stepsToday = ref<Number>(0)
const activeTime = ref<Number>(0)
const employee = ref<Employee>({} as Employee)

let updateTimer = null

// Transform heart rate data for the chart component
const chartData = computed(() => {
  return recentHeartRate.value.map(point => ({
    timeIndex: point.timeIndex,
    value: point.heartRate
  }))
})

onMounted(async () => {
  let res = await axios.get(
      import.meta.env.VITE_API_URL + `/Employee/${route.params.id}`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }
  )

  employee.value = res.data
  if(employee.value.watchId){
    await getLatestValues()

    updateTimer = setInterval(() => {
      getLatestValues()
    }, 30000)
  }
})

onBeforeUnmount(() => {
  if(updateTimer != null) {
    clearTimeout(updateTimer)
    updateTimer = null
  }
})

async function getLatestValues() {
  recentHeartRate.value = await getRecentHeartrate(employee.value.watchId, token)
  currentBPM.value = recentHeartRate.value.slice(-1)[0].heartRate
  stepsToday.value = await getStepsToday(employee.value.watchId, token)
  activeTime.value = await getActiveTime(employee.value.watchId, token)
}

const averageHeartRate = computed(() => {
  if (recentHeartRate.value.length === 0) return "N/A";

  const oneWeekAgo = new Date();
  oneWeekAgo.setDate(oneWeekAgo.getDate() - 7);

  const filteredData = recentHeartRate.value.filter(d => new Date(d.timeIndex) >= oneWeekAgo);

  if (filteredData.length === 0) return "N/A";

  const sum = filteredData.reduce((acc, d) => acc + d.heartRate, 0);
  return Math.round(sum / filteredData.length);
});
</script>

<template>
  <Title>{{ employee?.firstName }}</Title>

  <div v-if="employee.watchId" class="flex flex-col gap-2 w-full">
    <div class="grid grid-cols-1 grid-rows-2 md:grid-cols-3 md:grid-rows-1 gap-4 w-full">
      <SectionCard icon="rss_feed" title="Live Data" class="md:col-span-1">
        <div class="flex flex-col justify-center items-center md:flex-row md:justify-between w-full">
          <div class="flex md:flex-col gap-2">
            <div class="flex gap-2 items-center">
              <div class="flex justify-center items-center w-8 h-8 rounded-full bg-red-500">
                <span class="material-symbols-rounded">cardiology</span>
              </div>
              <p>{{currentBPM}} BPM</p>
            </div>

            <div class="flex gap-2 items-center">
              <div class="flex justify-center items-center w-8 h-8 rounded-full bg-green-500">
                <span class="material-symbols-rounded">steps</span>
              </div>
              <p>{{ stepsToday }} Steps</p>
            </div>

            <div class="flex gap-2 items-center">
              <div class="flex justify-center items-center w-8 h-8 rounded-full bg-blue-500">
                <span class="material-symbols-rounded">schedule</span>
              </div>
              <p>{{activeTime}} Mins.</p>
            </div>
          </div>

          <div class="flex flex-1 w-fit h-full relative justify-end items-center">
            <div class="progress-bar w-28 h-28 green"
                 :style="'--percentage: ' + (Number(stepsToday) / 6000) * 100 + '%'">
              <progress></progress>
            </div>

            <div class="progress-bar w-20 h-20 blue absolute right-4 z-10"
                 :style="'--percentage: ' + (Number(activeTime) / 480) * 100 + '%'">
              <progress></progress>
            </div>
          </div>
        </div>
      </SectionCard>

      <SectionCard icon="functions" title="This week" class="md:col-span-2">
        <div class="w-full h-full flex gap-2">
          <div class="flex flex-col gap-2 w-full h-full rounded-xl bg-white p-2">
            <div class="flex gap-2">
              <span class="material-symbols-rounded">cardiology</span>
              Heart Rate (avg.)
            </div>
            <div>
              <p class="text-3xl"><b>{{ averageHeartRate }}</b> BPM</p>
            </div>
          </div>

          <div class="flex flex-col gap-2 w-full h-full rounded-xl bg-white p-2">
            <div class="flex gap-2">
              <span class="material-symbols-rounded">footprint</span>
              Steps
            </div>
            <div>
              <p class="text-3xl">WIP</p>
            </div>
          </div>

          <div class="flex flex-col gap-2 w-full h-full rounded-xl bg-white p-2">
            <div class="flex gap-2">
              <span class="material-symbols-rounded">schedule</span>
              Active Time
            </div>
            <div>
              <p class="text-3xl">WIP</p>
            </div>
          </div>
        </div>
      </SectionCard>
    </div>

    <div class="grid grid-cols-1 grid-rows-1 w-full">
      <SectionCard icon="monitoring" title="Graphs">
        <div class="w-full h-full flex flex-col gap-2">
          <div class="flex flex-col gap-2 w-full h-full rounded-xl bg-white p-2">
            <div class="flex gap-2">
              <span class="material-symbols-rounded">cardiology</span>
              Heart Rate
            </div>
            <div class="h-[200px] max-w-full">
              <LineChart
                  :data="chartData"
                  yLabel="Heart Rate (BPM)"
                  :yMin="40"
                  :yMax="200"
                  :height="200"
              />
            </div>
          </div>

          <div class="flex flex-col gap-2 w-full h-full rounded-xl bg-white p-2">
            <div class="flex gap-2">
              <span class="material-symbols-rounded">sentiment_calm</span>
              Stress
            </div>
            <div class="flex justify-center items-center text-3xl h-full">
              WIP
            </div>
          </div>
        </div>
      </SectionCard>
    </div>
  </div>

  <div v-else class="flex flex-col gap-2 w-full items-center">
    <p>This user has no smartwatch associated</p>
  </div>

</template>

<style scoped>
.progress-bar {
  border-radius: 50%;
  --percentage: 0%;

  &.green{
    background:
        radial-gradient(closest-side, #dce9ef 79%, transparent 80% 100%),
        conic-gradient(#22c55e var(--percentage), pink 0);
  }

  &.blue{
    background:
        radial-gradient(closest-side, #dce9ef 79%, transparent 80% 100%),
        conic-gradient(#3b82f6 var(--percentage), pink 0);
  }

  progress{
    @apply hidden;
  }
}
</style>