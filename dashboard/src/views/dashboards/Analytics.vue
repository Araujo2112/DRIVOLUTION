<template>
  <div class="p-8 w-full">
    <div class="flex items-start justify-between mb-8">
      <div>
        <h1 class="text-2xl font-medium text-background-900 dark:text-background-50">
          {{ t('analytics.title') }}
        </h1>
        <p class="text-sm text-background-600 dark:text-background-400 mt-1">
          {{ t('analytics.subtitle') }}
        </p>
      </div>

      <button
        @click="loadAnalytics"
        class="flex items-center gap-2 bg-primary-500 hover:bg-primary-600 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
      >
        <span class="material-symbols-rounded text-base">refresh</span>
        {{ t('analytics.refresh') }}
      </button>
    </div>

    <div v-if="loading" class="flex items-center gap-2 text-background-500 text-sm py-12">
      <span class="material-symbols-rounded animate-spin text-lg">autorenew</span>
      {{ t('analytics.loading') }}
    </div>

    <div v-else class="flex flex-col gap-8">
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5 flex items-center justify-between">
          <div>
            <p class="text-xs font-medium text-background-500 uppercase tracking-wider">
              {{ t('analytics.kpis.averageTime') }}
            </p>
            <p class="text-3xl font-medium text-primary-500 mt-2">
              {{ averageGlobalTime }} min
            </p>
          </div>
          <span class="material-symbols-rounded text-primary-500 text-3xl opacity-70">
            timer
          </span>
        </div>

        <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5 flex items-center justify-between">
          <div>
            <p class="text-xs font-medium text-background-500 uppercase tracking-wider">
              {{ t('analytics.kpis.averageRework') }}
            </p>
            <p
              class="text-3xl font-medium mt-2"
              :class="averageReworkRate === 0 ? 'text-success-500' : 'text-danger-500'"
            >
              {{ averageReworkRate }}%
            </p>
          </div>
          <span
            class="material-symbols-rounded text-3xl opacity-70"
            :class="averageReworkRate === 0 ? 'text-success-500' : 'text-danger-500'"
          >
            {{ averageReworkRate === 0 ? 'verified' : 'build' }}
          </span>
        </div>

        <div class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5 flex items-center justify-between">
          <div>
            <p class="text-xs font-medium text-background-500 uppercase tracking-wider">
              {{ t('analytics.kpis.completedProducts') }}
            </p>
            <p class="text-3xl font-medium text-success-500 mt-2">
              {{ totalCompletedProducts }}
            </p>
          </div>
          <span class="material-symbols-rounded text-success-500 text-3xl opacity-70">
            inventory_2
          </span>
        </div>
      </div>

      <section class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5">
        <h2 class="text-lg font-medium text-background-900 dark:text-background-50 mb-1">
          {{ t('analytics.phaseDurations.title') }}
        </h2>
        <p class="text-xs text-background-500 mb-4">
          {{ t('analytics.phaseDurations.subtitle') }}
        </p>
        <div ref="phaseChart" class="w-full h-[320px]"></div>
      </section>

      <section
        :class="[
          'bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl',
          averageReworkRate > 0 ? 'p-5' : 'px-5 pt-5 pb-0'
        ]"
      >
        <h2 class="text-lg font-medium text-background-900 dark:text-background-50 mb-1">
          {{ t('analytics.reworkRate.title') }}
        </h2>
        <p class="text-xs text-background-500 mb-4">
          {{ t('analytics.reworkRate.subtitle') }}
        </p>
        <div
          ref="reworkChart"
          :class="[
            'w-full',
            averageReworkRate > 0 ? 'h-[320px]' : 'h-[90px]'
          ]"
        />
      </section>

      <section class="bg-background-50 dark:bg-background-800 border border-background-300 dark:border-background-700 rounded-xl p-5">
        <h2 class="text-lg font-medium text-background-900 dark:text-background-50 mb-1">
          {{ t('analytics.throughput.title') }}
        </h2>
        <p class="text-xs text-background-500 mb-4">
          {{ t('analytics.throughput.subtitle') }}
        </p>
        <div ref="throughputChart" class="w-full h-[320px]"></div>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue'
