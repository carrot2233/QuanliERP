<template>
  <div class="dashboard">
    <!-- 欢迎横幅 -->
    <div class="welcome-banner">
      <div class="welcome-left">
        <h2>欢迎回来，{{ auth.displayName }}</h2>
        <p>鹤壁市全力模具制造有限公司 · {{ todayStr }}</p>
      </div>
      <div class="welcome-right">
        <div class="weather-info">
          <span class="greeting">{{ greeting }}</span>
        </div>
      </div>
    </div>

    <!-- 核心指标 -->
    <div class="stat-cards">
      <div v-for="c in cards" :key="c.label" class="stat-card" :style="{ background: c.bg }">
        <div class="stat-icon" :style="{ background: c.iconBg }">
          <el-icon :size="22" color="#fff"><component :is="c.icon" /></el-icon>
        </div>
        <div class="stat-info">
          <div class="stat-value">{{ c.value }}</div>
          <div class="stat-label">{{ c.label }}</div>
        </div>
      </div>
    </div>

    <!-- 常用功能 -->
    <div class="section-card">
      <div class="section-header">
        <span class="section-title">常用功能</span>
      </div>
      <div class="shortcuts">
        <div v-for="s in shortcuts" :key="s.path" class="shortcut-item" @click="router.push(s.path)">
          <div class="shortcut-icon" :style="{ background: s.color }">
            <el-icon :size="20" color="#fff"><component :is="s.icon" /></el-icon>
          </div>
          <span class="shortcut-label">{{ s.label }}</span>
        </div>
      </div>
    </div>

    <!-- 图表区域 -->
    <div class="chart-row">
      <div class="section-card chart-card">
        <div class="section-header">
          <span class="section-title">销售/采购月度趋势</span>
        </div>
        <EChart :option="salesOption" height="300px" />
      </div>
      <div class="section-card chart-card">
        <div class="section-header">
          <span class="section-title">生产产量/废品趋势</span>
        </div>
        <EChart :option="prodOption" height="300px" />
      </div>
    </div>

    <div class="chart-row triple">
      <div class="section-card">
        <div class="section-header">
          <span class="section-title">库存结构</span>
        </div>
        <EChart :option="invOption" height="280px" />
      </div>
      <div class="section-card">
        <div class="section-header">
          <span class="section-title">工序产量分布</span>
        </div>
        <EChart :option="processOption" height="280px" />
      </div>
      <div class="section-card">
        <div class="section-header">
          <span class="section-title">生产质量趋势</span>
        </div>
        <EChart :option="qualityOption" height="280px" />
      </div>
    </div>

    <div class="chart-row">
      <div class="section-card chart-card wide">
        <div class="section-header">
          <span class="section-title">生产进度跟踪</span>
        </div>
        <EChart :option="progressOption" height="300px" />
      </div>
      <div class="section-card chart-card narrow">
        <div class="section-header">
          <span class="section-title">最近动态</span>
        </div>
        <div class="activity-list">
          <div v-for="(a, i) in activities" :key="i" class="activity-item">
            <div class="activity-dot" :style="{ background: activityColors[i % activityColors.length] }"></div>
            <div class="activity-content">
              <div class="activity-title">{{ a.title }}</div>
              <div class="activity-desc">{{ a.desc }}</div>
              <div class="activity-time">{{ formatTime(a.time) }}</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { Odometer, Box, Warning, SetUp, ShoppingCart, Goods, Van, OfficeBuilding, UserFilled, Tickets, DataAnalysis, Setting } from '@element-plus/icons-vue'
import EChart from '../components/EChart.vue'
import api from '../api/modules'

const router = useRouter()
const auth = useAuthStore()

const overview = ref({})
const invSummary = ref({})
const progress = ref([])
const quality = ref({ summary: {}, trend: [], reasons: [] })
const salesTrend = ref({ months: [], sales: [], purchase: [] })
const prodTrend = ref({ months: [], qty: [], scrap: [] })
const processDist = ref([])
const activities = ref([])

const todayStr = computed(() => {
  const d = new Date()
  const weekdays = ['日', '一', '二', '三', '四', '五', '六']
  return `${d.getFullYear()}年${d.getMonth() + 1}月${d.getDate()}日 星期${weekdays[d.getDay()]}`
})

