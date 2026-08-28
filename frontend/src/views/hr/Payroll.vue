<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>薪资管理</span>
        <el-button type="primary" size="small" @click="openCreate">新建</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="关键词">
        <el-input v-model="query.keyword" placeholder="工号/姓名" clearable style="width:150px" @keyup.enter="load" />
      </el-form-item>
      <el-form-item label="薪资月份">
        <el-input v-model="query.payMonth" placeholder="如 2026-08" clearable style="width:120px" @keyup.enter="load" />
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="load">查询</el-button>
        <el-button @click="resetQuery">重置</el-button>
      </el-form-item>
    </el-form>

    <el-table :data="rows" border stripe v-loading="loading" size="default">
      <el-table-column type="index" label="#" width="55" align="center" />
      <el-table-column prop="empCode" label="工号" width="90" align="center" class-name="col-nowrap" />
      <el-table-column prop="empName" label="姓名" width="80" align="center" />
      <el-table-column prop="payMonth" label="薪资月份" width="100" align="center" />
      <el-table-column prop="baseSalary" label="基本工资" width="100" align="right">
        <template #default="{ row }">{{ fmtMoney(row.baseSalary) }}</template>
      </el-table-column>
      <el-table-column prop="postSalary" label="岗位工资" width="100" align="right">
        <template #default="{ row }">{{ fmtMoney(row.postSalary) }}</template>
      </el-table-column>
      <el-table-column prop="performance" label="绩效工资" width="100" align="right">
        <template #default="{ row }">{{ fmtMoney(row.performance) }}</template>
      </el-table-column>
      <el-table-column prop="overtime" label="加班费" width="90" align="right">
        <template #default="{ row }">{{ fmtMoney(row.overtime) }}</template>
      </el-table-column>
      <el-table-column prop="bonus" label="奖金" width="80" align="right">
        <template #default="{ row }">{{ fmtMoney(row.bonus) }}</template>
      </el-table-column>
      <el-table-column prop="deduction" label="扣款" width="80" align="right">
        <template #default="{ row }">{{ fmtMoney(row.deduction) }}</template>
      </el-table-column>
      <el-table-column prop="actualSalary" label="实发工资" width="110" align="right">
        <template #default="{ row }"><b>{{ fmtMoney(row.actualSalary) }}</b></template>
      </el-table-column>
      <el-table-column prop="status" label="状态" width="90" align="center">
        <template #default="{ row }">
          <el-tag :type="row.status === '已发放' ? 'success' : 'warning'" size="small">{{ row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="150" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 新建/编辑对话框 -->
    <el-dialog v-model="dialogVisible" :title="editing ? '编辑' : '新建'" width="680px" destroy-on-close>
      <el-form :model="form" label-width="100px">
        <el-row :gutter="10">
          <el-col :span="12"><el-form-item label="工号" required><el-input v-model="form.empCode" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="姓名" required><el-input v-model="form.empName" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="薪资月份"><el-input v-model="form.payMonth" placeholder="如 2026-08" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="基本工资"><el-input-number v-model="form.baseSalary" :min="0" style="width:100%" :controls="false" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="岗位工资"><el-input-number v-model="form.postSalary" :min="0" style="width:100%" :controls="false" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="绩效工资"><el-input-number v-model="form.performance" :min="0" style="width:100%" :controls="false" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="加班费"><el-input-number v-model="form.overtime" :min="0" style="width:100%" :controls="false" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="奖金"><el-input-number v-model="form.bonus" :min="0" style="width:100%" :controls="false" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="扣款"><el-input-number v-model="form.deduction" :min="0" style="width:100%" :controls="false" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="社保"><el-input-number v-model="form.socialInsurance" :min="0" style="width:100%" :controls="false" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="公积金"><el-input-number v-model="form.housingFund" :min="0" style="width:100%" :controls="false" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="状态"><el-select v-model="form.status" style="width:100%"><el-option label="待发放" value="待发放" /><el-option label="已发放" value="已发放" /></el-select></el-form-item></el-col>
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

const rows = ref([])
const loading = ref(false)
const query = reactive({ keyword: '', payMonth: '' })
const dialogVisible = ref(false)
const editing = ref(false)
const form = reactive({})

async function load() {
  loading.value = true
  try {
    rows.value = await api.payrolls({ ...query })
  } finally {
    loading.value = false
  }
}

function resetQuery() {
  query.keyword = ''
  query.payMonth = ''
  load()
}

function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { status: '待发放', payMonth: new Date().toISOString().slice(0, 7) })
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
    await api.updatePayroll(form.id, form)
    ElMessage.success('更新成功')
  } else {
    await api.createPayroll(form)
    ElMessage.success('新建成功')
  }
  dialogVisible.value = false
  load()
}

async function remove(row) {
  await ElMessageBox.confirm('确定删除该薪资单？', '提示', { type: 'warning' })
  await api.deletePayroll(row.id)
  ElMessage.success('删除成功')
  load()
}

function fmtMoney(v) { return v == null ? '-' : Number(v).toFixed(2) }

onMounted(load)
</script>

<style scoped>
.search-bar { margin-bottom: 12px; }
</style>