import * as d3 from 'd3'
import { useI18n } from 'vue-i18n'
import { analyticsService } from '@/services/analyticsService'
import type { PhaseDuration, ReworkRate, Throughput } from '@/services/analyticsService'
import { toast } from '@/plugins/toast'

const { t, locale } = useI18n()

const loading = ref(true)

const phaseDurations = ref<PhaseDuration[]>([])
const reworkRates = ref<ReworkRate[]>([])
const throughput = ref<Throughput[]>([])

const phaseChart = ref<HTMLDivElement>()
const reworkChart = ref<HTMLDivElement>()
const throughputChart = ref<HTMLDivElement>()

const averageGlobalTime = computed(() => {
  if (phaseDurations.value.length === 0) return 0

  const total = phaseDurations.value.reduce(
    (sum, item) => sum + item.averageDurationMinutes,
    0,
  )

  return Math.round((total / phaseDurations.value.length) * 10) / 10
})

const averageReworkRate = computed(() => {
  if (reworkRates.value.length === 0) return 0

  const total = reworkRates.value.reduce(
    (sum, item) => sum + item.reworkRatePercent,
    0,
  )

  return Math.round((total / reworkRates.value.length) * 10) / 10
})

const totalCompletedProducts = computed(() => {
  return throughput.value.reduce(
    (sum, item) => sum + item.completedProducts,
    0,
  )
})

watch(locale, async () => {
  await nextTick()
  drawCharts()
})

onMounted(async () => {
  await loadAnalytics()
  window.addEventListener('resize', drawCharts)
})

onUnmounted(() => {
  window.removeEventListener('resize', drawCharts)
})

async function loadAnalytics() {
  loading.value = true

  try {
    const [phaseRes, reworkRes, throughputRes] = await Promise.all([
      analyticsService.getPhaseDurations(),
      analyticsService.getReworkRate(),
      analyticsService.getThroughput(),
    ])

    phaseDurations.value = phaseRes.data
    reworkRates.value = reworkRes.data
    throughput.value = throughputRes.data

    loading.value = false
    await nextTick()
    drawCharts()
  } catch {
    toast.error(t('analytics.loadError'))
    loading.value = false
  }
}

function drawCharts() {
  drawBarChart(
    phaseChart.value,
    phaseDurations.value.map(x => ({
      label: x.phaseName,
      value: x.averageDurationMinutes,
      suffix: 'min',
    })),
  )

  drawBarChart(
    reworkChart.value,
    reworkRates.value.map(x => ({
      label: x.phaseName,
      value: x.reworkRatePercent,
      suffix: '%',
    })),
    t('analytics.reworkRate.empty'),
  )

  drawLineChart(
    throughputChart.value,
    throughput.value.map(x => ({
      label: new Date(x.period).toLocaleDateString('pt-PT'),
      value: x.completedProducts,
    })),
  )
}

