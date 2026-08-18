<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>采购订单</span>
        <el-button type="primary" @click="openCreate">新增采购订单</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="订单号"><el-input v-model="query.keyword" clearable style="width:180px" @keyup.enter="load" /></el-form-item>
      <el-form-item label="状态">
        <el-select v-model="query.status" clearable style="width:130px">
          <el-option v-for="s in statuses" :key="s" :label="s" :value="s" />
        </el-select>
      </el-form-item>
      <el-form-item><el-button type="primary" @click="load">查询</el-button></el-form-item>
      <el-form-item><el-button @click="resetQuery">重置</el-button></el-form-item>
    </el-form>

    <el-table :data="displayRows" border stripe v-loading="loading">
      <el-table-column prop="orderNo" label="采购单号" width="180" align="center" class-name="col-nowrap" />
      <el-table-column prop="supplierName" label="供应商" min-width="170" align="center" />
      <el-table-column prop="orderDate" label="下单日期" width="130" align="center" class-name="col-nowrap">
        <template #default="{ row }">{{ fmt(row.orderDate) }}</template>
      </el-table-column>
      <el-table-column prop="expectDate" label="预计到货" width="130" align="center" class-name="col-nowrap">
        <template #default="{ row }">{{ fmt(row.expectDate) }}</template>
      </el-table-column>
      <el-table-column prop="amount" label="金额" width="110" align="center">
        <template #default="{ row }">{{ Number(row.amount).toFixed(2) }}</template>
      </el-table-column>
      <el-table-column prop="status" label="状态" width="100" align="center">
        <template #default="{ row }">
          <el-tag :type="statusTag(row.status)" size="small">{{ row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="170" align="center" fixed="right">
        <template #default="{ row }">
          <div class="op-btns">
            <el-button link type="primary" @click="openEdit(row)">编辑</el-button>
            <template v-if="row.status !== '已到货' && row.status !== '完成'">
              <span class="op-sep">|</span>
              <el-dropdown trigger="click">
                <el-button link type="warning">状态</el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item v-for="s in statuses" :key="s" @click="changeStatus(row, s)">{{ s }}</el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
              <span class="op-sep">|</span>
              <el-button link type="danger" @click="remove(row)">删除</el-button>
            </template>
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

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑采购订单' : '新增采购订单'" width="820px" destroy-on-close>
      <el-form :model="form" label-width="90px">
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="供应商" required>
              <el-select v-model="form.supplierId" filterable style="width:100%">
                <el-option v-for="s in suppliers" :key="s.id" :label="s.name" :value="s.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="下单日期">
              <el-date-picker v-model="form.orderDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="预计到货">
              <el-date-picker v-model="form.expectDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="备注"><el-input v-model="form.remark" /></el-form-item>
        <el-divider content-position="left">采购明细</el-divider>
        <el-table :data="form.items" border size="small">
          <el-table-column label="材料" min-width="230">
            <template #default="{ row }">
              <el-select v-model="row.materialId" filterable placeholder="选择材料" style="width:100%" @change="() => onMaterialChange(row)">
                <el-option v-for="m in materials" :key="m.id" :label="m.name + ' ' + m.specification" :value="m.id" />
              </el-select>
            </template>
          </el-table-column>
          <el-table-column label="数量" width="130" align="center">
            <template #default="{ row }"><el-input-number v-model="row.qty" :min="1" style="width:100%" /></template>
          </el-table-column>
          <el-table-column label="单价" width="130" align="center">
            <template #default="{ row }"><el-input-number v-model="row.price" :min="0" :precision="2" style="width:100%" /></template>
          </el-table-column>
          <el-table-column label="金额" width="110" align="center">
            <template #default="{ row }">{{ ((row.qty || 0) * (row.price || 0)).toFixed(2) }}</template>
          </el-table-column>
          <el-table-column label="" width="60" align="center">
            <template #default="{ $index }"><el-button link type="danger" size="small" @click="form.items.splice($index, 1)">删除</el-button></template>
          </el-table-column>
        </el-table>

    <div class="pagination-wrap">
      <el-pagination background
        v-model:current-page="currentPage" v-model:page-size="pageSize"
        :page-sizes="pageSizes" :total="total" :small="true"
        layout="total, sizes, prev, pager, next" @size-change="handleSizeChange" @current-change="() => {}" />
    </div>
        <el-button size="small" style="margin-top:8px" @click="form.items.push({ materialId: null, qty: 1, price: 0 })">+ 添加明细</el-button>
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

const statuses = ['草稿', '已下单', '部分到货', '已到货', '完成', '取消']
const rows = ref([])
import { usePagination } from '../../composables/usePagination'
const { currentPage, pageSize, pageSizes, total, displayRows, resetPage, handleSizeChange } = usePagination(rows)
const suppliers = ref([])
const materials = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref(false)
const query = reactive({ keyword: '', status: '' })
const _initQuery = { ...query }
const form = reactive({})

async function load() {
  loading.value = true
  try { rows.value = await api.purchaseOrders(query) } finally { loading.value = false }
}

function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { supplierId: null, orderDate: new Date().toISOString().slice(0, 10), expectDate: new Date().toISOString().slice(0, 10), remark: '', items: [{ materialId: null, qty: 1, price: 0 }] })
  dialogVisible.value = true
}

async function openEdit(row) {
  editing.value = true
  const o = await api.purchaseOrders().then(list => list.find(x => x.id === row.id))
  Object.assign(form, { ...o, items: o.items.map(i => ({ ...i })) })
  dialogVisible.value = true
}

function onMaterialChange(row) {
  const m = materials.value.find(x => x.id === row.materialId)
  if (m && !row.price) row.price = Number(m.unitPrice || 0)
}

async function save() {
  if (!form.supplierId) return ElMessage.warning('请选择供应商')
  if (!form.items?.length || form.items.some(i => !i.materialId)) return ElMessage.warning('请完善采购明细')
  if (editing.value) {
    await api.updatePurchaseOrder(form.id, form)
    ElMessage.success('更新成功')
  } else {
    await api.createPurchaseOrder(form)
    ElMessage.success('新增成功')
  }
  dialogVisible.value = false
  load()
}

async function changeStatus(row, s) {
  await api.purchaseOrderStatus(row.id, s)
  ElMessage.success('状态更新成功')
  load()
}

async function remove(row) {
  await ElMessageBox.confirm('确定删除该采购订单？', '提示', { type: 'warning' })
  await api.deletePurchaseOrder(row.id)
  ElMessage.success('删除成功')
  load()
}

function statusTag(s) {
  return { 草稿: 'info', 已下单: 'primary', 部分到货: 'warning', 已到货: 'success', 完成: 'success', 取消: 'danger' }[s] || 'info'
}
function fmt(v) { return v ? String(v).slice(0, 10) : '-' }
function resetQuery() { Object.assign(query, { ..._initQuery }); resetPage(); load() }

onMounted(async () => {
  load()
  suppliers.value = await api.suppliers()
  materials.value = await api.materials()
})
</script>

<style scoped>
.pagination-wrap { display: flex; justify-content: flex-end; margin-top: 12px; }
:deep(.col-nowrap .cell) { white-space: nowrap !important; overflow: hidden !important; text-overflow: unset !important; }
.op-btns { display: inline-flex; align-items: center; gap: 0; white-space: nowrap; }
.op-sep { color: #dcdfe6; margin: 0 6px; font-weight: 300; user-select: none; }
.op-btns :deep(.el-button) { font-size: 14px; margin: 0; }
</style>
