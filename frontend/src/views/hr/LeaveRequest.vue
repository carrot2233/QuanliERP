<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>请假管理</span>
        <el-button type="primary" size="small" @click="openCreate">新建</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="关键词">
        <el-input v-model="query.keyword" placeholder="请输入关键词" clearable style="width:180px" @keyup.enter="load" />
      </el-form-item>
      <el-form-item label="状态">
        <el-select v-model="query.status" placeholder="全部" clearable style="width:120px">
          <el-option label="待审批" value="待审批" />
          <el-option label="审批通过" value="审批通过" />
          <el-option label="审批拒绝" value="审批拒绝" />
        </el-select>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="load">查询</el-button>
        <el-button @click="resetQuery">重置</el-button>
      </el-form-item>
    </el-form>

    <el-table :data="rows" border stripe v-loading="loading" size="default">
      <el-table-column type="index" label="#" width="55" align="center" />
      <el-table-column prop="leaveNo" label="请假单号" width="140" align="center" class-name="col-nowrap" />
      <el-table-column prop="empCode" label="工号" width="90" align="center" />
      <el-table-column prop="empName" label="姓名" width="80" align="center" />
      <el-table-column prop="leaveType" label="请假类型" width="90" align="center" />
      <el-table-column prop="startDate" label="开始日期" width="110" align="center">
        <template #default="{ row }">{{ fmtDate(row.startDate) }}</template>
      </el-table-column>
      <el-table-column prop="endDate" label="结束日期" width="110" align="center">
        <template #default="{ row }">{{ fmtDate(row.endDate) }}</template>
      </el-table-column>
      <el-table-column prop="days" label="天数" width="70" align="center" />
      <el-table-column prop="reason" label="请假事由" min-width="150" show-overflow-tooltip />
      <el-table-column prop="status" label="状态" width="90" align="center">
        <template #default="{ row }">
          <el-tag :type="statusType(row.status)" size="small">{{ row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="180" align="center" fixed="right">
        <template #default="{ row }">
          <el-button v-if="row.status === '待审批'" link type="success" size="small" @click="approve(row, true)">通过</el-button>
          <el-button v-if="row.status === '待审批'" link type="danger" size="small" @click="approve(row, false)">拒绝</el-button>
          <el-button link type="primary" size="small" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 新建/编辑对话框 -->
    <el-dialog v-model="dialogVisible" :title="editing ? '编辑' : '新建'" width="600px" destroy-on-close>
      <el-form :model="form" label-width="100px">
        <el-row :gutter="10">
          <el-col :span="12"><el-form-item label="工号" required><el-input v-model="form.empCode" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="姓名" required><el-input v-model="form.empName" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="请假类型"><el-select v-model="form.leaveType" style="width:100%"><el-option v-for="t in leaveTypes" :key="t" :label="t" :value="t" /></el-select></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="天数"><el-input-number v-model="form.days" :min="0.5" :step="0.5" style="width:100%" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="开始日期"><el-date-picker v-model="form.startDate" type="date" value-format="YYYY-MM-DD" style="width:100%" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="结束日期"><el-date-picker v-model="form.endDate" type="date" value-format="YYYY-MM-DD" style="width:100%" /></el-form-item></el-col>
          <el-col :span="24"><el-form-item label="请假事由"><el-input v-model="form.reason" type="textarea" :rows="3" /></el-form-item></el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>
  </el-card>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '../../api/modules'
import { useAuthStore } from '../../stores/auth'

const auth = useAuthStore()
const rows = ref([])
const loading = ref(false)
const query = reactive({ keyword: '', status: '' })
const dialogVisible = ref(false)
const editing = ref(false)
const form = reactive({})
const leaveTypes = ['事假', '病假', '年假', '调休', '婚假', '产假']

async function load() {
  loading.value = true
  try {
    rows.value = await api.leaveRequests({ ...query })
  } finally {
    loading.value = false
  }
}

function resetQuery() {
  query.keyword = ''
  query.status = ''
  load()
}

function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { leaveType: '事假', days: 1, startDate: new Date().toISOString().slice(0, 10), endDate: new Date().toISOString().slice(0, 10) })
  dialogVisible.value = true
}

function openEdit(row) {
  editing.value = true
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, JSON.parse(JSON.stringify(row)))
  dialogVisible.value = true
}

async function save() {
  if (editing.value) {
    await api.updateLeaveRequest(form.id, form)
    ElMessage.success('更新成功')
  } else {
    await api.createLeaveRequest(form)
    ElMessage.success('新建成功')
  }
  dialogVisible.value = false
  load()
}

async function approve(row, approved) {
  await ElMessageBox.confirm(`确定${approved ? '通过' : '拒绝'}该请假申请？`, '提示', { type: 'warning' })
  await api.leaveApprove(row.id, { approved, approver: auth.displayName })
  ElMessage.success(approved ? '已通过' : '已拒绝')
  load()
}

async function remove(row) {
  await ElMessageBox.confirm('确定删除该请假单？', '提示', { type: 'warning' })
  await api.deleteLeaveRequest(row.id)
  ElMessage.success('删除成功')
  load()
}

function statusType(s) {
  return { 待审批: 'warning', 审批通过: 'success', 审批拒绝: 'danger' }[s] || 'info'
}
function fmtDate(v) { return v ? String(v).slice(0, 10) : '-' }

onMounted(load)
</script>

<style scoped>
.search-bar { margin-bottom: 12px; }
</style>
