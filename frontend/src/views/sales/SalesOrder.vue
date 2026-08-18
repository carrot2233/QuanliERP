<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>销售订单</span>
        <el-button type="primary" @click="openCreate">新增订单</el-button>
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

    <el-table :data="displayRows" border stripe v-loading="loading" header-cell-class-name="th-cell">
      <el-table-column prop="orderNo" label="订单号" width="180" align="center" class-name="col-nowrap" />
      <el-table-column prop="customerName" label="客户" min-width="160" align="center" />
      <el-table-column prop="orderDate" label="下单日期" width="130" align="center" class-name="col-nowrap">
        <template #default="{ row }">{{ fmt(row.orderDate) }}</template>
      </el-table-column>
      <el-table-column prop="deliveryDate" label="约定交期" width="130" align="center" class-name="col-nowrap">
        <template #default="{ row }">{{ fmt(row.deliveryDate) }}</template>
      </el-table-column>
      <el-table-column prop="amount" label="金额" width="120" align="center">
        <template #default="{ row }">{{ Number(row.amount).toFixed(2) }}</template>
      </el-table-column>
      <el-table-column prop="status" label="状态" width="100" align="center">
        <template #default="{ row }">
          <el-tag :type="statusTag(row.status)" size="small">{{ row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="210" align="center" fixed="right">
        <template #default="{ row }">
          <div class="op-btns">
            <el-button link type="primary" @click="openDetail(row)">查看</el-button>
            <template v-if="row.status !== '完成' && row.status !== '已发货'">
              <span class="op-sep">|</span>
              <el-button link type="primary" @click="openEdit(row)">编辑</el-button>
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

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑订单' : '新增订单'" width="820px" destroy-on-close>
      <el-form :model="form" label-width="90px">
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="客户" required>
              <el-select v-model="form.customerId" filterable style="width:100%">
                <el-option v-for="c in customers" :key="c.id" :label="c.name" :value="c.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="下单日期">
              <el-date-picker v-model="form.orderDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="约定交期">
              <el-date-picker v-model="form.deliveryDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="备注"><el-input v-model="form.remark" /></el-form-item>
        <el-divider content-position="left">订单明细</el-divider>
        <el-table :data="form.items" border size="small">
          <el-table-column label="产品" min-width="220">
            <template #default="{ row }">
              <el-select v-model="row.productId" filterable placeholder="选择产品" style="width:100%" @change="() => onProductChange(row)">
                <el-option v-for="p in products" :key="p.id" :label="p.code + ' ' + p.name" :value="p.id" />
              </el-select>
            </template>
          </el-table-column>
          <el-table-column label="规格" width="150" align="center">
            <template #default="{ row }">{{ productSpec(row.productId) }}</template>
          </el-table-column>
          <el-table-column label="数量" width="120" align="center">
            <template #default="{ row }"><el-input-number v-model="row.qty" :min="1" style="width:100%" /></template>
          </el-table-column>
          <el-table-column label="单价" width="120" align="center">
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
        <el-button size="small" style="margin-top:8px" @click="form.items.push({ productId: null, qty: 1, price: 0 })">+ 添加明细</el-button>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>

    <el-dialog v-model="detailVisible" title="订单详情" width="700px">
      <el-descriptions :column="2" border size="small">
        <el-descriptions-item label="订单号">{{ detail.orderNo }}</el-descriptions-item>
        <el-descriptions-item label="客户">{{ detail.customerName }}</el-descriptions-item>
        <el-descriptions-item label="下单日期">{{ fmt(detail.orderDate) }}</el-descriptions-item>
        <el-descriptions-item label="约定交期">{{ fmt(detail.deliveryDate) }}</el-descriptions-item>
        <el-descriptions-item label="状态">{{ detail.status }}</el-descriptions-item>
        <el-descriptions-item label="总金额">{{ Number(detail.amount).toFixed(2) }}</el-descriptions-item>
      </el-descriptions>
      <el-table :data="detail.items || []" border size="small" style="margin-top:12px">
        <el-table-column prop="productName" label="产品" min-width="180" />
        <el-table-column prop="productSpec" label="规格" min-width="140" />
        <el-table-column prop="qty" label="数量" width="90" align="center" />
        <el-table-column prop="price" label="单价" width="90" align="center" />
        <el-table-column prop="deliveredQty" label="已发" width="90" align="center" />
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
import { reactive, ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '../../api/modules'

const statuses = ['草稿', '确认', '已排产', '部分发货', '已发货', '完成', '取消']
const rows = ref([])
import { usePagination } from '../../composables/usePagination'
const { currentPage, pageSize, pageSizes, total, displayRows, resetPage, handleSizeChange } = usePagination(rows)
const customers = ref([])
const products = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const detailVisible = ref(false)
const editing = ref(false)
const query = reactive({ keyword: '', status: '' })
const _initQuery = { ...query }
const form = reactive({})
const detail = ref({})

async function load() {
  loading.value = true
  try { rows.value = await api.salesOrders(query) } finally { loading.value = false }
}

function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { customerId: null, orderDate: new Date().toISOString().slice(0, 10), deliveryDate: new Date().toISOString().slice(0, 10), remark: '', items: [{ productId: null, qty: 1, price: 0 }] })
  dialogVisible.value = true
}

async function openEdit(row) {
  editing.value = true
  const d = await api.salesOrder(row.id)
  Object.assign(form, { ...d, items: d.items.map(i => ({ ...i })) })
  dialogVisible.value = true
}

async function openDetail(row) {
  detail.value = await api.salesOrder(row.id)
  detailVisible.value = true
}

function onProductChange(row) {
  const p = products.value.find(x => x.id === row.productId)
  if (p && !row.price) row.price = Number(p.salePrice || 0)
}

function productSpec(pid) {
  return products.value.find(p => p.id === pid)?.specification || ''
}

async function save() {
  if (!form.customerId) return ElMessage.warning('请选择客户')
  if (!form.items?.length || form.items.some(i => !i.productId)) return ElMessage.warning('请完善订单明细')
  if (editing.value) {
    await api.updateSalesOrder(form.id, form)
    ElMessage.success('更新成功')
  } else {
    await api.createSalesOrder(form)
    ElMessage.success('新增成功')
  }
  dialogVisible.value = false
  load()
}

async function changeStatus(row, s) {
  await api.salesOrderStatus(row.id, s)
  ElMessage.success('状态更新成功')
  load()
}

async function remove(row) {
  await ElMessageBox.confirm('确定删除该订单？', '提示', { type: 'warning' })
  await api.deleteSalesOrder(row.id)
  ElMessage.success('删除成功')
  load()
}

function statusTag(s) {
  return { 草稿: 'info', 确认: 'primary', 已排产: 'warning', 部分发货: 'warning', 已发货: 'success', 完成: 'success', 取消: 'danger' }[s] || 'info'
}

function fmt(v) { return v ? String(v).slice(0, 10) : '-' }
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
