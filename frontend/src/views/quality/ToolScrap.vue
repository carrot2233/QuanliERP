<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>计量器具报废处理单</span>
        <el-button type="primary" size="small" @click="openCreate">新增报废单</el-button>
      </div>
    </template>

    <el-table :data="rows" border stripe v-loading="loading">
      <el-table-column prop="scrapNo" label="报废单号" width="120" align="center" />
      <el-table-column prop="toolName" label="器具名称" min-width="140" align="center" />
      <el-table-column prop="specification" label="规格" min-width="110" align="center" />
      <el-table-column prop="manageNo" label="管理编号" width="100" align="center" />
      <el-table-column prop="factoryNo" label="出厂编号" width="100" align="center" />
      <el-table-column prop="manufacturer" label="制造厂家" min-width="130" align="center" />
      <el-table-column prop="holder" label="保管人" width="80" align="center" />
      <el-table-column prop="qty" label="数量" width="70" align="center" />
      <el-table-column prop="reason" label="报废原因" min-width="170" align="center" />
      <el-table-column prop="scrapDate" label="报废日期" width="110" align="center">
        <template #default="{ row }">{{ fmt(row.scrapDate) }}</template>
      </el-table-column>
      <el-table-column prop="applicant" label="申请人" width="80" align="center" />
      <el-table-column prop="approver" label="审批人" width="80" align="center" />
      <el-table-column label="操作" width="140" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑报废单' : '新增报废单'" width="640px" destroy-on-close>
      <el-form :model="form" label-width="90px">
        <el-form-item label="报废单号"><el-input v-model="form.scrapNo" placeholder="留空自动生成" /></el-form-item>
        <el-form-item label="器具名称"><el-input v-model="form.toolName" /></el-form-item>
        <el-form-item label="规格"><el-input v-model="form.specification" /></el-form-item>
        <el-row :gutter="10">
          <el-col :span="12"><el-form-item label="管理编号"><el-input v-model="form.manageNo" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="出厂编号"><el-input v-model="form.factoryNo" /></el-form-item></el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="12"><el-form-item label="制造厂家"><el-input v-model="form.manufacturer" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="保管人"><el-input v-model="form.holder" /></el-form-item></el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="数量"><el-input-number v-model="form.qty" :min="1" style="width:100%" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="领用日期">
              <el-date-picker v-model="form.receiveDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="报废日期">
              <el-date-picker v-model="form.scrapDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="报废原因"><el-input v-model="form.reason" type="textarea" :rows="2" /></el-form-item>
        <el-row :gutter="10">
          <el-col :span="12"><el-form-item label="申请人"><el-input v-model="form.applicant" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="审批人"><el-input v-model="form.approver" /></el-form-item></el-col>
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

const rows = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref(false)
const form = reactive({})

async function load() {
  loading.value = true
  try { rows.value = await api.toolScraps() } finally { loading.value = false }
}
function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { scrapNo: '', toolName: '', specification: '', manageNo: '', factoryNo: '', manufacturer: '', holder: '', qty: 1, receiveDate: '', scrapDate: new Date().toISOString().slice(0, 10), reason: '', applicant: '', approver: '', remark: '' })
  dialogVisible.value = true
}
function openEdit(row) {
  editing.value = true
  Object.assign(form, { ...row, scrapDate: String(row.scrapDate).slice(0, 10) })
  dialogVisible.value = true
}
async function save() {
  if (editing.value) { await api.updateToolScrap(form.id, form); ElMessage.success('更新成功') }
  else { await api.createToolScrap(form); ElMessage.success('新增成功') }
  dialogVisible.value = false
  load()
}
async function remove(row) {
  await ElMessageBox.confirm('确定删除该报废单？', '提示', { type: 'warning' })
  await api.deleteToolScrap(row.id)
  ElMessage.success('删除成功')
  load()
}
function fmt(v) { return v ? String(v).slice(0, 10) : '-' }
onMounted(load)
</script>
