<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>{{ direction === 'in' ? '入库管理' : '出库管理' }}</span>
        <el-button type="primary" @click="openCreate">新增{{ direction === 'in' ? '入库' : '出库' }}</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="物料名称"><el-input v-model="query.itemName" clearable style="width:160px" @keyup.enter="load" /></el-form-item>
      <el-form-item label="单据类型">
        <el-select v-model="query.billType" clearable style="width:140px">
          <el-option v-for="t in billTypes" :key="t" :label="t" :value="t" />
        </el-select>
      </el-form-item>
      <el-form-item label="开始">
        <el-date-picker v-model="query.start" type="date" value-format="YYYY-MM-DD" style="width:140px" />
      </el-form-item>
      <el-form-item label="结束">
        <el-date-picker v-model="query.end" type="date" value-format="YYYY-MM-DD" style="width:140px" />
      </el-form-item>
      <el-form-item><el-button type="primary" @click="load">查询</el-button></el-form-item>
      <el-form-item><el-button @click="resetQuery">重置</el-button></el-form-item>
    </el-form>

    <el-table :data="displayRows" border stripe v-loading="loading">
      <el-table-column prop="operationTime" label="操作时间" width="160" align="center" class-name="col-nowrap">
        <template #default="{ row }">{{ fmt(row.operationTime) }}</template>
      </el-table-column>
      <el-table-column prop="itemType" label="类型" width="70" align="center" />
      <el-table-column prop="itemName" label="物料名称" min-width="150" align="center" />
      <el-table-column prop="specification" label="规格" min-width="150" align="center" />
      <el-table-column prop="billType" label="单据类型" width="100" align="center">
        <template #default="{ row }"><el-tag size="small">{{ row.billType }}</el-tag></template>
      </el-table-column>
      <el-table-column prop="billNo" label="单号" width="180" align="center" class-name="col-nowrap" />
      <el-table-column v-if="direction === 'in'" prop="inQty" label="入库数量" width="100" align="center">
        <template #default="{ row }"><span style="color:#67c23a;font-weight:600">{{ row.inQty }}</span></template>
      </el-table-column>
      <el-table-column v-else prop="outQty" label="出库数量" width="100" align="center">
        <template #default="{ row }"><span style="color:#f56c6c;font-weight:600">{{ row.outQty }}</span></template>
      </el-table-column>
      <el-table-column prop="balanceQty" label="结存" width="90" align="center" />
      <el-table-column prop="operator" label="操作人" width="90" align="center" />
      <el-table-column prop="remark" label="备注" min-width="120" align="center" show-overflow-tooltip />
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

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑' + (direction === 'in' ? '入库' : '出库') + '记录' : '新增' + (direction === 'in' ? '入库' : '出库') + '记录'" width="560px" destroy-on-close>
      <el-form :model="form" label-width="90px">
        <el-form-item label="物料类型" required>
          <el-radio-group v-model="form.itemType">
            <el-radio-button label="材料">材料</el-radio-button>
            <el-radio-button label="产品">产品</el-radio-button>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="物料" required>
          <el-select v-model="form.itemId" filterable placeholder="选择物料" style="width:100%">
            <el-option v-for="it in items" :key="it.id" :label="it.code + ' ' + it.name + ' ' + (it.specification || '')" :value="it.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="仓库" required>
          <el-select v-model="form.warehouseId" style="width:100%">
            <el-option v-for="w in warehouses" :key="w.id" :label="w.name" :value="w.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="单据类型" required>
          <el-select v-model="form.billType" style="width:100%">
            <el-option v-for="t in formBillTypes" :key="t" :label="t" :value="t" />
          </el-select>
        </el-form-item>
        <el-form-item :label="direction === 'in' ? '入库数量' : '出库数量'" required>
          <el-input-number v-model="qty" :min="0" style="width:100%" />
        </el-form-item>
        <el-form-item label="单号"><el-input v-model="form.billNo" placeholder="留空自动生成" /></el-form-item>
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
import { computed, reactive, ref, watch, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '../../api/modules'
import { usePagination } from '../../composables/usePagination'

const props = defineProps({
  direction: { type: String, default: 'in' }
})

const inBillTypes = ['采购入库', '车间入库', '退件', '盘盈', '其他']
const outBillTypes = ['生产领用', '销售出库', '盘亏', '其他']
const billTypes = computed(() => props.direction === 'in' ? inBillTypes : outBillTypes)
const formBillTypes = computed(() => billTypes.value)

const rows = ref([])
const { currentPage, pageSize, pageSizes, total, displayRows, resetPage, handleSizeChange } = usePagination(rows)
const loading = ref(false)
const query = reactive({ itemName: '', billType: '', start: '', end: '', direction: props.direction })
const _initQuery = { ...query }
const dialogVisible = ref(false)
const editing = ref(false)
const form = reactive({})
const qty = ref(1)
const warehouses = ref([])
const materials = ref([])
const products = ref([])
const items = computed(() => form.itemType === '材料' ? materials.value : products.value)

function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { itemType: '材料', itemId: null, warehouseId: null, billType: formBillTypes.value[0], billNo: '', remark: '' })
  qty.value = 1
  dialogVisible.value = true
}

