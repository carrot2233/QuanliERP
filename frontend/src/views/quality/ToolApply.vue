<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>量具申购单</span>
        <el-button type="primary" size="small" @click="openCreate">新增申购单</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="关键词"><el-input v-model="query.keyword" clearable style="width:180px" @keyup.enter="load" /></el-form-item>
      <el-form-item label="审核状态">
        <el-select v-model="query.auditStatus" clearable style="width:120px">
          <el-option v-for="s in auditStatuses" :key="s" :label="s" :value="s" />
        </el-select>
      </el-form-item>
      <el-form-item><el-button type="primary" @click="load">查询</el-button></el-form-item>
    </el-form>

    <el-table :data="rows" border stripe v-loading="loading">
      <el-table-column prop="applyNo" label="申购单号" width="130" align="center" />
      <el-table-column prop="name" label="名称" min-width="150" align="center" />
      <el-table-column prop="specification" label="规格" min-width="120" align="center" />
      <el-table-column prop="qty" label="数量" width="80" align="center" />
      <el-table-column prop="dept" label="部门" width="100" align="center" />
      <el-table-column prop="reason" label="申购原因" min-width="150" align="center" class="allow-wrap" />
      <el-table-column prop="applyDate" label="申购日期" width="110" align="center">
        <template #default="{ row }">{{ fmt(row.applyDate) }}</template>
      </el-table-column>
      <el-table-column prop="arrivalDate" label="到货日期" width="110" align="center">
        <template #default="{ row }">{{ fmt(row.arrivalDate) }}</template>
      </el-table-column>
      <el-table-column prop="auditStatus" label="审核状态" width="100" align="center">
        <template #default="{ row }">
          <el-tag :type="{ 待审核: 'warning', 同意: 'success', 驳回: 'danger' }[row.auditStatus]" size="small">{{ row.auditStatus }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="180" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="openEdit(row)">编辑</el-button>
          <el-dropdown trigger="click" style="margin:0 4px">
            <el-button link type="warning" size="small">审核</el-button>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item v-for="s in auditStatuses" :key="s" @click="audit(row, s)">{{ s }}</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑申购单' : '新增申购单'" width="560px" destroy-on-close>
      <el-form :model="form" label-width="90px">
        <el-form-item label="申购单号"><el-input v-model="form.applyNo" placeholder="留空自动生成" /></el-form-item>
        <el-form-item label="名称" required><el-input v-model="form.name" /></el-form-item>
        <el-form-item label="规格"><el-input v-model="form.specification" /></el-form-item>
        <el-form-item label="数量"><el-input-number v-model="form.qty" :min="1" style="width:100%" /></el-form-item>
        <el-form-item label="部门"><el-input v-model="form.dept" /></el-form-item>
        <el-form-item label="申购原因"><el-input v-model="form.reason" /></el-form-item>
        <el-form-item label="申购日期">
          <el-date-picker v-model="form.applyDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
        </el-form-item>
        <el-form-item label="到货日期">
          <el-date-picker v-model="form.arrivalDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
        </el-form-item>
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

const auditStatuses = ['待审核', '同意', '驳回']
const rows = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref(false)
const query = reactive({ keyword: '', auditStatus: '' })
const form = reactive({})

async function load() {
  loading.value = true
  try { rows.value = await api.toolApplies(query) } finally { loading.value = false }
}
function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { applyNo: '', name: '', specification: '', qty: 1, reason: '', dept: '', applyDate: new Date().toISOString().slice(0, 10), arrivalDate: null, auditStatus: '待审核', remark: '' })
  dialogVisible.value = true
}
function openEdit(row) {
  editing.value = true
  Object.assign(form, { ...row, applyDate: String(row.applyDate).slice(0, 10) })
  dialogVisible.value = true
}
async function save() {
  if (!form.name) return ElMessage.warning('请输入名称')
  const data = { ...form }
  if (data.arrivalDate === '' || data.arrivalDate === undefined) data.arrivalDate = null
  if (editing.value) { await api.updateToolApply(form.id, data); ElMessage.success('更新成功') }
  else { await api.createToolApply(data); ElMessage.success('新增成功') }
  dialogVisible.value = false
  load()
}
async function audit(row, s) {
  await api.updateToolApply(row.id, { ...row, auditStatus: s })
  ElMessage.success('审核状态已更新')
  load()
}
async function remove(row) {
  await ElMessageBox.confirm('确定删除该申购单？', '提示', { type: 'warning' })
  await api.deleteToolApply(row.id)
  ElMessage.success('删除成功')
  load()
}
function fmt(v) { return v ? String(v).slice(0, 10) : '-' }
onMounted(load)
</script>
