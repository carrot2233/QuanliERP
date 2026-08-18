<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>库存预警</span>
        <el-button size="small" type="primary" @click="load">刷新</el-button>
      </div>
    </template>

    <el-alert title="库存低于安全库存或为零的物料，请及时安排采购/生产补货。" type="warning" :closable="false" style="margin-bottom:12px" />

    <el-table :data="displayRows" border stripe v-loading="loading">
      <el-table-column prop="warehouseName" label="仓库" width="110" align="center" />
      <el-table-column prop="itemType" label="类型" width="70" align="center">
        <template #default="{ row }"><el-tag :type="row.itemType === '材料' ? 'warning' : 'primary'" size="small">{{ row.itemType }}</el-tag></template>
      </el-table-column>
      <el-table-column prop="code" label="编号" width="110" align="center" />
      <el-table-column prop="name" label="名称" min-width="160" align="center" />
      <el-table-column prop="specification" label="规格尺寸" min-width="170" align="center" />
      <el-table-column prop="qty" label="当前库存" width="100" align="center">
        <template #default="{ row }"><span style="color:#f56c6c;font-weight:600">{{ row.qty }}</span></template>
      </el-table-column>
      <el-table-column prop="safeStock" label="安全库存" width="100" align="center" />
      <el-table-column prop="unit" label="单位" width="70" align="center" />
      <el-table-column prop="shortQty" label="缺口数量" width="100" align="center">
        <template #default="{ row }">{{ row.shortQty }}</template>
      </el-table-column>
      <el-table-column prop="stockStatus" label="状态" width="90" align="center">
        <template #default="{ row }">
          <el-tag :type="row.qty <= 0 ? 'danger' : 'warning'" size="small">{{ row.qty <= 0 ? '缺货' : row.stockStatus }}</el-tag>
        </template>
      </el-table-column>
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
import { ref, onMounted } from 'vue'
import api from '../../api/modules'

const rows = ref([])
import { usePagination } from '../../composables/usePagination'
const { currentPage, pageSize, pageSizes, total, displayRows, resetPage, handleSizeChange } = usePagination(rows)
const loading = ref(false)

async function load() {
  loading.value = true
  try { rows.value = await api.inventoryWarnings() } finally { loading.value = false }
}
onMounted(load)
</script>

<style scoped>
.pagination-wrap { display: flex; justify-content: flex-end; margin-top: 12px; }
</style>
