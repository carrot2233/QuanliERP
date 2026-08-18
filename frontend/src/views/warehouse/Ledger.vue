<template>
  <el-card shadow="never">
    <template #header><span>库存流水</span></template>

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
      <el-table-column prop="itemName" label="物料名称" min-width="160" align="center" />
      <el-table-column prop="specification" label="规格" min-width="150" align="center" />
      <el-table-column prop="billType" label="单据类型" width="100" align="center">
        <template #default="{ row }"><el-tag size="small">{{ row.billType }}</el-tag></template>
      </el-table-column>
      <el-table-column prop="billNo" label="单号" width="180" align="center" class-name="col-nowrap" />
      <el-table-column prop="inQty" label="入库" width="90" align="center">
        <template #default="{ row }"><span style="color:#67c23a">{{ row.inQty || '-' }}</span></template>
      </el-table-column>
      <el-table-column prop="outQty" label="出库" width="90" align="center">
        <template #default="{ row }"><span style="color:#f56c6c">{{ row.outQty || '-' }}</span></template>
      </el-table-column>
      <el-table-column prop="balanceQty" label="结存" width="90" align="center" />
      <el-table-column prop="operator" label="操作人" width="90" align="center" />
      <el-table-column prop="remark" label="备注" min-width="130" align="center" class="allow-wrap" show-overflow-tooltip />
    </el-table>

    <div class="pagination-wrap">
      <el-pagination background
        v-model:current-page="currentPage" v-model:page-size="pageSize"
        :page-sizes="pageSizes" :total="total" :small="true"
        layout="total, sizes, prev, pager, next" @size-change="handleSizeChange" @current-change="() => {}" />
    </div>
  </el-card>
</template>

<script setup>
import { reactive, ref, onMounted } from 'vue'
import api from '../../api/modules'

const billTypes = ['采购入库', '车间入库', '生产领用', '销售出库', '退件', '发货退回', '入库冲销', '盘盈', '盘亏']
const rows = ref([])
import { usePagination } from '../../composables/usePagination'
const { currentPage, pageSize, pageSizes, total, displayRows, resetPage, handleSizeChange } = usePagination(rows)
const loading = ref(false)
const query = reactive({ itemName: '', billType: '', start: '', end: '' })
const _initQuery = { ...query }

async function load() {
  loading.value = true
  try { rows.value = await api.ledger(query) } finally { loading.value = false }
}
function fmt(v) { return v ? String(v).replace('T', ' ').slice(0, 19) : '-' }
function resetQuery() { Object.assign(query, { ..._initQuery }); resetPage(); load() }

onMounted(load)
</script>

<style scoped>
.pagination-wrap { display: flex; justify-content: flex-end; margin-top: 12px; }
:deep(.col-nowrap .cell) { white-space: nowrap !important; overflow: hidden !important; text-overflow: unset !important; }
</style>
