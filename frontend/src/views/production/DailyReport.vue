<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>生产日报表</span>
        <el-button type="primary" size="small" @click="openCreate">新增日报</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="制号"><el-input v-model="query.keyword" clearable style="width:140px" @keyup.enter="load" /></el-form-item>
      <el-form-item label="日期">
        <el-date-picker v-model="query.range" type="daterange" value-format="YYYY-MM-DD" style="width:240px" @change="load" />
      </el-form-item>
      <el-form-item><el-button type="primary" @click="load">查询</el-button></el-form-item>
    </el-form>

    <el-table :data="rows" border stripe v-loading="loading">
      <el-table-column prop="reportDate" label="日期" width="100" align="center">
        <template #default="{ row }">{{ fmt(row.reportDate) }}</template>
      </el-table-column>
      <el-table-column prop="planNo" label="制号" width="90" align="center" />
      <el-table-column prop="materialSpec" label="材质" width="90" align="center" />
      <el-table-column prop="sizeSpec" label="尺寸" width="110" align="center" />
      <el-table-column prop="prevCarryQty" label="上期结转" width="90" align="center" />
      <el-table-column prop="materialQty" label="领用" width="80" align="center" />
      <el-table-column prop="inStockQty" label="入库" width="80" align="center" />
      <el-table-column prop="shipQty" label="发货" width="80" align="center" />
      <el-table-column prop="totalChengpin" label="合格品" width="90" align="center" />
      <el-table-column prop="totalFeipin" label="废品" width="80" align="center" />
      <el-table-column prop="totalGongshi" label="工时" width="80" align="center" />
      <el-table-column prop="batchNo" label="批号" width="90" align="center" />
      <el-table-column label="操作" width="140" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="openDetail(row)">查看</el-button>
          <el-button link type="primary" size="small" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑日报' : '新增日报'" width="820px" destroy-on-close>
      <el-form :model="form" label-width="90px">
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="日期">
              <el-date-picker v-model="form.reportDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="制号" required>
              <el-input v-model="form.planNo" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="批号"><el-input v-model="form.batchNo" /></el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="材质"><el-input v-model="form.materialSpec" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="尺寸"><el-input v-model="form.sizeSpec" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="台份"><el-input-number v-model="form.taiFen" :min="0" style="width:100%" /></el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="6">
            <el-form-item label="上期结转"><el-input-number v-model="form.prevCarryQty" :min="0" style="width:100%" /></el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="领用"><el-input-number v-model="form.materialQty" :min="0" style="width:100%" /></el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="废料张数"><el-input-number v-model="form.scrapSheets" :min="0" style="width:100%" /></el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="入库数量"><el-input-number v-model="form.inStockQty" :min="0" style="width:100%" /></el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="发货数量"><el-input-number v-model="form.shipQty" :min="0" style="width:100%" /></el-form-item>
          </el-col>
          <el-col :span="16">
            <el-form-item label="备注"><el-input v-model="form.remark" /></el-form-item>
          </el-col>
        </el-row>
        <el-divider content-position="left">工序汇总</el-divider>
        <el-table :data="form.processes" border size="small">
          <el-table-column label="工序" width="140">
            <template #default="{ row }">
              <el-select v-model="row.processName" allow-create filterable style="width:100%">
                <el-option v-for="p in processNames" :key="p" :label="p" :value="p" />
              </el-select>
            </template>
          </el-table-column>
          <el-table-column label="设备" width="130">
            <template #default="{ row }"><el-input v-model="row.equipmentNo" /></template>
          </el-table-column>
          <el-table-column label="合格数量" width="120">
            <template #default="{ row }"><el-input-number v-model="row.qualifiedQty" :min="0" style="width:100%" /></template>
          </el-table-column>
          <el-table-column label="废品数量" width="120">
            <template #default="{ row }"><el-input-number v-model="row.scrapQty" :min="0" style="width:100%" /></template>
          </el-table-column>
          <el-table-column label="工时" width="120">
            <template #default="{ row }"><el-input-number v-model="row.workHours" :min="0" :precision="2" style="width:100%" /></template>
          </el-table-column>
          <el-table-column label="" width="70" align="center">
            <template #default="{ $index }"><el-button link type="danger" size="small" @click="form.processes.splice($index, 1)">删除</el-button></template>
          </el-table-column>
        </el-table>
        <el-button size="small" style="margin-top:8px" @click="form.processes.push({ processName: '', equipmentNo: '', qualifiedQty: 0, scrapQty: 0, workHours: 0 })">+ 添加工序</el-button>
        <el-divider content-position="left">总计（自动计算）</el-divider>
        <el-table :data="[{ _t: '合计' }]" border size="small" :show-header="false">
          <el-table-column label="" width="140">
            <template #default>合计</template>
          </el-table-column>
          <el-table-column label="" width="130">
            <template #default>—</template>
          </el-table-column>
          <el-table-column label="合格" width="120">
            <template #default>{{ totalQualified }}</template>
          </el-table-column>
          <el-table-column label="废品" width="120">
            <template #default>{{ totalScrap }}</template>
          </el-table-column>
          <el-table-column label="工时">
            <template #default>{{ totalHours }}</template>
          </el-table-column>
        </el-table>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>

    <el-dialog v-model="detailVisible" title="日报详情" width="800px">
      <el-descriptions :column="4" border size="small">
        <el-descriptions-item label="日期">{{ fmt(detail.reportDate) }}</el-descriptions-item>
        <el-descriptions-item label="制号">{{ detail.planNo }}</el-descriptions-item>
        <el-descriptions-item label="批号">{{ detail.batchNo || '-' }}</el-descriptions-item>
        <el-descriptions-item label="台份">{{ detail.taiFen }}</el-descriptions-item>
        <el-descriptions-item label="材质">{{ detail.materialSpec || '-' }}</el-descriptions-item>
        <el-descriptions-item label="尺寸">{{ detail.sizeSpec || '-' }}</el-descriptions-item>
        <el-descriptions-item label="领用">{{ detail.materialQty }}</el-descriptions-item>
        <el-descriptions-item label="入库">{{ detail.inStockQty }}</el-descriptions-item>
      </el-descriptions>
      <el-table :data="detail.processes || []" border size="small" style="margin-top:12px">
        <el-table-column prop="processName" label="工序" width="120" align="center" />
        <el-table-column prop="equipmentNo" label="设备" width="120" align="center" />
        <el-table-column prop="qualifiedQty" label="合格数量" align="center" />
        <el-table-column prop="scrapQty" label="废品数量" align="center" />
        <el-table-column prop="workHours" label="工时" align="center" />
      </el-table>
    </el-dialog>
  </el-card>
