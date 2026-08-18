<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>排班计划</span>
        <el-button type="primary" size="small" @click="openCreate">新增排班</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="车间">
        <el-input v-model="query.workshop" clearable style="width:130px" @keyup.enter="load" />
      </el-form-item>
      <el-form-item label="日期">
        <el-date-picker v-model="query.workDate" type="date" value-format="YYYY-MM-DD" style="width:150px" @change="load" />
      </el-form-item>
      <el-form-item><el-button type="primary" @click="load">查询</el-button></el-form-item>
    </el-form>

    <el-table :data="rows" border stripe v-loading="loading">
      <el-table-column prop="workDate" label="日期" width="110" align="center">
        <template #default="{ row }">{{ fmt(row.workDate) }}</template>
      </el-table-column>
      <el-table-column prop="workshop" label="车间" width="110" align="center" />
      <el-table-column prop="employeeName" label="员工" min-width="100" align="center" />
      <el-table-column prop="shiftName" label="班次" width="110" align="center" />
      <el-table-column prop="task" label="工作任务" min-width="220" align="center" class="allow-wrap" />
      <el-table-column prop="remark" label="备注" min-width="120" align="center" class="allow-wrap" />
      <el-table-column label="操作" width="150" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑排班' : '新增排班'" width="520px" destroy-on-close>
      <el-form :model="form" label-width="90px">
        <el-form-item label="日期" required>
          <el-date-picker v-model="form.workDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
        </el-form-item>
        <el-form-item label="员工" required>
          <el-select v-model="form.employeeId" filterable style="width:100%">
            <el-option v-for="e in employees" :key="e.id" :label="e.name + ' (' + (e.position||'') + ')'" :value="e.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="班次" required>
          <el-select v-model="form.shiftId" style="width:100%">
            <el-option v-for="s in shifts" :key="s.id" :label="s.name" :value="s.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="车间"><el-input v-model="form.workshop" /></el-form-item>
        <el-form-item label="工作任务"><el-input v-model="form.task" /></el-form-item>
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

const rows = ref([])
const employees = ref([])
const shifts = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref(false)
const query = reactive({ workshop: '', workDate: '' })
const form = reactive({})

async function load() {
  loading.value = true
  try {
    rows.value = await api.workSchedules({
      keyword: query.workshop,
      start: query.workDate || '', end: query.workDate || ''
    })
  } finally { loading.value = false }
}
function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { workDate: new Date().toISOString().slice(0, 10), employeeId: null, shiftId: null, workshop: '', task: '', remark: '' })
  dialogVisible.value = true
}
function openEdit(row) {
  editing.value = true
  Object.assign(form, { ...row })
  dialogVisible.value = true
}
async function save() {
  if (!form.workDate) return ElMessage.warning('请选择日期')
  if (!form.employeeId) return ElMessage.warning('请选择员工')
  if (!form.shiftId) return ElMessage.warning('请选择班次')
  if (editing.value) { await api.updateWorkSchedule(form.id, form); ElMessage.success('更新成功') }
  else { await api.createWorkSchedule(form); ElMessage.success('新增成功') }
  dialogVisible.value = false
  load()
}
async function remove(row) {
  await ElMessageBox.confirm('确定删除该排班？', '提示', { type: 'warning' })
  await api.deleteWorkSchedule(row.id)
  ElMessage.success('删除成功')
  load()
}
function fmt(v) { return v ? String(v).slice(0, 10) : '-' }
onMounted(async () => {
  load()
  employees.value = await api.employees()
  shifts.value = await api.shifts()
})
</script>