const greeting = computed(() => {
  const h = new Date().getHours()
  if (h < 6) return '夜深了，注意休息'
  if (h < 12) return '上午好'
  if (h < 14) return '中午好'
  if (h < 18) return '下午好'
  return '晚上好'
})

const cards = computed(() => [
  { label: '本月产量', value: overview.value.monthOutput ?? 0, icon: SetUp, bg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', iconBg: 'rgba(255,255,255,0.2)' },
  { label: '本月销售金额', value: '¥' + Number(overview.value.monthSales ?? 0).toLocaleString(), icon: Odometer, bg: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)', iconBg: 'rgba(255,255,255,0.2)' },
  { label: '库存预警', value: overview.value.warningCount ?? 0, icon: Warning, bg: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)', iconBg: 'rgba(255,255,255,0.2)' },
  { label: '在产计划', value: overview.value.inProduction ?? 0, icon: Box, bg: 'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)', iconBg: 'rgba(255,255,255,0.2)' }
])

const shortcuts = [
  { label: '销售订单', path: '/sales/orders', icon: ShoppingCart, color: '#667eea' },
  { label: '采购订单', path: '/purchase/orders', icon: Goods, color: '#f5576c' },
  { label: '生产计划', path: '/production/plans', icon: DataAnalysis, color: '#43e97b' },
  { label: '发货管理', path: '/sales/deliveries', icon: Van, color: '#4facfe' },
  { label: '库存查询', path: '/warehouse/inventory', icon: OfficeBuilding, color: '#fa709a' },
  { label: '质检记录', path: '/quality/inspections', icon: Tickets, color: '#a18cd1' },
  { label: '员工管理', path: '/base/employees', icon: UserFilled, color: '#fccb90' },
  { label: '系统设置', path: '/system/users', icon: Setting, color: '#868f96' }
]

const activityColors = ['#667eea', '#f5576c', '#43e97b', '#4facfe', '#fa709a', '#a18cd1', '#fccb90']

const salesOption = computed(() => ({
  tooltip: { trigger: 'axis', backgroundColor: 'rgba(255,255,255,0.95)', borderColor: '#eee', textStyle: { color: '#333' } },
  legend: { data: ['销售金额', '采购金额'], top: 0, itemGap: 20, textStyle: { color: '#666' } },
  grid: { left: 68, right: 20, top: 40, bottom: 30 },
  xAxis: { type: 'category', data: salesTrend.value.months, axisLine: { lineStyle: { color: '#e0e0e0' } }, axisLabel: { color: '#888' } },
  yAxis: { type: 'value', axisLine: { show: false }, splitLine: { lineStyle: { type: 'dashed', color: '#f0f0f0' } }, axisLabel: { color: '#888' } },
  series: [
    { name: '销售金额', type: 'bar', data: salesTrend.value.sales, barWidth: 18, itemStyle: { color: { type: 'linear', x: 0, y: 0, x2: 0, y2: 1, colorStops: [{ offset: 0, color: '#667eea' }, { offset: 1, color: '#764ba2' }] }, borderRadius: [4, 4, 0, 0] } },
    { name: '采购金额', type: 'line', smooth: true, data: salesTrend.value.purchase, symbol: 'circle', symbolSize: 6, itemStyle: { color: '#f5576c' }, lineStyle: { width: 2.5 } }
  ]
}))

const prodOption = computed(() => ({
  tooltip: { trigger: 'axis', backgroundColor: 'rgba(255,255,255,0.95)', borderColor: '#eee', textStyle: { color: '#333' } },
  legend: { data: ['产量', '废品'], top: 0, itemGap: 20, textStyle: { color: '#666' } },
  grid: { left: 68, right: 20, top: 40, bottom: 30 },
  xAxis: { type: 'category', data: prodTrend.value.months, axisLine: { lineStyle: { color: '#e0e0e0' } }, axisLabel: { color: '#888' } },
  yAxis: { type: 'value', axisLine: { show: false }, splitLine: { lineStyle: { type: 'dashed', color: '#f0f0f0' } }, axisLabel: { color: '#888' } },
  series: [
    { name: '产量', type: 'bar', data: prodTrend.value.qty, barWidth: 18, itemStyle: { color: { type: 'linear', x: 0, y: 0, x2: 0, y2: 1, colorStops: [{ offset: 0, color: '#43e97b' }, { offset: 1, color: '#38f9d7' }] }, borderRadius: [4, 4, 0, 0] } },
    { name: '废品', type: 'line', smooth: true, data: prodTrend.value.scrap, symbol: 'circle', symbolSize: 6, itemStyle: { color: '#f5576c' }, lineStyle: { width: 2.5 } }
  ]
}))

const invOption = computed(() => {
  const pieData = [
    { name: '正常', value: invSummary.value.normal ?? 0, itemStyle: { color: '#43e97b' } },
    { name: '预警', value: invSummary.value.warning ?? 0, itemStyle: { color: '#fccb90' } },
    { name: '缺货', value: invSummary.value.outOfStock ?? 0, itemStyle: { color: '#f5576c' } }
  ].filter(x => x.value > 0)
  return {
    tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)', backgroundColor: 'rgba(255,255,255,0.95)', borderColor: '#eee', textStyle: { color: '#333' } },
    legend: { bottom: 0, textStyle: { color: '#666' } },
    series: [{ type: 'pie', radius: ['42%', '70%'], center: ['50%', '45%'], data: pieData, label: { formatter: '{b}\n{c}', color: '#555' }, emphasis: { itemStyle: { shadowBlur: 10, shadowColor: 'rgba(0,0,0,0.15)' } } }]
  }
})