</template>

<script setup>
import { reactive, ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '../../api/modules'

const processNames = ['落料', '拉延', '修边', '冲孔', '侧冲']
const rows = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const detailVisible = ref(false)
const editing = ref(false)
const query = reactive({ keyword: '', range: null })
const form = reactive({})
const detail = ref({})

const totalQualified = computed(() => (form.processes || []).reduce((s, p) => s + (p.qualifiedQty || 0), 0))
const totalScrap = computed(() => (form.processes || []).reduce((s, p) => s + (p.scrapQty || 0), 0))
const totalHours = computed(() => (form.processes || []).reduce((s, p) => s + (p.workHours || 0), 0))

async function load() {
  loading.value = true
  try {
    rows.value = await api.dailyReports({
      keyword: query.keyword,
      start: query.range?.[0] || '', end: query.range?.[1] || ''
    })
  } finally { loading.value = false }
}
function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { reportDate: new Date().toISOString().slice(0, 10), planNo: '', prevCarryQty: 0, materialQty: 0, batchNo: '', scrapSheets: 0, inStockQty: 0, shipQty: 0, sizeSpec: '', materialSpec: '', taiFen: 0, remark: '', processes: [] })
  dialogVisible.value = true
}
function openEdit(row) {
  editing.value = true
  Object.assign(form, { ...row, reportDate: String(row.reportDate).slice(0, 10) })
  dialogVisible.value = true
}
async function openDetail(row) {
  detail.value = await api.dailyReports().then(list => list.find(x => x.id === row.id))
  detailVisible.value = true
}
async function save() {
  if (!form.planNo) return ElMessage.warning('请输入制号')
  form.totalChengpin = totalQualified.value
  form.totalFeipin = totalScrap.value
  form.totalGongshi = totalHours.value
  form.totalLingliao = form.materialQty || 0
  form.totalFeiliao = form.scrapSheets || 0
  if (editing.value) { await api.updateDailyReport(form.id, form); ElMessage.success('更新成功') }
  else { await api.createDailyReport(form); ElMessage.success('新增成功') }
  dialogVisible.value = false
  load()
}
async function remove(row) {
  await ElMessageBox.confirm('确定删除该日报？', '提示', { type: 'warning' })
  await api.deleteDailyReport(row.id)
  ElMessage.success('删除成功')
  load()
}
function fmt(v) { return v ? String(v).slice(0, 10) : '-' }
onMounted(load)
</script>
