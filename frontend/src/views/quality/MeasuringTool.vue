<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>计量器具台账</span>
        <el-button type="primary" size="small" @click="openCreate">新增器具</el-button>
      </div>
    </template>

    <el-alert v-if="overdueList.length" :closable="false" type="error" style="margin-bottom:12px">
      <b>校准超期/临近器具 {{ overdueList.length }} 项：</b>
      <el-tag v-for="t in overdueList" :key="t.id" size="small" type="danger" style="margin:0 4px">{{ t.toolNo }} {{ t.name }}</el-tag>
    </el-alert>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="关键词"><el-input v-model="query.keyword" clearable style="width:200px" @keyup.enter="load" /></el-form-item>
      <el-form-item label="状态">
        <el-select v-model="query.status" clearable style="width:110px">
          <el-option v-for="s in statuses" :key="s" :label="s" :value="s" />
        </el-select>
      </el-form-item>
      <el-form-item><el-button type="primary" @click="load">查询</el-button></el-form-item>
    </el-form>

    <el-table :data="rows" border stripe v-loading="loading">
      <el-table-column prop="toolNo" label="器具编号" width="110" align="center" />
      <el-table-column prop="name" label="名称" min-width="140" align="center" />
      <el-table-column prop="specification" label="规格" min-width="110" align="center" />
      <el-table-column prop="qty" label="数量" width="70" align="center" />
      <el-table-column prop="status" label="状态" width="80" align="center">
        <template #default="{ row }">
          <el-tag :type="statusTag(row.status)" size="small">{{ row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="dept" label="使用部门" width="100" align="center" />
      <el-table-column prop="holder" label="保管人" width="90" align="center" />
      <el-table-column prop="calibrationCycle" label="检定周期" width="90" align="center" />
      <el-table-column prop="calibrationPlanDate" label="计划检定" width="110" align="center">
        <template #default="{ row }">{{ fmt(row.calibrationPlanDate) }}</template>
      </el-table-column>
      <el-table-column prop="calibrationDate" label="最近检定" width="110" align="center">
        <template #default="{ row }">{{ fmt(row.calibrationDate) }}</template>
      </el-table-column>
      <el-table-column label="校准提醒" width="90" align="center">
        <template #default="{ row }">
          <el-tag v-if="calOverdue(row)" type="danger" size="small">超期</el-tag>
          <el-tag v-else-if="calNear(row)" type="warning" size="small">即将到期</el-tag>
          <el-tag v-else type="success" size="small">正常</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="140" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑器具' : '新增器具'" width="700px" destroy-on-close>
      <el-form :model="form" label-width="100px">
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="器具编号" required><el-input v-model="form.toolNo" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="名称" required><el-input v-model="form.name" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="规格"><el-input v-model="form.specification" /></el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="数量"><el-input-number v-model="form.qty" :min="0" style="width:100%" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="状态">
              <el-select v-model="form.status" style="width:100%">
                <el-option v-for="s in statuses" :key="s" :label="s" :value="s" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="来源"><el-input v-model="form.origin" /></el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="购置日期">
              <el-date-picker v-model="form.purchaseDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="单价"><el-input-number v-model="form.unitPrice" :min="0" :precision="2" style="width:100%" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="使用部门"><el-input v-model="form.dept" /></el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="保管人"><el-input v-model="form.holder" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="领用日期">
              <el-date-picker v-model="form.receiveDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="检定周期"><el-input v-model="form.calibrationCycle" placeholder="如 1年/6个月" /></el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="计划检定">
              <el-date-picker v-model="form.calibrationPlanDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="最近检定">
              <el-date-picker v-model="form.calibrationDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="停用日期">
              <el-date-picker v-model="form.stopDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="备注"><el-input v-model="form.remark" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>
  </el-card>
</template>

<script setup>
import { reactive, ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '../../api/modules'

const statuses = ['在用', '待检', '封存', '停用', '报废']
const rows = ref([])
const overdueList = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref(false)
const query = reactive({ keyword: '', status: '' })
const form = reactive({})

async function load() {
  loading.value = true
  try {
    rows.value = await api.measuringTools(query)
    overdueList.value = await api.calibrationOverdue()
  } finally { loading.value = false }
}
function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { toolNo: '', name: '', specification: '', qty: 1, status: '在用', origin: '', purchaseDate: '', unitPrice: 0, dept: '', holder: '', receiveDate: '', calibrationCycle: '', calibrationPlanDate: '', calibrationDate: '', stopDate: '', remark: '' })
  dialogVisible.value = true
}
function openEdit(row) {
  editing.value = true
  Object.assign(form, { ...row })
  dialogVisible.value = true
}
async function save() {
  if (!form.toolNo || !form.name) return ElMessage.warning('请填写编号与名称')
  if (editing.value) { await api.updateMeasuringTool(form.id, form); ElMessage.success('更新成功') }
  else { await api.createMeasuringTool(form); ElMessage.success('新增成功') }
  dialogVisible.value = false
  load()
}
async function remove(row) {
  await ElMessageBox.confirm('确定删除该器具？', '提示', { type: 'warning' })
  await api.deleteMeasuringTool(row.id)
  ElMessage.success('删除成功')
  load()
}
function statusTag(s) { return { 在用: 'success', 待检: 'warning', 封存: 'info', 停用: 'danger', 报废: 'danger' }[s] || 'info' }
function calOverdue(row) { return row.calibrationPlanDate && new Date(row.calibrationPlanDate) < new Date().setHours(0, 0, 0, 0) }
function calNear(row) { return row.calibrationPlanDate && !calOverdue(row) && new Date(row.calibrationPlanDate) - new Date() < 30 * 86400000 }
function fmt(v) { return v ? String(v).slice(0, 10) : '-' }
onMounted(load)
</script>
