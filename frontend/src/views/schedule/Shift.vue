<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>班次管理</span>
        <el-button type="primary" size="small" @click="openCreate">新增班次</el-button>
      </div>
    </template>

    <el-table :data="rows" border stripe v-loading="loading">
      <el-table-column prop="name" label="班次名称" min-width="120" align="center" />
      <el-table-column prop="startTime" label="开始时间" width="120" align="center" />
      <el-table-column prop="endTime" label="结束时间" width="120" align="center" />
      <el-table-column prop="remark" label="备注" min-width="200" align="center" class="allow-wrap" />
      <el-table-column label="操作" width="150" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

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
