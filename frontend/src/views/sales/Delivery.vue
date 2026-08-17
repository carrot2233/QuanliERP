<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>发货管理</span>
        <el-button type="primary" size="small" @click="openCreate">新增发货单</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="发货单号"><el-input v-model="query.keyword" clearable style="width:180px" @keyup.enter="load" /></el-form-item>
      <el-form-item><el-button type="primary" @click="load">查询</el-button></el-form-item>
    </el-form>

    <el-table :data="rows" border stripe v-loading="loading">
      <el-table-column prop="deliveryNo" label="发货单号" width="140" align="center" />
      <el-table-column prop="orderNo" label="关联订单" width="140" align="center" />
      <el-table-column prop="customerName" label="客户" min-width="160" align="center" />
      <el-table-column prop="warehouseName" label="发货仓库" width="110" align="center" />
      <el-table-column prop="deliveryDate" label="发货日期" width="110" align="center">
        <template #default="{ row }">{{ fmt(row.deliveryDate) }}</template>
      </el-table-column>
      <el-table-column prop="carrier" label="承运方" width="110" align="center" />
      <el-table-column prop="plateNo" label="车牌号" width="100" align="center" />
      <el-table-column prop="status" label="状态" width="90" align="center">
        <template #default="{ row }"><el-tag type="success" size="small">{{ row.status }}</el-tag></template>
      </el-table-column>
      <el-table-column label="操作" width="150" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="openDetail(row)">查看</el-button>
          <el-button v-if="row.status !== '完成'" link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="dialogVisible" title="新增发货单" width="760px" destroy-on-close>
      <el-form :model="form" label-width="100px">
        <el-row :gutter="10">
          <el-col :span="12">
            <el-form-item label="关联订单" required>
              <el-select v-model="form.salesOrderId" filterable style="width:100%" @change="onOrderChange">
                <el-option v-for="o in orders" :key="o.id" :label="o.orderNo + ' (' + o.customerName + ')'" :value="o.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="发货日期">
              <el-date-picker v-model="form.deliveryDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="发货仓库" required>
              <el-select v-model="form.warehouseId" style="width:100%">
                <el-option v-for="w in warehouses" :key="w.id" :label="w.name" :value="w.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="承运方"><el-input v-model="form.carrier" /></el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="车牌号"><el-input v-model="form.plateNo" /></el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="司机"><el-input v-model="form.driver" /></el-form-item>
          </el-col>
        </el-row>
        <el-divider content-position="left">发货明细（自动带出订单未发数量）</el-divider>
        <el-table :data="form.items" border size="small">
          <el-table-column prop="productName" label="产品" min-width="180" />
          <el-table-column prop="productSpec" label="规格" min-width="140" />
          <el-table-column label="未发数量" width="100" align="center">
            <template #default="{ row }">{{ remainQty(row) }}</template>
          </el-table-column>
          <el-table-column label="本次发货" width="130" align="center">
            <template #default="{ row }">
              <el-input-number v-model="row.qty" :min="0" :max="remainQty(row)" style="width:100%" />
            </template>
          </el-table-column>
        </el-table>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存发货</el-button>
      </template>
    </el-dialog>

    <el-dialog v-model="detailVisible" title="发货单详情" width="700px">
      <el-descriptions :column="2" border size="small">
        <el-descriptions-item label="发货单号">{{ detail.deliveryNo }}</el-descriptions-item>
        <el-descriptions-item label="关联订单">{{ detail.orderNo }}</el-descriptions-item>
        <el-descriptions-item label="客户">{{ detail.customerName }}</el-descriptions-item>
        <el-descriptions-item label="仓库">{{ detail.warehouseName }}</el-descriptions-item>
        <el-descriptions-item label="承运方">{{ detail.carrier }} {{ detail.plateNo }}</el-descriptions-item>
        <el-descriptions-item label="司机">{{ detail.driver }}</el-descriptions-item>
      </el-descriptions>
      <el-table :data="detail.items || []" border size="small" style="margin-top:12px">
        <el-table-column prop="productName" label="产品" min-width="180" />
        <el-table-column prop="productSpec" label="规格" min-width="140" />
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
  try { rows.value = await api.deliveries(query) } finally { loading.value = false }
}

function openCreate() {
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { salesOrderId: null, warehouseId: null, deliveryDate: new Date().toISOString().slice(0, 10), carrier: '', plateNo: '', driver: '', items: [] })
  dialogVisible.value = true
}

async function onOrderChange(orderId) {
  if (!orderId) { form.items = []; return }
  const o = await api.salesOrder(orderId)
  form.items = o.items.map(i => ({
    productId: i.productId, productName: i.productName, productSpec: i.productSpec,
    qty: i.qty - i.deliveredQty, price: i.price, remain: i.qty - i.deliveredQty
  })).filter(i => i.remain > 0)
  if (!form.customerId) form.customerId = o.customerId
}

function remainQty(row) { return row.remain }

async function save() {
  if (!form.salesOrderId) return ElMessage.warning('请选择订单')
  if (!form.warehouseId) return ElMessage.warning('请选择仓库')
  if (!form.items.length || form.items.some(i => !i.qty)) return ElMessage.warning('请填写发货数量')
  await api.createDelivery(form)
  ElMessage.success('发货成功，库存已扣减')
  dialogVisible.value = false
  load()
}

async function openDetail(row) {
  detail.value = await api.deliveries().then(list => list.find(x => x.id === row.id))
  detailVisible.value = true
}

async function remove(row) {
  await ElMessageBox.confirm('删除发货单将回滚库存，确定继续？', '提示', { type: 'warning' })
  await api.deleteDelivery(row.id)
  ElMessage.success('删除成功，库存已回滚')
  load()
}

function fmt(v) { return v ? String(v).slice(0, 10) : '-' }
onMounted(async () => {
  load()
  orders.value = await api.salesOrders({})
  warehouses.value = await api.warehouses()
})
</script>
