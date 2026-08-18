<template>
  <div>
    <!-- 顶部指标卡片 -->
    <el-row :gutter="16" class="cards">
      <el-col :span="6" v-for="c in cards" :key="c.label">
        <el-card shadow="hover" class="stat-card">
          <div class="stat">
            <el-icon :size="34" :color="c.color"><component :is="c.icon" /></el-icon>
            <div>
              <div class="stat-value">{{ c.value }}</div>
              <div class="stat-label">{{ c.label }}</div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="16" class="charts-row">
      <el-col :span="12">
        <el-card shadow="never" header="销售/采购月度趋势">
          <EChart :option="salesOption" height="320px" />
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card shadow="never" header="生产产量/废品月度趋势">
          <EChart :option="prodOption" height="320px" />
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="16" class="charts-row">
      <el-col :span="8">
        <el-card shadow="never" header="车间库存结构（含预警）">
          <EChart :option="invOption" height="320px" />
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="never" header="工序产量分布">
          <EChart :option="processOption" height="320px" />
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="never" header="生产过程质量">
          <EChart :option="qualityOption" height="320px" />
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="16" class="charts-row">
      <el-col :span="14">
        <el-card shadow="never" header="生产进度" class="progress-card">
          <EChart :option="progressOption" height="320px" />
        </el-card>
      </el-col>
      <el-col :span="10">
        <el-card shadow="never" header="最近动态">
          <el-timeline style="padding: 10px; max-height: 320px; overflow-y: auto;">
            <el-timeline-item v-for="(a, i) in activities" :key="i" :timestamp="formatTime(a.time)" placement="top">
              <b>{{ a.title }}</b>
              <div style="color:#888;font-size:12px">{{ a.desc }}</div>
            </el-timeline-item>
          </el-timeline>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { Odometer, Box, Warning, SetUp } from '@element-plus/icons-vue'
import EChart from '../components/EChart.vue'
import api from '../api/modules'

const overview = ref({})
const invSummary = ref({})
const progress = ref([])
const quality = ref({ summary: {}, trend: [], reasons: [] })
const salesTrend = ref({ months: [], sales: [], purchase: [] })
const prodTrend = ref({ months: [], qty: [], scrap: [] })
const processDist = ref([])
const activities = ref([])

const cards = computed(() => [
  { label: '本月产量', value: overview.value.monthOutput ?? 0, color: '#409eff', icon: SetUp },
  { label: '本月销售金额', value: overview.value.monthSales ?? 0, color: '#67c23a', icon: Odometer },
  { label: '库存预警项', value: overview.value.warningCount ?? 0, color: '#e6a23c', icon: Warning },
  { label: '在产计划数', value: overview.value.inProduction ?? 0, color: '#f56c6c', icon: Box }
])

const salesOption = computed(() => ({
  tooltip: { trigger: 'axis' },
  legend: { data: ['销售金额', '采购金额'] },
  grid: { left: 50, right: 20, top: 40, bottom: 30 },
  xAxis: { type: 'category', data: salesTrend.value.months },
  yAxis: { type: 'value' },
  series: [
    { name: '销售金额', type: 'bar', data: salesTrend.value.sales, itemStyle: { color: '#409eff' } },
    { name: '采购金额', type: 'line', smooth: true, data: salesTrend.value.purchase, itemStyle: { color: '#e6a23c' } }
  ]
}))

const prodOption = computed(() => ({
  tooltip: { trigger: 'axis' },
  legend: { data: ['产量', '废品'] },
  grid: { left: 50, right: 20, top: 40, bottom: 30 },
  xAxis: { type: 'category', data: prodTrend.value.months },
  yAxis: { type: 'value' },
  series: [
    { name: '产量', type: 'bar', data: prodTrend.value.qty, itemStyle: { color: '#67c23a' } },
    { name: '废品', type: 'line', smooth: true, data: prodTrend.value.scrap, itemStyle: { color: '#f56c6c' } }
  ]
}))

const invOption = computed(() => {
  const pieData = [
    { name: '正常', value: invSummary.value.normal ?? 0, itemStyle: { color: '#67c23a' } },
    { name: '预警', value: invSummary.value.warning ?? 0, itemStyle: { color: '#e6a23c' } },
    { name: '缺货', value: invSummary.value.outOfStock ?? 0, itemStyle: { color: '#f56c6c' } }
  ].filter(x => x.value > 0)
  return {
    tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
    legend: { bottom: 0 },
    series: [{ type: 'pie', radius: ['40%', '68%'], data: pieData, label: { formatter: '{b}\n{c}' } }]
  }
})

const processOption = computed(() => ({
  tooltip: { trigger: 'item' },
  legend: { bottom: 0 },
  series: [{ type: 'pie', radius: '55%', data: processDist.value, label: { formatter: '{b}\n{c}', overflow: 'break', width: 100 } }]
}))

const qualityOption = computed(() => {
  const trend = quality.value.trend || []
  return {
    tooltip: { trigger: 'axis' },
    grid: { left: 50, right: 50, top: 40, bottom: 30 },
    xAxis: { type: 'category', data: trend.map(t => t.date) },
    yAxis: [
      { type: 'value', name: '数量' },
      { type: 'value', name: '合格率', max: 100, min: 0 }
    ],
    series: [
      { name: '检验数量', type: 'bar', data: trend.map(t => t.inspectQty), itemStyle: { color: '#409eff' } },
      { name: '缺陷数量', type: 'bar', data: trend.map(t => t.defectQty), itemStyle: { color: '#f56c6c' } },
      { name: '合格率%', type: 'line', smooth: true, yAxisIndex: 1, data: trend.map(t => t.passRate), itemStyle: { color: '#67c23a' } }
    ]
  }
})

const progressOption = computed(() => {
  const labels = progress.value.map(p => p.planNo + ' ' + p.projectName)
  const maxLen = labels.reduce((m, s) => Math.max(m, s.length), 0)
  return {
    tooltip: { trigger: 'axis', formatter: p => `${p[0].name}<br/>${p[0].marker}进度: ${p[0].value}%` },
    grid: { left: maxLen * 14 + 16, right: 40, top: 20, bottom: 30 },
    xAxis: { type: 'value', max: 100, axisLabel: { formatter: '{value}%' } },
    yAxis: { type: 'category', data: labels },
    series: [{
      type: 'bar', data: progress.value.map(p => p.progress), barWidth: 14,
      itemStyle: { color: p => p.value >= 100 ? '#67c23a' : p.value >= 50 ? '#409eff' : '#e6a23c' },
      label: { show: true, position: 'right', formatter: '{c}%' }
    }]
  }
})

function formatTime(t) {
  if (!t) return ''
  const d = new Date(t)
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')} ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
}

onMounted(async () => {
  const [ov, inv, pr, qu, st, pt, pd, ac] = await Promise.all([
    api.overview(), api.invSummary(), api.prodProgress(), api.quality(),
    api.salesTrend(), api.prodTrend(), api.processDist(), api.activities()
  ])
  overview.value = ov
  invSummary.value = inv
  progress.value = pr
  quality.value = qu
  salesTrend.value = st
  prodTrend.value = pt
  processDist.value = pd
  activities.value = ac
})
</script>

<style scoped>
.cards { margin-bottom: 16px; }
.stat-card :deep(.el-card__body) { padding: 18px; }
.stat { display: flex; align-items: center; gap: 14px; }
.stat-value { font-size: 26px; font-weight: bold; color: #1f3b73; }
.stat-label { font-size: 13px; color: #888; margin-top: 2px; }
.charts-row { margin-bottom: 16px; }
.progress-card :deep(.el-card__body) { overflow: visible; }
</style>
