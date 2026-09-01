<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>到货管理</span>
        <el-button type="primary" @click="openCreate">新增到货单</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="到货单号"><el-input v-model="query.keyword" clearable style="width:180px" @keyup.enter="load" /></el-form-item>
      <el-form-item><el-button type="primary" @click="load">查询</el-button></el-form-item>
      <el-form-item><el-button @click="resetQuery">重置</el-button></el-form-item>
    </el-form>

    <el-table :data="displayRows" border stripe v-loading="loading">
      <el-table-column prop="receiptNo" label="到货单号" width="180" align="center" class-name="col-nowrap" />
      <el-table-column prop="orderNo" label="关联采购单" width="180" align="center" class-name="col-nowrap" />
      <el-table-column prop="supplierName" label="供应商" min-width="170" align="center" />
      <el-table-column prop="warehouseName" label="入库仓库" width="110" align="center" />
      <el-table-column prop="receiptDate" label="到货日期" width="130" align="center" class-name="col-nowrap">
        <template #default="{ row }">{{ fmt(row.receiptDate) }}</template>
      </el-table-column>
      <el-table-column prop="status" label="状态" width="90" align="center">
        <template #default="{ row }"><el-tag type="success" size="small">{{ row.status }}</el-tag></template>
      </el-table-column>
      <el-table-column prop="remark" label="备注" min-width="140" align="center" class="allow-wrap" show-overflow-tooltip />
      <el-table-column label="操作" width="210" align="center" fixed="right">
        <template #default="{ row }">
          <div class="op-btns">
            <el-button link type="primary" @click="openDetail(row)">查看</el-button>
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

    <el-dialog v-model="dialogVisible" title="新增到货单" width="760px" destroy-on-close>
      <el-form :model="form" label-width="100px">
        <el-row :gutter="10">
          <el-col :span="12">
            <el-form-item label="关联采购单" required>
              <el-select v-model="form.purchaseOrderId" filterable style="width:100%" @change="onOrderChange">
                <el-option v-for="o in receivableOrders" :key="o.id" :label="o.orderNo + ' (' + o.supplierName + ')'" :value="o.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="到货日期">
              <el-date-picker v-model="form.receiptDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="入库仓库" required>
              <el-select v-model="form.warehouseId" style="width:100%">
                <el-option v-for="w in warehouses" :key="w.id" :label="w.name" :value="w.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="备注"><el-input v-model="form.remark" /></el-form-item>
          </el-col>
        </el-row>
        <el-divider content-position="left">到货明细（自动带出采购未收数量）</el-divider>
        <el-table :data="form.items" border size="small">
          <el-table-column prop="materialName" label="材料" min-width="190" />
          <el-table-column prop="materialSpec" label="规格" min-width="150" />
          <el-table-column label="未收数量" width="100" align="center">
            <template #default="{ row }">{{ row.remain }}</template>
          </el-table-column>
          <el-table-column label="本次到货" width="130" align="center">
            <template #default="{ row }">
              <el-input-number v-model="row.qty" :min="0" :max="row.remain" style="width:100%" />
            </template>
          </el-table-column>
        </el-table>

    <div class="pagination-wrap">
      <el-pagination background
        v-model:current-page="currentPage" v-model:page-size="pageSize"
        :page-sizes="pageSizes" :total="total" :small="true"
        layout="total, sizes, prev, pager, next" @size-change="handleSizeChange" @current-change="() => {}" />
    </div>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存入库</el-button>
      </template>
    </el-dialog>

    <el-dialog v-model="detailVisible" title="到货单详情" width="700px">
      <el-descriptions :column="2" border size="small">
        <el-descriptions-item label="到货单号">{{ detail.receiptNo }}</el-descriptions-item>
        <el-descriptions-item label="关联采购单">{{ detail.orderNo }}</el-descriptions-item>
        <el-descriptions-item label="供应商">{{ detail.supplierName }}</el-descriptions-item>
        <el-descriptions-item label="仓库">{{ detail.warehouseName }}</el-descriptions-item>
        <el-descriptions-item label="到货日期">{{ fmt(detail.receiptDate) }}</el-descriptions-item>
      </el-descriptions>
      <el-table :data="detail.items || []" border size="small" style="margin-top:12px">
        <el-table-column prop="materialName" label="材料" min-width="180" />
        <el-table-column prop="materialSpec" label="规格" min-width="150" />
        <el-table-column prop="qty" label="数量" width="90" align="center" />
        <el-table-column prop="price" label="单价" width="90" align="center" />
      </el-table>

    <div class="pagination-wrap">
      <el-pagination background
        v-model:current-page="currentPage" v-model:page-size="pageSize"
        :page-sizes="pageSizes" :total="total" :small="true"
        layout="total, sizes, prev, pager, next" @size-change="handleSizeChange" @current-change="() => {}" />
    </div>
    </el-dialog>
  </el-card>