const processOption = computed(() => ({
  tooltip: { trigger: 'item', backgroundColor: 'rgba(255,255,255,0.95)', borderColor: '#eee', textStyle: { color: '#333' } },
  legend: { bottom: 0, textStyle: { color: '#666' } },
  series: [{ type: 'pie', radius: ['42%', '70%'], center: ['50%', '45%'], data: processDist.value, label: { formatter: '{b}\n{c}', overflow: 'break', width: 100, color: '#555' }, emphasis: { itemStyle: { shadowBlur: 10, shadowColor: 'rgba(0,0,0,0.15)' } } }]
}))

const qualityOption = computed(() => {
  const trend = quality.value.trend || []
  return {
    tooltip: { trigger: 'axis', backgroundColor: 'rgba(255,255,255,0.95)', borderColor: '#eee', textStyle: { color: '#333' } },
    grid: { left: 68, right: 50, top: 40, bottom: 30 },
    xAxis: { type: 'category', data: trend.map(t => t.date), axisLine: { lineStyle: { color: '#e0e0e0' } }, axisLabel: { color: '#888' } },
    yAxis: [
      { type: 'value', name: '数量', axisLine: { show: false }, splitLine: { lineStyle: { type: 'dashed', color: '#f0f0f0' } }, axisLabel: { color: '#888' } },
      { type: 'value', name: '合格率', max: 100, min: 0, axisLine: { show: false }, splitLine: { show: false }, axisLabel: { color: '#888' } }
    ],
    series: [
      { name: '检验数量', type: 'bar', data: trend.map(t => t.inspectQty), barWidth: 14, itemStyle: { color: '#667eea', borderRadius: [3, 3, 0, 0] } },
      { name: '缺陷数量', type: 'bar', data: trend.map(t => t.defectQty), barWidth: 14, itemStyle: { color: '#f5576c', borderRadius: [3, 3, 0, 0] } },
      { name: '合格率%', type: 'line', smooth: true, yAxisIndex: 1, data: trend.map(t => t.passRate), symbol: 'circle', symbolSize: 6, itemStyle: { color: '#43e97b' }, lineStyle: { width: 2.5 } }
    ]
  }
})

const progressOption = computed(() => {
  const labels = progress.value.map(p => p.planNo + ' ' + p.projectName)
  const maxLen = labels.reduce((m, s) => Math.max(m, s.length), 0)
  return {
    tooltip: { trigger: 'axis', formatter: p => `${p[0].name}<br/>${p[0].marker}进度: ${p[0].value}%`, backgroundColor: 'rgba(255,255,255,0.95)', borderColor: '#eee', textStyle: { color: '#333' } },
    grid: { left: maxLen * 14 + 16, right: 50, top: 20, bottom: 30 },
    xAxis: { type: 'value', max: 100, axisLabel: { formatter: '{value}%', color: '#888' }, axisLine: { show: false }, splitLine: { lineStyle: { type: 'dashed', color: '#f0f0f0' } } },
    yAxis: { type: 'category', data: labels, axisLine: { lineStyle: { color: '#e0e0e0' } }, axisLabel: { color: '#555' } },
    series: [{
      type: 'bar', data: progress.value.map(p => p.progress), barWidth: 16,
      itemStyle: { borderRadius: [0, 4, 4, 0], color: p => {
        if (p.value >= 100) return { type: 'linear', x: 0, y: 0, x2: 1, y2: 0, colorStops: [{ offset: 0, color: '#43e97b' }, { offset: 1, color: '#38f9d7' }] }
        if (p.value >= 50) return { type: 'linear', x: 0, y: 0, x2: 1, y2: 0, colorStops: [{ offset: 0, color: '#667eea' }, { offset: 1, color: '#764ba2' }] }
        return { type: 'linear', x: 0, y: 0, x2: 1, y2: 0, colorStops: [{ offset: 0, color: '#fccb90' }, { offset: 1, color: '#f5576c' }] }
      }},
      label: { show: true, position: 'right', formatter: '{c}%', color: '#555' }
    }]
  }
})

