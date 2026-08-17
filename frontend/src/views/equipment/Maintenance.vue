<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>设备维护保养/维修记录</span>
        <el-button type="primary" size="small" @click="openCreate">新增记录</el-button>
      </div>
    </template>

    <el-table :data="rows" border stripe v-loading="loading">
      <el-table-column prop="equipmentId" label="设备编号" width="110" align="center">
        <template #default="{ row }">{{ equipments.find(e => e.id === row.equipmentId)?.code || row.equipmentId }}</template>
      </el-table-column>
      <el-table-column prop="equipmentName" label="设备名称" min-width="130" align="center" />
      <el-table-column prop="maintainDate" label="日期" width="100" align="center">
        <template #default="{ row }">{{ fmt(row.maintainDate) }}</template>
      </el-table-column>
      <el-table-column prop="type" label="类型" width="80" align="center">
        <template #default="{ row }">
          <el-tag :type="{ 保养: 'success', 维修: 'danger', 点检: 'info' }[row.type]" size="small">{{ row.type }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="content" label="内容" min-width="220" align="center" />
      <el-table-column prop="cost" label="费用" width="90" align="center">
        <template #default="{ row }">{{ Number(row.cost || 0).toFixed(2) }}</template>
      </el-table-column>
      <el-table-column prop="handler" label="经办人" width="90" align="center" />
      <el-table-column prop="result" label="结果" width="100" align="center" />
      <el-table-column label="操作" width="140" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑记录' : '新增记录'" width="600px" destroy-on-close>
      <el-form :model="form" label-width="90px">
        <el-form-item label="设备" required>
          <el-select v-model="form.equipmentId" filterable style="width:100%">
            <el-option v-for="e in equipments" :key="e.id" :label="e.code + ' ' + e.name" :value="e.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="日期">
          <el-date-picker v-model="form.maintainDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
        </el-form-item>
        <el-form-item label="类型">
          <el-select v-model="form.type" style="width:100%">
            <el-option v-for="t in types" :key="t" :label="t" :value="t" />
          </el-select>
        </el-form-item>
        <el-form-item label="内容"><el-input v-model="form.content" type="textarea" :rows="2" /></el-form-item>
        <el-form-item label="费用"><el-input-number v-model="form.cost" :min="0" :precision="2" style="width:100%" /></el-form-item>
        <el-form-item label="经办人"><el-input v-model="form.handler" /></el-form-item>
        <el-form-item label="结果"><el-input v-model="form.result" /></el-form-item>
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

const types = ['保养', '维修', '点检']
const rows = ref([])
const equipments = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref(false)
const form = reactive({})

async function load() {
  loading.value = true
  try { rows.value = await api.maintenances() } finally { loading.value = false }
}
function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { equipmentId: null, maintainDate: new Date().toISOString().slice(0, 10), type: '保养', content: '', cost: 0, handler: '', result: '', remark: '' })
  dialogVisible.value = true
}
function openEdit(row) {
  editing.value = true
  Object.assign(form, { ...row, maintainDate: String(row.maintainDate).slice(0, 10) })
  dialogVisible.value = true
}
async function save() {
  if (!form.equipmentId) return ElMessage.warning('请选择设备')
  if (editing.value) { await api.updateMaintenance(form.id, form); ElMessage.success('更新成功') }
  else { await api.createMaintenance(form); ElMessage.success('新增成功') }
  dialogVisible.value = false
  load()
}
async function remove(row) {
  await ElMessageBox.confirm('确定删除该记录？', '提示', { type: 'warning' })
  await api.deleteMaintenance(row.id)
  ElMessage.success('删除成功')
  load()
}
function fmt(v) { return v ? String(v).slice(0, 10) : '-' }
onMounted(async () => {
  load()
  equipments.value = await api.equipments({})
})
</script>
