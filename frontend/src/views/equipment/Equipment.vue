<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>设备台账</span>
        <el-button type="primary" @click="openCreate">新增设备</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="关键词"><el-input v-model="query.keyword" clearable style="width:200px" @keyup.enter="load" /></el-form-item>
      <el-form-item label="状态">
        <el-select v-model="query.status" clearable style="width:100px">
          <el-option v-for="s in statuses" :key="s" :label="s" :value="s" />
        </el-select>
      </el-form-item>
      <el-form-item><el-button type="primary" @click="load">查询</el-button></el-form-item>
      <el-form-item><el-button @click="resetQuery">重置</el-button></el-form-item>
    </el-form>

    <el-table :data="displayRows" border stripe v-loading="loading">
      <el-table-column prop="code" label="设备编号" width="110" align="center" />
      <el-table-column prop="name" label="设备名称" min-width="130" align="center" />
      <el-table-column prop="model" label="型号" min-width="120" align="center" />
      <el-table-column prop="equipType" label="类型" width="90" align="center" />
      <el-table-column prop="tonnage" label="吨位" width="80" align="center">
        <template #default="{ row }">{{ row.tonnage ? row.tonnage + 'T' : '-' }}</template>
      </el-table-column>
      <el-table-column prop="workshop" label="车间" width="100" align="center" />
      <el-table-column prop="status" label="状态" width="90" align="center">
        <template #default="{ row }">
          <el-tag :type="statusTag(row.status)" size="small">{{ row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="manufacturer" label="制造商" min-width="120" align="center" />
      <el-table-column prop="purchaseDate" label="购置日期" width="130" align="center" class-name="col-nowrap">
        <template #default="{ row }">{{ fmt(row.purchaseDate) }}</template>
      </el-table-column>
      <el-table-column prop="nextMaintainDate" label="下次保养" width="130" align="center" class-name="col-nowrap">
        <template #default="{ row }">{{ fmt(row.nextMaintainDate) }}</template>
      </el-table-column>
      <el-table-column label="保养提醒" width="90" align="center">
        <template #default="{ row }">
          <el-tag v-if="row.overdue" type="danger" size="small">超期</el-tag>
          <el-tag v-else-if="nearOverdue(row)" type="warning" size="small">即将到期</el-tag>
          <el-tag v-else type="success" size="small">正常</el-tag>
        </template>
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

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑设备' : '新增设备'" width="800px" destroy-on-close>
      <el-form :model="form" label-width="100px">
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="设备编号" required><el-input v-model="form.code" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="设备名称" required><el-input v-model="form.name" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="型号"><el-input v-model="form.model" /></el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="类型">
              <el-select v-model="form.equipType" allow-create filterable style="width:100%">
                <el-option v-for="t in types" :key="t" :label="t" :value="t" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="吨位"><el-input-number v-model="form.tonnage" :min="0" style="width:100%" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="车间"><el-input v-model="form.workshop" /></el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="状态">
              <el-select v-model="form.status" style="width:100%">
                <el-option v-for="s in statuses" :key="s" :label="s" :value="s" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="制造商"><el-input v-model="form.manufacturer" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="购置日期">
              <el-date-picker v-model="form.purchaseDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="保养周期"><el-input v-model="form.maintenanceCycle" placeholder="如 每月/每季度" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="上次保养">
              <el-date-picker v-model="form.lastMaintainDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="下次保养">
              <el-date-picker v-model="form.nextMaintainDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
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

const types = ['冲床', '油压机', '剪板机', '折弯机', '车床', '铣床', '钻床', '线切割', '磨床', '焊机', '行车', '其他']
const statuses = ['运行', '维修', '停机', '报废']
const rows = ref([])
import { usePagination } from '../../composables/usePagination'
const { currentPage, pageSize, pageSizes, total, displayRows, resetPage, handleSizeChange } = usePagination(rows)
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref(false)
const query = reactive({ keyword: '', status: '' })
const _initQuery = { ...query }
const form = reactive({})

async function load() {
  loading.value = true
  try { rows.value = await api.equipments(query) } finally { loading.value = false }
}
function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { code: '', name: '', model: '', equipType: '', tonnage: 0, workshop: '', status: '运行', manufacturer: '', purchaseDate: null, maintenanceCycle: '', lastMaintainDate: null, nextMaintainDate: null, remark: '' })
  dialogVisible.value = true
}
function openEdit(row) {
  editing.value = true
  Object.assign(form, { ...row })
  dialogVisible.value = true
}
async function save() {
  if (!form.code || !form.name) return ElMessage.warning('请填写编号与名称')
  const data = { ...form }
  ;['purchaseDate', 'lastMaintainDate', 'nextMaintainDate'].forEach(f => { if (data[f] === '' || data[f] === undefined) data[f] = null })
  if (editing.value) { await api.updateEquipment(form.id, data); ElMessage.success('更新成功') }
  else { await api.createEquipment(data); ElMessage.success('新增成功') }
  dialogVisible.value = false
  load()
}
async function remove(row) {
  await ElMessageBox.confirm('确定删除该设备？', '提示', { type: 'warning' })
  await api.deleteEquipment(row.id)
  ElMessage.success('删除成功')
  load()
}
function statusTag(s) { return { 运行: 'success', 维修: 'warning', 停机: 'danger', 报废: 'danger' }[s] || 'info' }
function nearOverdue(row) { return row.nextMaintainDate && !row.overdue && new Date(row.nextMaintainDate) - new Date() < 30 * 86400000 }
function fmt(v) { return v ? String(v).slice(0, 10) : '-' }
function resetQuery() { Object.assign(query, { ..._initQuery }); resetPage(); load() }

onMounted(load)
</script>

<style scoped>
.pagination-wrap { display: flex; justify-content: flex-end; margin-top: 12px; }
:deep(.col-nowrap .cell) { white-space: nowrap !important; overflow: hidden !important; text-overflow: unset !important; }
.op-btns { display: inline-flex; align-items: center; gap: 0; white-space: nowrap; }
.op-sep { color: #dcdfe6; margin: 0 6px; font-weight: 300; user-select: none; }
.op-btns :deep(.el-button) { font-size: 14px; margin: 0; }
</style>
