<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>班次管理</span>
        <el-button type="primary" @click="openCreate">新增班次</el-button>
      </div>
    </template>

    <el-table :data="displayRows" border stripe v-loading="loading">
      <el-table-column prop="name" label="班次名称" min-width="120" align="center" />
      <el-table-column prop="startTime" label="开始时间" width="120" align="center" class-name="col-nowrap" />
      <el-table-column prop="endTime" label="结束时间" width="120" align="center" class-name="col-nowrap" />
      <el-table-column prop="remark" label="备注" min-width="200" align="center" class="allow-wrap" show-overflow-tooltip />
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

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑班次' : '新增班次'" width="480px" destroy-on-close>
      <el-form :model="form" label-width="90px">
        <el-form-item label="班次名称" required><el-input v-model="form.name" /></el-form-item>
        <el-form-item label="开始时间" required>
          <el-time-picker v-model="form.startTime" value-format="HH:mm" style="width:100%" />
        </el-form-item>
        <el-form-item label="结束时间" required>
          <el-time-picker v-model="form.endTime" value-format="HH:mm" style="width:100%" />
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

const rows = ref([])
import { usePagination } from '../../composables/usePagination'
const { currentPage, pageSize, pageSizes, total, displayRows, resetPage, handleSizeChange } = usePagination(rows)
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref(false)
const form = reactive({})

async function load() {
  loading.value = true
  try { rows.value = await api.shifts() } finally { loading.value = false }
}
function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { name: '', startTime: '', endTime: '', remark: '' })
  dialogVisible.value = true
}
function openEdit(row) {
  editing.value = true
  Object.assign(form, { ...row })
  dialogVisible.value = true
}
async function save() {
  if (!form.name) return ElMessage.warning('请输入班次名称')
  if (editing.value) { await api.updateShift(form.id, form); ElMessage.success('更新成功') }
  else { await api.createShift(form); ElMessage.success('新增成功') }
  dialogVisible.value = false
  load()
}
async function remove(row) {
  await ElMessageBox.confirm('确定删除该班次？', '提示', { type: 'warning' })
  await api.deleteShift(row.id)
  ElMessage.success('删除成功')
  load()
}
onMounted(load)
</script>

<style scoped>
.pagination-wrap { display: flex; justify-content: flex-end; margin-top: 12px; }
:deep(.col-nowrap .cell) { white-space: nowrap !important; overflow: hidden !important; text-overflow: unset !important; }
.op-btns { display: inline-flex; align-items: center; gap: 0; white-space: nowrap; }
.op-sep { color: #dcdfe6; margin: 0 6px; font-weight: 300; user-select: none; }
.op-btns :deep(.el-button) { font-size: 14px; margin: 0; }
</style>