</template>

<script setup>
import { reactive, ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '../../api/modules'

const rows = ref([])
import { usePagination } from '../../composables/usePagination'
const { currentPage, pageSize, pageSizes, total, displayRows, resetPage, handleSizeChange } = usePagination(rows)
const orders = ref([])
const warehouses = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const detailVisible = ref(false)
const query = reactive({ keyword: '' })
const _initQuery = { ...query }
const form = reactive({})
const detail = ref({})

// 可到货订单状态（草稿/取消/已到货/完成 不可到货）
const receivableStatuses = ['已下单', '部分到货']
const receivableOrders = computed(() => orders.value.filter(o => receivableStatuses.includes(o.status)))

async function load() {
  loading.value = true
  try { rows.value = await api.receipts(query) } finally { loading.value = false }
}

function openCreate() {
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { purchaseOrderId: null, supplierId: null, warehouseId: null, receiptDate: new Date().toISOString().slice(0, 10), remark: '', items: [] })
  dialogVisible.value = true
}

function onOrderChange(orderId) {
  if (!orderId) { form.supplierId = null; form.items = []; return }
  const o = orders.value.find(x => x.id === orderId)
  form.supplierId = o.supplierId
  form.items = (o.items || []).map(i => ({
    materialId: i.materialId, materialName: i.materialName, materialSpec: i.materialSpec,
    qty: i.qty - i.receivedQty, price: i.price, remain: i.qty - i.receivedQty
  })).filter(i => i.remain > 0)
}

async function save() {
  if (!form.purchaseOrderId) return ElMessage.warning('请选择采购订单')
  if (!form.warehouseId) return ElMessage.warning('请选择入库仓库')
  if (!form.items.length || form.items.some(i => !i.qty)) return ElMessage.warning('请填写到货数量')
  await api.createReceipt(form)
  ElMessage.success('到货入库成功，库存已增加')
  dialogVisible.value = false
  load()
}

async function openDetail(row) {
  detail.value = await api.receipts().then(list => list.find(x => x.id === row.id))
  detailVisible.value = true
}

async function remove(row) {
  await ElMessageBox.confirm('删除到货单将回滚库存，确定继续？', '提示', { type: 'warning' })
  await api.deleteReceipt(row.id)
  ElMessage.success('删除成功，库存已回滚')
  load()
}

function fmt(v) { return v ? String(v).slice(0, 10) : '-' }
function resetQuery() { Object.assign(query, { ..._initQuery }); resetPage(); load() }

onMounted(async () => {
  load()
  orders.value = await api.purchaseOrders({})
  warehouses.value = await api.warehouses()
})
</script>

<style scoped>
.pagination-wrap { display: flex; justify-content: flex-end; margin-top: 12px; }
:deep(.col-nowrap .cell) { white-space: nowrap !important; overflow: hidden !important; text-overflow: unset !important; }
.op-btns { display: inline-flex; align-items: center; gap: 0; white-space: nowrap; }
.op-sep { color: #dcdfe6; margin: 0 6px; font-weight: 300; user-select: none; }
.op-btns :deep(.el-button) { font-size: 14px; margin: 0; }
</style>
