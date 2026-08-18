<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>模具台账</span>
        <el-button type="primary" @click="openCreate">新增模具</el-button>
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
      <el-form-item><el-button @click="resetQuery">重置</el-button></el-form-item>
    </el-form>

    <el-table :data="displayRows" border stripe v-loading="loading">
      <el-table-column prop="moldNo" label="模具编号" width="130" align="center" class-name="col-nowrap" />
      <el-table-column prop="name" label="模具名称" min-width="150" align="center" />
      <el-table-column prop="customerName" label="客户" min-width="130" align="center" />
      <el-table-column prop="projectName" label="项目" min-width="130" align="center" />
      <el-table-column prop="planNo" label="制号" width="130" align="center" class-name="col-nowrap" />
      <el-table-column prop="processType" label="工艺类型" width="100" align="center" />
      <el-table-column prop="tonnage" label="吨位" width="80" align="center">
        <template #default="{ row }">{{ row.tonnage ? row.tonnage + 'T' : '-' }}</template>
      </el-table-column>
      <el-table-column prop="status" label="状态" width="100" align="center">
        <template #default="{ row }">
          <el-tag :type="statusTag(row.status)" size="small">{{ row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="location" label="存放位置" width="120" align="center" />
      <el-table-column prop="manager" label="负责人" width="90" align="center" />
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

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑模具' : '新增模具'" width="660px" destroy-on-close>
      <el-form :model="form" label-width="100px">
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="模具编号" required><el-input v-model="form.moldNo" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="模具名称" required><el-input v-model="form.name" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="制号"><el-input v-model="form.planNo" /></el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="客户">
              <el-select v-model="form.customerId" filterable clearable style="width:100%">
                <el-option v-for="c in customers" :key="c.id" :label="c.name" :value="c.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="项目名称"><el-input v-model="form.projectName" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="工艺类型">
              <el-select v-model="form.processType" allow-create filterable clearable style="width:100%">
                <el-option v-for="t in processTypes" :key="t" :label="t" :value="t" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="吨位"><el-input-number v-model="form.tonnage" :min="0" style="width:100%" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="状态">
              <el-select v-model="form.status" style="width:100%">
                <el-option v-for="s in statuses" :key="s" :label="s" :value="s" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="产品">
              <el-select v-model="form.productId" filterable clearable style="width:100%">
                <el-option v-for="p in products" :key="p.id" :label="p.name" :value="p.id" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="12"><el-form-item label="存放位置"><el-input v-model="form.location" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="负责人"><el-input v-model="form.manager" /></el-form-item></el-col>
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

const statuses = ['设计', '制造中', '试模', '调试完成', '量产', '维修', '报废']
const processTypes = ['冲压模', '拉伸模', '注塑模', '压铸模', '成型模', '工装夹具']
const rows = ref([])
import { usePagination } from '../../composables/usePagination'
const { currentPage, pageSize, pageSizes, total, displayRows, resetPage, handleSizeChange } = usePagination(rows)
const customers = ref([])
const products = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref(false)
const query = reactive({ keyword: '', status: '' })
const _initQuery = { ...query }
const form = reactive({})

async function load() {
  loading.value = true
  try { rows.value = await api.molds(query) } finally { loading.value = false }
}
function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { moldNo: '', name: '', customerId: null, projectName: '', planNo: '', processType: '', tonnage: 0, status: '制造中', location: '', manager: '', productId: null, remark: '' })
  dialogVisible.value = true
}
function openEdit(row) {
  editing.value = true
  Object.assign(form, { ...row })
  dialogVisible.value = true
}
async function save() {
  if (!form.moldNo || !form.name) return ElMessage.warning('请填写编号与名称')
  if (editing.value) { await api.updateMold(form.id, form); ElMessage.success('更新成功') }
  else { await api.createMold(form); ElMessage.success('新增成功') }
  dialogVisible.value = false
  load()
}
async function remove(row) {
  await ElMessageBox.confirm('确定删除该模具？', '提示', { type: 'warning' })
  await api.deleteMold(row.id)
  ElMessage.success('删除成功')
  load()
}
function statusTag(s) { return { 设计: 'info', 制造中: 'primary', 试模: 'warning', 调试完成: 'success', 量产: 'success', 维修: 'danger', 报废: 'danger' }[s] || 'info' }
function resetQuery() { Object.assign(query, { ..._initQuery }); resetPage(); load() }

onMounted(async () => {
  load()
  customers.value = await api.customers()
  products.value = await api.products()
})
</script>

<style scoped>
.pagination-wrap { display: flex; justify-content: flex-end; margin-top: 12px; }
:deep(.col-nowrap .cell) { white-space: nowrap !important; overflow: hidden !important; text-overflow: unset !important; }
.op-btns { display: inline-flex; align-items: center; gap: 0; white-space: nowrap; }
.op-sep { color: #dcdfe6; margin: 0 6px; font-weight: 300; user-select: none; }
.op-btns :deep(.el-button) { font-size: 14px; margin: 0; }
</style>
