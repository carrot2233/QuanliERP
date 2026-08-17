<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>生产计划（制号）</span>
        <el-button type="primary" size="small" @click="openCreate">新增计划</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="关键词"><el-input v-model="query.keyword" clearable style="width:200px" @keyup.enter="load" /></el-form-item>
      <el-form-item label="状态">
        <el-select v-model="query.status" clearable style="width:120px">
          <el-option v-for="s in statuses" :key="s" :label="s" :value="s" />
        </el-select>
      </el-form-item>
      <el-form-item><el-button type="primary" @click="load">查询</el-button></el-form-item>
    </el-form>

    <el-table :data="rows" border stripe v-loading="loading">
      <el-table-column prop="planNo" label="制号" width="100" align="center" />
      <el-table-column prop="customerName" label="客户" min-width="150" align="center" />
      <el-table-column prop="projectName" label="项目名称" min-width="160" align="center" />
      <el-table-column prop="productName" label="产品" min-width="140" align="center">
        <template #default="{ row }">{{ row.productName }} {{ row.productSpec }}</template>
      </el-table-column>
      <el-table-column prop="materialName" label="材料" min-width="140" align="center">
        <template #default="{ row }">{{ row.materialName }} {{ row.materialSpec }}</template>
      </el-table-column>
      <el-table-column prop="oneOutputs" label="台份" width="60" align="center" />
      <el-table-column prop="planQty" label="计划数量" width="90" align="center" />
      <el-table-column label="完成/计划" width="110" align="center">
        <template #default="{ row }">
          <el-progress :percentage="Math.min(100, Math.round((row.doneQty / row.planQty || 0) * 100))" :stroke-width="10" />
          <span style="font-size:12px">{{ row.doneQty }}/{{ row.planQty }}</span>
        </template>
      </el-table-column>
      <el-table-column label="计划起止" width="200" align="center">
        <template #default="{ row }">{{ fmt(row.plannedStart) }} ~ {{ fmt(row.plannedEnd) }}</template>
      </el-table-column>
      <el-table-column prop="status" label="状态" width="90" align="center">
        <template #default="{ row }">
          <el-tag :type="statusTag(row.status)" size="small">{{ row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="180" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="openEdit(row)">编辑</el-button>
          <el-dropdown trigger="click" style="margin:0 4px">
            <el-button link type="warning" size="small">状态</el-button>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item v-for="s in statuses" :key="s" @click="changeStatus(row, s)">{{ s }}</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑生产计划' : '新增生产计划'" width="720px" destroy-on-close>
      <el-form :model="form" label-width="100px">
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="制号">
              <el-input v-model="form.planNo" placeholder="留空自动生成 M+日期" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="客户" required>
              <el-select v-model="form.customerId" filterable style="width:100%">
                <el-option v-for="c in customers" :key="c.id" :label="c.name" :value="c.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="项目名称"><el-input v-model="form.projectName" /></el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="产品">
              <el-select v-model="form.productId" filterable clearable style="width:100%">
                <el-option v-for="p in products" :key="p.id" :label="p.name" :value="p.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="材料">
              <el-select v-model="form.materialId" filterable clearable style="width:100%">
                <el-option v-for="m in materials" :key="m.id" :label="m.name" :value="m.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="台份"><el-input-number v-model="form.oneOutputs" :min="1" style="width:100%" /></el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="计划数量" required><el-input-number v-model="form.planQty" :min="1" style="width:100%" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="计划开始">
              <el-date-picker v-model="form.plannedStart" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="计划结束">
              <el-date-picker v-model="form.plannedEnd" type="date" value-format="YYYY-MM-DD" style="width:100%" />
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

const statuses = ['未开始', '进行中', '已完成', '暂停']
const rows = ref([])
const customers = ref([])
const products = ref([])
const materials = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref(false)
const query = reactive({ keyword: '', status: '' })
const form = reactive({})

async function load() {
  loading.value = true
  try { rows.value = await api.productionPlans(query) } finally { loading.value = false }
}
function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { planNo: '', customerId: null, projectName: '', productId: null, materialId: null, oneOutputs: 1, planQty: 1, plannedStart: new Date().toISOString().slice(0, 10), plannedEnd: '', remark: '' })
  dialogVisible.value = true
}
async function openEdit(row) {
  editing.value = true
  const o = await api.productionPlans().then(list => list.find(x => x.id === row.id))
  Object.assign(form, { ...o })
  dialogVisible.value = true
}
async function save() {
  if (!form.customerId) return ElMessage.warning('请选择客户')
  if (!form.planQty) return ElMessage.warning('请输入计划数量')
  if (editing.value) { await api.updateProductionPlan(form.id, form); ElMessage.success('更新成功') }
  else { await api.createProductionPlan(form); ElMessage.success('新增成功') }
  dialogVisible.value = false
  load()
}
async function changeStatus(row, s) {
  await api.productionPlanStatus(row.id, s)
  ElMessage.success('状态更新成功')
  load()
}
async function remove(row) {
  await ElMessageBox.confirm('确定删除该生产计划？', '提示', { type: 'warning' })
  await api.deleteProductionPlan(row.id)
  ElMessage.success('删除成功')
  load()
}
function statusTag(s) { return { 未开始: 'info', 进行中: 'primary', 已完成: 'success', 暂停: 'warning' }[s] || 'info' }
function fmt(v) { return v ? String(v).slice(0, 10) : '-' }
onMounted(async () => {
  load()
  customers.value = await api.customers()
  products.value = await api.products()
  materials.value = await api.materials()
})
</script>