function formatTime(t) {
  if (!t) return ''
  const d = new Date(t)
  const now = new Date()
  const diff = now - d
  if (diff < 3600000) return Math.floor(diff / 60000) + '分钟前'
  if (diff < 86400000) return Math.floor(diff / 3600000) + '小时前'
  return `${d.getMonth() + 1}月${d.getDate()}日 ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
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
.dashboard { padding: 0; }

.welcome-banner {
  background: linear-gradient(135deg, #1f3b73 0%, #3a5ba0 50%, #5b7fd4 100%);
  border-radius: 12px;
  padding: 28px 32px;
  margin-bottom: 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  color: #fff;
  box-shadow: 0 4px 20px rgba(31,59,115,0.3);
}
.welcome-banner h2 { font-size: 22px; font-weight: 600; margin: 0 0 6px 0; letter-spacing: 0.5px; }
.welcome-banner p { font-size: 13px; opacity: 0.8; margin: 0; }
.weather-info { text-align: right; }
.greeting { font-size: 16px; opacity: 0.9; }

.stat-cards {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  margin-bottom: 20px;
}
.stat-card {
  border-radius: 12px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 16px;
  color: #fff;
  box-shadow: 0 4px 16px rgba(0,0,0,0.1);
  transition: transform 0.2s, box-shadow 0.2s;
  cursor: default;
}
.stat-card:hover { transform: translateY(-2px); box-shadow: 0 8px 24px rgba(0,0,0,0.15); }
.stat-icon {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.stat-value { font-size: 24px; font-weight: 700; line-height: 1.2; }
.stat-label { font-size: 13px; opacity: 0.85; margin-top: 4px; }

.section-card {
  background: #fff;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 12px rgba(0,0,0,0.04);
  margin-bottom: 20px;
}
.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}
.section-title {
  font-size: 15px;
  font-weight: 600;
  color: #1f3b73;
  position: relative;
  padding-left: 12px;
}
.section-title::before {
  content: '';
  position: absolute;
  left: 0;
  top: 2px;
  bottom: 2px;
  width: 3px;
  border-radius: 2px;
  background: linear-gradient(180deg, #667eea, #764ba2);
}

.shortcuts {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
  gap: 12px;
}
.shortcut-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 16px 8px;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.2s;
}
.shortcut-item:hover { background: #f5f7fa; transform: translateY(-2px); }
.shortcut-icon {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}
.shortcut-label { font-size: 13px; color: #555; white-space: nowrap; }

.chart-row { display: flex; gap: 16px; margin-bottom: 20px; }
.chart-row .section-card { flex: 1; margin-bottom: 0; }
.chart-row.triple .section-card { flex: 1; }
.chart-card.wide { flex: 1.4; }
.chart-card.narrow { flex: 1; }

.activity-list {
  max-height: 300px;
  overflow-y: auto;
  padding-right: 4px;
}
.activity-list::-webkit-scrollbar { width: 4px; }
.activity-list::-webkit-scrollbar-thumb { background: #ddd; border-radius: 2px; }
.activity-item {
  display: flex;
  gap: 12px;
  padding: 10px 0;
  border-bottom: 1px solid #f5f5f5;
}
.activity-item:last-child { border-bottom: none; }
.activity-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  margin-top: 6px;
  flex-shrink: 0;
}
.activity-content { flex: 1; min-width: 0; }
.activity-title { font-size: 13px; font-weight: 500; color: #333; }
.activity-desc { font-size: 12px; color: #999; margin-top: 2px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.activity-time { font-size: 11px; color: #bbb; margin-top: 4px; }
</style>