function openEdit(row) {
  editing.value = true
  Object.assign(form, { ...row })
  qty.value = props.direction === 'in' ? row.inQty : row.outQty
  dialogVisible.value = true
}

async function save() {
  if (!form.itemId) return ElMessage.warning('请选择物料')
  if (!form.warehouseId) return ElMessage.warning('请选择仓库')
  if (!qty.value || qty.value <= 0) return ElMessage.warning(`请输入${props.direction === 'in' ? '入库' : '出库'}数量`)
  const it = items.value.find(i => i.id === form.itemId)
  const payload = {
    warehouseId: form.warehouseId, itemType: form.itemType, itemId: form.itemId,
    itemName: it?.name || form.itemName || '',
    specification: it?.specification || form.specification || '',
    billType: form.billType, billNo: form.billNo, remark: form.remark,
    inQty: props.direction === 'in' ? qty.value : 0,
    outQty: props.direction === 'out' ? qty.value : 0
  }
  if (editing.value) {
    await api.updateStockRecord(form.id, payload)
    ElMessage.success('更新成功')
  } else {
    await api.stockInOut(payload)
    ElMessage.success('新增成功')
  }
  dialogVisible.value = false
  load()
}

async function remove(row) {
  await ElMessageBox.confirm(`确定删除该${props.direction === 'in' ? '入库' : '出库'}记录？删除后对应库存会反向冲减。`, '提示', { type: 'warning' })
  await api.deleteStockRecord(row.id)
  ElMessage.success('删除成功')
  load()
}

function fmt(v) { return v ? String(v).replace('T', ' ').slice(0, 19) : '-' }
function resetQuery() { Object.assign(query, { ..._initQuery }); resetPage(); load() }

async function load() {
  loading.value = true
  try { rows.value = await api.stockRecords({ ...query }) } finally { loading.value = false }
}

onMounted(async () => {
  load()
  warehouses.value = await api.warehouses()
  materials.value = await api.materials()
  products.value = await api.products()
})

watch(() => props.direction, () => {
  query.direction = props.direction
  resetQuery()
  load()
})
</script>

<style scoped>
.search-bar { margin-bottom: 12px; }
.pagination-wrap { display: flex; justify-content: flex-end; margin-top: 12px; }
:deep(.col-nowrap .cell) { white-space: nowrap !important; overflow: hidden !important; text-overflow: unset !important; }
.op-btns { display: inline-flex; align-items: center; gap: 0; white-space: nowrap; }
.op-sep { color: #dcdfe6; margin: 0 6px; font-weight: 300; user-select: none; }
.op-btns :deep(.el-button) { font-size: 14px; margin: 0; }
</style>