function drawBarChart(
  container: HTMLDivElement | undefined,
  data: { label: string; value: number; suffix: string }[],
  emptyMessage?: string,
) {
  if (!container) return

  container.innerHTML = ''

  const width = container.clientWidth
  const height = container.clientHeight || 320
  const margin = { top: 20, right: 20, bottom: 55, left: 55 }

  const svg = d3
    .select(container)
    .append('svg')
    .attr('width', width)
    .attr('height', height)

  if (data.length === 0 || data.every(d => d.value === 0)) {
    svg.append('text')
      .attr('x', width / 2)
      .attr('y', height / 2 - 12)
      .attr('text-anchor', 'middle')
      .attr('font-size', 28)
      .attr('fill', '#22c55e')
      .text('✓')

    svg.append('text')
      .attr('x', width / 2)
      .attr('y', height / 2 + 18)
      .attr('text-anchor', 'middle')
      .attr('font-size', 14)
      .attr('fill', '#64748b')
      .text(emptyMessage ?? t('analytics.noData'))

    return
  }

  const x = d3
    .scaleBand()
    .domain(data.map(d => d.label))
    .range([margin.left, width - margin.right])
    .padding(0.25)

  const y = d3
    .scaleLinear()
    .domain([0, d3.max(data, d => d.value) || 1])
    .nice()
    .range([height - margin.bottom, margin.top])

  svg.append('g')
    .attr('transform', `translate(0,${height - margin.bottom})`)
    .call(d3.axisBottom(x))
    .selectAll('text')
    .attr('transform', 'rotate(-25)')
    .style('text-anchor', 'end')

  svg.append('g')
    .attr('transform', `translate(${margin.left},0)`)
    .call(d3.axisLeft(y))

  svg.selectAll('rect')
    .data(data)
    .enter()
    .append('rect')
    .attr('x', d => x(d.label) ?? 0)
    .attr('y', d => y(d.value))
    .attr('width', x.bandwidth())
    .attr('height', d => y(0) - y(d.value))
    .attr('rx', 6)
    .attr('fill', '#3E55F2')

  svg.selectAll('.value-label')
    .data(data)
    .enter()
    .append('text')
    .attr('x', d => (x(d.label) ?? 0) + x.bandwidth() / 2)
    .attr('y', d => y(d.value) - 6)
    .attr('text-anchor', 'middle')
    .attr('font-size', 11)
    .attr('fill', '#64748b')
    .text(d => `${d.value}${d.suffix}`)
}

function drawLineChart(
  container: HTMLDivElement | undefined,
  data: { label: string; value: number }[],
) {
  if (!container) return

  container.innerHTML = ''

  const width = container.clientWidth
  const height = container.clientHeight || 320
  const margin = { top: 20, right: 25, bottom: 55, left: 55 }

  const svg = d3
    .select(container)
    .append('svg')
    .attr('width', width)
    .attr('height', height)

  if (data.length === 0) {
    svg.append('text')
      .attr('x', width / 2)
      .attr('y', height / 2)
      .attr('text-anchor', 'middle')
      .attr('fill', '#94a3b8')
      .text(t('analytics.noData'))

    return
  }

  const x = d3
    .scalePoint()
    .domain(data.map(d => d.label))
    .range([margin.left, width - margin.right])
    .padding(0.5)

  const y = d3
    .scaleLinear()
    .domain([0, d3.max(data, d => d.value) || 1])
    .nice()
    .range([height - margin.bottom, margin.top])

  svg.append('g')
    .attr('transform', `translate(0,${height - margin.bottom})`)
    .call(d3.axisBottom(x))
    .selectAll('text')
    .attr('transform', 'rotate(-25)')
    .style('text-anchor', 'end')

  svg.append('g')
    .attr('transform', `translate(${margin.left},0)`)
    .call(d3.axisLeft(y))

  const area = d3
    .area<{ label: string; value: number }>()
    .x(d => x(d.label) ?? margin.left)
    .y0(height - margin.bottom)
    .y1(d => y(d.value))

  svg.append('path')
    .datum(data)
    .attr('fill', '#3E55F2')
    .attr('opacity', 0.18)
    .attr('d', area)

  const line = d3
    .line<{ label: string; value: number }>()
    .x(d => x(d.label) ?? margin.left)
    .y(d => y(d.value))

  svg.append('path')
    .datum(data)
    .attr('fill', 'none')
    .attr('stroke', '#3E55F2')
    .attr('stroke-width', 3)
    .attr('d', line)

  svg.selectAll('circle')
    .data(data)
    .enter()
    .append('circle')
    .attr('cx', d => x(d.label) ?? margin.left)
    .attr('cy', d => y(d.value))
    .attr('r', 5)
    .attr('fill', '#3E55F2')

  svg.selectAll('.value-label')
    .data(data)
    .enter()
    .append('text')
    .attr('x', d => x(d.label) ?? margin.left)
    .attr('y', d => y(d.value) - 10)
    .attr('text-anchor', 'middle')
    .attr('font-size', 11)
    .attr('fill', '#64748b')
    .text(d => d.value)
}
</script>