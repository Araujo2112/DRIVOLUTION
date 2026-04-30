<script setup lang="ts">
import { ref, onMounted, watch, onUnmounted } from 'vue'
import * as d3 from 'd3'

interface Props {
  data: Array<{
    timeIndex: string,
    value: number
  }>,
  xLabel?: string,
  yLabel?: string,
  color?: string,
  width?: number,
  height?: number,
  yMin?: number,
  yMax?: number,
  yPadding?: number,
  showPoints?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  xLabel: 'Time',
  yLabel: 'Value',
  color: '#ff4757',
  height: 200,
  yPadding: 5,
  showPoints: false
})

const chartRef = ref<HTMLElement | null>(null)
const containerWidth = ref(0)

const margin = { top: 20, right: 20, bottom: 30, left: 50 }

let resizeObserver: ResizeObserver | null = null

const createChart = () => {
  if (!chartRef.value || !props.data.length) return


  const parentWidth = chartRef.value.getBoundingClientRect().width

  const width = props.width || Math.min(containerWidth.value, parentWidth) || parentWidth || 300

  d3.select(chartRef.value).selectAll("*").remove()

  const svg = d3.select(chartRef.value)
      .append("svg")
      .style("width", "100%")
      .style("height", "100%")
      .attr("preserveAspectRatio", "xMidYMid meet")
      .attr("viewBox", `0 0 ${width} ${props.height}`)

  const g = svg.append("g")
      .attr("transform", `translate(${margin.left},${margin.top})`)

  const parseTime = d3.isoParse

  const now = new Date()
  const oneHourAgo = new Date(now.getTime() - 60 * 60 * 1000)

  const x = d3.scaleTime()
      .domain([oneHourAgo, now])
      .range([0, width - margin.left - margin.right])

  const minValue = props.yMin !== undefined
      ? props.yMin
      : Math.min(d3.min(props.data, d => d.value) as number - props.yPadding)

  const maxValue = props.yMax !== undefined
      ? props.yMax
      : Math.max(d3.max(props.data, d => d.value) as number + props.yPadding)

  const y = d3.scaleLinear()
      .domain([minValue, maxValue])
      .range([props.height - margin.top - margin.bottom, 0])

  const line = d3.line<typeof props.data[0]>()
      .x(d => x(parseTime(d.timeIndex)!))
      .y(d => y(d.value))
      .defined(d => parseTime(d.timeIndex)! >= oneHourAgo)
      .curve(d3.curveMonotoneX)

  g.append("g")
      .attr("transform", `translate(0,${props.height - margin.top - margin.bottom})`)
      .call(d3.axisBottom(x)
          .ticks(d3.timeMinute.every(10))
          .tickFormat(d3.utcFormat("%H:%M")))
      .append("text")
      .attr("fill", "#000")
      .attr("x", width - margin.left - margin.right)
      .attr("y", -10)
      .attr("text-anchor", "end")
      .text(props.xLabel)

  g.append("g")
      .call(d3.axisLeft(y))
      .append("text")
      .attr("fill", "#000")
      .attr("transform", "rotate(-90)")
      .attr("y", 6)
      .attr("dy", "0.71em")
      .attr("text-anchor", "end")
      .text(props.yLabel)


  const path = g.append("path")
      .datum(props.data)
      .attr("fill", "none")
      .attr("stroke", props.color)
      .attr("stroke-width", 2)
      .attr("d", line)

  if (props.showPoints) {
    g.selectAll("circle")
        .data(props.data)
        .enter()
        .append("circle")
        .attr("cx", d => x(parseTime(d.timeIndex)!))
        .attr("cy", d => y(d.value))
        .attr("r", 4)
        .attr("fill", props.color)
        .attr("stroke", "white")
        .attr("stroke-width", 2)
  }

  const tooltip = d3.select("body")
      .append("div")
      .attr("class", "tooltip")
      .style("position", "absolute")
      .style("background-color", "white")
      .style("padding", "5px")
      .style("border", "1px solid #ddd")
      .style("border-radius", "4px")
      .style("pointer-events", "none")
      .style("opacity", 0)

  const bisect = d3.bisector<typeof props.data[0], Date>(d => parseTime(d.timeIndex)! as Date).left

  const overlay = g.append("rect")
      .attr("class", "overlay")
      .attr("fill", "none")
      .attr("pointer-events", "all")
      .attr("width", width - margin.left - margin.right)
      .attr("height", props.height - margin.top - margin.bottom)

  const tooltipLine = g.append("line")
      .attr("stroke", "#999")
      .attr("stroke-width", 1)
      .attr("stroke-dasharray", "3,3")
      .style("opacity", 0)

  overlay
      .on("mousemove", (event) => {
        const mouseX = d3.pointer(event)[0]
        const x0 = x.invert(mouseX)
        const i = bisect(props.data, x0 as Date, 1)
        const d0 = props.data[i - 1]
        const d1 = props.data[i]
        if (!d0 || !d1) return

        const d = x0.getTime() - parseTime(d0.timeIndex)!.getTime() > parseTime(d1.timeIndex)!.getTime() - x0.getTime() ? d1 : d0

        const formattedTime = d3.utcFormat("%Y-%m-%d %H:%M:%S")(parseTime(d.timeIndex))

        tooltipLine
            .style("opacity", 1)
            .attr("x1", x(parseTime(d.timeIndex)!))
            .attr("x2", x(parseTime(d.timeIndex)!))
            .attr("y1", 0)
            .attr("y2", props.height - margin.top - margin.bottom)

        tooltip
            .style("opacity", 0.9)
            .html(`Time: ${formattedTime}<br/>${props.yLabel}: ${d.value}`)
            .style("left", (event.pageX + 10) + "px")
            .style("top", (event.pageY - 28) + "px")
      })
      .on("mouseout", () => {
        tooltipLine.style("opacity", 0)
        tooltip.style("opacity", 0)
      })
}

onMounted(() => {
  if (chartRef.value) {
    resizeObserver = new ResizeObserver(entries => {
      const newWidth = entries[0].contentRect.width
      if (newWidth !== containerWidth.value) {
        containerWidth.value = newWidth
        createChart()
      }
    })
    resizeObserver.observe(chartRef.value)
    containerWidth.value = chartRef.value.offsetWidth
    createChart()
  }
})

onUnmounted(() => {
  if (resizeObserver) {
    resizeObserver.disconnect()
  }
})

watch(() => props.data, createChart, { deep: true })
watch(() => props.color, createChart)
watch(() => props.width, createChart)
watch(() => props.height, createChart)
watch(() => props.yMin, createChart)
watch(() => props.yMax, createChart)
watch(() => props.showPoints, createChart)
</script>

<template>
  <div ref="chartRef" class="w-full overflow-hidden"></div>
</template>