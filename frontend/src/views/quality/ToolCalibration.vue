<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>计量器具检定结果处理单</span>
        <el-button type="primary" @click="openCreate">新增检定处理单</el-button>
      </div>
    </template>

    <el-table :data="displayRows" border stripe v-loading="loading">
      <el-table-column prop="calibrationNo" label="检定单号" width="180" align="center" class-name="col-nowrap" />
      <el-table-column prop="toolName" label="器具名称" min-width="130" align="center" />
      <el-table-column prop="measureRange" label="测量范围" min-width="120" align="center" />
      <el-table-column prop="toolNo" label="器具编号" width="130" align="center" class-name="col-nowrap" />
      <el-table-column prop="origin" label="来源" min-width="110" align="center" />
      <el-table-column prop="dept" label="部门" width="90" align="center" />
      <el-table-column prop="userName" label="使用人" width="80" align="center" />
      <el-table-column prop="result" label="检定结果" width="90" align="center">
        <template #default="{ row }">
          <el-tag :type="{ 合格: 'success', 不合格: 'danger', 待检定: 'warning' }[row.result]" size="small">{{ row.result }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="anomalyDesc" label="异常描述" min-width="150" align="center" class="allow-wrap" show-overflow-tooltip />
      <el-table-column prop="handleAdvice" label="处理意见" min-width="150" align="center" class="allow-wrap" show-overflow-tooltip />
      <el-table-column prop="reviewer" label="复核人" width="80" align="center" />
      <el-table-column prop="reviewDate" label="复核日期" width="130" align="center" class-name="col-nowrap">
        <template #default="{ row }">{{ fmt(row.reviewDate) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="150" align="center" fixed="right">
        <template #default="{ row }">
          <div class="op-btns">
            <el-button link type="primary" @click="openEdit(row)">编辑</el-button>
            <span class="op-sep">|</span>
            <el-button link type="danger" @click="remove(row)">删除</el-button>
          </div>
        </template>
      </el-table-column>
    </el-table>

    <div class="pagination-wrap">
      <el-pagination background
        v-model:current-page="currentPage" v-model:page-size="pageSize"
        :page-sizes="pageSizes" :total="total" :small="true"
        layout="total, sizes, prev, pager, next" @size-change="handleSizeChange" @current-change="() => {}" />
    </div>

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑检定单' : '新增检定单'" width="760px" destroy-on-close>
      <el-form :model="form" label-width="90px">
        <el-form-item label="检定单号"><el-input v-model="form.calibrationNo" placeholder="留空自动生成" /></el-form-item>
        <el-row :gutter="10">
          <el-col :span="12"><el-form-item label="器具名称"><el-input v-model="form.toolName" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="器具编号"><el-input v-model="form.toolNo" /></el-form-item></el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="12"><el-form-item label="测量范围"><el-input v-model="form.measureRange" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="来源"><el-input v-model="form.origin" /></el-form-item></el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="领用日期">
              <el-date-picker v-model="form.receiveDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8"><el-form-item label="部门"><el-input v-model="form.dept" /></el-form-item></el-col>
          <el-col :span="8"><el-form-item label="使用人"><el-input v-model="form.userName" /></el-form-item></el-col>
        </el-row>
        <el-form-item label="检定结果">
          <el-select v-model="form.result" style="width:100%">
            <el-option v-for="r in results" :key="r" :label="r" :value="r" />
          </el-select>
        </el-form-item>
        <el-form-item label="异常描述"><el-input v-model="form.anomalyDesc" type="textarea" :rows="2" /></el-form-item>
        <el-form-item label="处理意见"><el-input v-model="form.handleAdvice" type="textarea" :rows="2" /></el-form-item>
        <el-form-item label="复核意见"><el-input v-model="form.reviewAdvice" type="textarea" :rows="2" /></el-form-item>
        <el-row :gutter="10">
          <el-col :span="12"><el-form-item label="复核人"><el-input v-model="form.reviewer" /></el-form-item></el-col>
          <el-col :span="12">
            <el-form-item label="复核日期">
              <el-date-picker v-model="form.reviewDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
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

const results = ['待检定', '合格', '不合格']
const rows = ref([])
import { usePagination } from '../../composables/usePagination'
const { currentPage, pageSize, pageSizes, total, displayRows, resetPage, handleSizeChange } = usePagination(rows)
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref(false)
const form = reactive({})

async function load() {
  loading.value = true
  try { rows.value = await api.toolCalibrations() } finally { loading.value = false }
}
function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { calibrationNo: '', toolName: '', measureRange: '', toolNo: '', origin: '', receiveDate: null, dept: '', userName: '', result: '待检定', anomalyDesc: '', handleAdvice: '', reviewAdvice: '', reviewer: '', reviewDate: null, remark: '' })
  dialogVisible.value = true
}
function openEdit(row) {
  editing.value = true
  Object.assign(form, { ...row })
  dialogVisible.value = true
}
async function save() {
  const data = { ...form }
  if (data.receiveDate === '' || data.receiveDate === undefined) data.receiveDate = null
  if (data.reviewDate === '' || data.reviewDate === undefined) data.reviewDate = null
  if (editing.value) { await api.updateToolCalibration(form.id, data); ElMessage.success('更新成功') }
  else { await api.createToolCalibration(data); ElMessage.success('新增成功') }
  dialogVisible.value = false
  load()
}
async function remove(row) {
  await ElMessageBox.confirm('确定删除该检定单？', '提示', { type: 'warning' })
  await api.deleteToolCalibration(row.id)
  ElMessage.success('删除成功')
  load()
}
function fmt(v) { return v ? String(v).slice(0, 10) : '-' }
onMounted(load)
</script>

<style scoped>
.pagination-wrap { display: flex; justify-content: flex-end; margin-top: 12px; }
:deep(.col-nowrap .cell) { white-space: nowrap !important; overflow: hidden !important; text-overflow: unset !important; }
.op-btns { display: inline-flex; align-items: center; gap: 0; white-space: nowrap; }
.op-sep { color: #dcdfe6; margin: 0 6px; font-weight: 300; user-select: none; }
.op-btns :deep(.el-button) { font-size: 14px; margin: 0; }
</style>
