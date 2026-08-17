<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>模具制造生产总计划</span>
        <el-button type="primary" size="small" @click="openCreate">新增制造计划</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="关键词"><el-input v-model="query.keyword" clearable style="width:200px" @keyup.enter="load" /></el-form-item>
      <el-form-item><el-button type="primary" @click="load">查询</el-button></el-form-item>
    </el-form>

    <el-table :data="rows" border stripe v-loading="loading">
      <el-table-column prop="planNo" label="计划号" width="150" align="center" />
      <el-table-column prop="projectName" label="项目" min-width="130" align="center" />
      <el-table-column prop="customerName" label="客户" min-width="120" align="center" />
      <el-table-column prop="moldNo" label="模具编号" width="100" align="center" />
      <el-table-column prop="moldName" label="模具名称" min-width="130" align="center" />
      <el-table-column prop="processName" label="工艺" width="90" align="center" />
      <el-table-column prop="tonnage" label="吨位" width="80" align="center">
        <template #default="{ row }">{{ row.tonnage ? row.tonnage + 'T' : '-' }}</template>
      </el-table-column>
      <el-table-column label="进度" width="150" align="center">
        <template #default="{ row }">
          <el-progress :percentage="Math.round(row.progress)" :stroke-width="12" />
          <span style="font-size:12px">{{ row.doneStages }}/{{ row.totalStages }}</span>
        </template>
      </el-table-column>
      <el-table-column prop="moldStatus" label="状态" width="100" align="center">
        <template #default="{ row }">
          <el-tag :type="statusTag(row.moldStatus)" size="small">{{ row.moldStatus }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="计划/实际到货" width="200" align="center">
        <template #default="{ row }">
          {{ fmt(row.planArrival) }} / {{ fmt(row.actualArrival) }}
        </template>
      </el-table-column>
      <el-table-column label="操作" width="160" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="openDetail(row)">阶段</el-button>
          <el-button link type="primary" size="small" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑制造计划' : '新增制造计划'" width="760px" destroy-on-close>
      <el-form :model="form" label-width="100px">
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="计划号"><el-input v-model="form.planNo" placeholder="留空自动生成" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="项目名称"><el-input v-model="form.projectName" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="客户">
              <el-select v-model="form.customerId" filterable clearable style="width:100%">
                <el-option v-for="c in customers" :key="c.id" :label="c.name" :value="c.id" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="模具编号"><el-input v-model="form.moldNo" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="模具名称"><el-input v-model="form.moldName" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="工艺">
              <el-select v-model="form.processName" allow-create filterable clearable style="width:100%">
                <el-option v-for="t in processTypes" :key="t" :label="t" :value="t" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="吨位"><el-input-number v-model="form.tonnage" :min="0" style="width:100%" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="状态">
              <el-select v-model="form.moldStatus" style="width:100%">
                <el-option v-for="s in moldStatuses" :key="s" :label="s" :value="s" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="计划到货">
              <el-date-picker v-model="form.planArrival" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="实际到货">
              <el-date-picker v-model="form.actualArrival" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="16">
            <el-form-item label="备注"><el-input v-model="form.remark" /></el-form-item>
          </el-col>
        </el-row>
        <el-divider content-position="left">制造阶段（编程2D → 编程3D → 2D加工 → 淬火 → 投线 → 线切割 → 机钳装配 → 3D精加工 → 合模装配 → 研合 → 调试）</el-divider>
        <el-table :data="form.stages" border size="small">
          <el-table-column label="阶段" width="120">
            <template #default="{ row }">
              <el-select v-model="row.stageName" filterable allow-create style="width:100%">
                <el-option v-for="s in stageNames" :key="s" :label="s" :value="s" />
              </el-select>
            </template>
          </el-table-column>
          <el-table-column label="计划开始" width="130">
            <template #default="{ row }"><el-date-picker v-model="row.planStart" type="date" value-format="YYYY-MM-DD" style="width:100%" /></template>
          </el-table-column>
          <el-table-column label="计划结束" width="130">
            <template #default="{ row }"><el-date-picker v-model="row.planEnd" type="date" value-format="YYYY-MM-DD" style="width:100%" /></template>
          </el-table-column>
          <el-table-column label="实际开始" width="130">
            <template #default="{ row }"><el-date-picker v-model="row.actualStart" type="date" value-format="YYYY-MM-DD" style="width:100%" /></template>
          </el-table-column>
          <el-table-column label="实际结束" width="130">
            <template #default="{ row }"><el-date-picker v-model="row.actualEnd" type="date" value-format="YYYY-MM-DD" style="width:100%" /></template>
          </el-table-column>
          <el-table-column label="状态" width="100">
            <template #default="{ row }">
              <el-select v-model="row.status" style="width:100%">
                <el-option v-for="s in stageStatuses" :key="s" :label="s" :value="s" />
              </el-select>
            </template>
          </el-table-column>
          <el-table-column label="" width="60" align="center">
            <template #default="{ $index }"><el-button link type="danger" size="small" @click="form.stages.splice($index, 1)">删除</el-button></template>
          </el-table-column>
        </el-table>
        <el-button size="small" style="margin-top:8px" @click="form.stages.push({ stageName: '', planStart: '', planEnd: '', actualStart: '', actualEnd: '', status: '未开始', remark: '' })">+ 添加阶段</el-button>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>

    <el-dialog v-model="detailVisible" :title="detail.planNo + ' - ' + detail.moldName" width="980px">
      <el-descriptions :column="3" border size="small">
        <el-descriptions-item label="项目">{{ detail.projectName }}</el-descriptions-item>
        <el-descriptions-item label="客户">{{ detail.customerName }}</el-descriptions-item>
        <el-descriptions-item label="状态">{{ detail.moldStatus }}</el-descriptions-item>
        <el-descriptions-item label="模具">{{ detail.moldNo }}</el-descriptions-item>
        <el-descriptions-item label="工艺">{{ detail.processName }}</el-descriptions-item>
        <el-descriptions-item label="吨位">{{ detail.tonnage }}</el-descriptions-item>
      </el-descriptions>
      <el-table :data="detail.stages || []" border size="small" style="margin-top:12px">
        <el-table-column type="index" label="#" width="45" align="center" />
        <el-table-column prop="stageName" label="阶段" min-width="110" align="center" />
        <el-table-column label="计划起止" width="180" align="center">
          <template #default="{ row }">{{ fmt(row.planStart) }} ~ {{ fmt(row.planEnd) }}</template>
        </el-table-column>
        <el-table-column label="实际起止" width="180" align="center">
          <template #default="{ row }">{{ fmt(row.actualStart) }} ~ {{ fmt(row.actualEnd) }}</template>
        </el-table-column>
        <el-table-column prop="status" label="状态" width="90" align="center">
          <template #default="{ row }">
            <el-tag :type="{ 已完成: 'success', 进行中: 'primary', 未开始: 'info', 超期: 'danger' }[row.status]" size="small">{{ row.status }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="remark" label="备注" min-width="120" align="center" />
      </el-table>
    </el-dialog>
  </el-card>
</template>

<script setup>
import { reactive, ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '../../api/modules'

const stageNames = ['编程2D', '编程3D', '2D加工', '淬火计划', '投线', '线切割', '机钳装配', '3D精加工', '合模装配', '研合完成', '调试完成']
const stageStatuses = ['未开始', '进行中', '已完成', '超期']
const processTypes = ['冲压模', '拉伸模', '注塑模', '压铸模', '成型模']
const moldStatuses = ['排产中', '制造中', '试模', '已完成', '暂停']
const rows = ref([])
const customers = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const detailVisible = ref(false)
const editing = ref(false)
const query = reactive({ keyword: '' })
const form = reactive({})
const detail = ref({})

async function load() {
  loading.value = true
  try { rows.value = await api.moldPlans(query) } finally { loading.value = false }
}
function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { planNo: '', customerId: null, projectName: '', moldNo: '', moldName: '', processName: '', tonnage: 0, moldStatus: '排产中', planArrival: '', actualArrival: '', remark: '', stages: stageNames.map(n => ({ id: 0, stageName: n, planStart: '', planEnd: '', actualStart: '', actualEnd: '', status: '未开始', remark: '' })) })
  dialogVisible.value = true
}
async function openEdit(row) {
  editing.value = true
  const o = await api.moldPlans().then(list => list.find(x => x.id === row.id))
  Object.assign(form, { ...o })
  form.customerId = o.customerId
  dialogVisible.value = true
}
async function openDetail(row) {
  detail.value = await api.moldPlans().then(list => list.find(x => x.id === row.id))
  detailVisible.value = true
}
async function save() {
  if (editing.value) { await api.updateMoldPlan(form.id, form); ElMessage.success('更新成功') }
  else { await api.createMoldPlan(form); ElMessage.success('新增成功') }
  dialogVisible.value = false
  load()
}
async function remove(row) {
  await ElMessageBox.confirm('确定删除该制造计划？', '提示', { type: 'warning' })
  await api.deleteMoldPlan(row.id)
  ElMessage.success('删除成功')
  load()
}
function statusTag(s) { return { 排产中: 'info', 制造中: 'primary', 试模: 'warning', 已完成: 'success', 暂停: 'warning' }[s] || 'info' }
function fmt(v) { return v ? String(v).slice(0, 10) : '-' }
onMounted(async () => {
  load()
  customers.value = await api.customers()
})
</script>
