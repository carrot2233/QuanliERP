<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>到货管理</span>
        <el-button type="primary" size="small" @click="openCreate">新增到货单</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="到货单号"><el-input v-model="query.keyword" clearable style="width:180px" @keyup.enter="load" /></el-form-item>
      <el-form-item><el-button type="primary" @click="load">查询</el-button></el-form-item>
    </el-form>

    <el-table :data="rows" border stripe v-loading="loading">
      <el-table-column prop="receiptNo" label="到货单号" width="140" align="center" />
      <el-table-column prop="orderNo" label="关联采购单" width="140" align="center" />
      <el-table-column prop="supplierName" label="供应商" min-width="170" align="center" />
      <el-table-column prop="warehouseName" label="入库仓库" width="110" align="center" />
      <el-table-column prop="receiptDate" label="到货日期" width="110" align="center">
        <template #default="{ row }">{{ fmt(row.receiptDate) }}</template>
      </el-table-column>
      <el-table-column prop="status" label="状态" width="90" align="center">
        <template #default="{ row }"><el-tag type="success" size="small">{{ row.status }}</el-tag></template>
      </el-table-column>
      <el-table-column prop="remark" label="备注" min-width="140" align="center" class="allow-wrap" />
      <el-table-column label="操作" width="130" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="openDetail(row)">查看</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="dialogVisible" title="新增到货单" width="760px" destroy-on-close>
      <el-form :model="form" label-width="100px">
        <el-row :gutter="10">
          <el-col :span="12">
            <el-form-item label="关联采购单" required>
              <el-select v-model="form.purchaseOrderId" filterable style="width:100%" @change="onOrderChange">
                <el-option v-for="o in orders" :key="o.id" :label="o.orderNo + ' (' + o.supplierName + ')'" :value="o.id" />
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
    </el-dialog>
  </el-card>
</template>

<script setup>
import { reactive, ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '../../api/modules'

const rows = ref([])
const orders = ref([])
const warehouses = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const detailVisible = ref(false)
const query = reactive({ keyword: '' })
const form = reactive({})
const detail = ref({})

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
onMounted(async () => {
  load()
  orders.value = await api.purchaseOrders({})
  warehouses.value = await api.warehouses()
})
</script>
