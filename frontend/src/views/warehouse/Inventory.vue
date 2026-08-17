<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>库存查询</span>
        <div>
          <el-button size="small" @click="load">刷新</el-button>
        </div>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="关键词"><el-input v-model="query.keyword" clearable style="width:200px" @keyup.enter="load" /></el-form-item>
      <el-form-item label="类型">
        <el-select v-model="query.itemType" clearable style="width:110px">
          <el-option label="材料" value="材料" /><el-option label="产品" value="产品" />
        </el-select>
      </el-form-item>
      <el-form-item label="仓库">
        <el-select v-model="query.warehouseId" clearable style="width:130px">
          <el-option v-for="w in warehouses" :key="w.id" :label="w.name" :value="String(w.id)" />
        </el-select>
      </el-form-item>
      <el-form-item><el-button type="primary" @click="load">查询</el-button></el-form-item>
    </el-form>

    <el-table :data="rows" border stripe v-loading="loading">
      <el-table-column prop="warehouseName" label="仓库" width="110" align="center" />
      <el-table-column prop="itemType" label="类型" width="70" align="center">
        <template #default="{ row }"><el-tag :type="row.itemType === '材料' ? 'warning' : 'primary'" size="small">{{ row.itemType }}</el-tag></template>
      </el-table-column>
      <el-table-column prop="code" label="编号" width="110" align="center" />
      <el-table-column prop="name" label="名称" min-width="160" align="center" />
      <el-table-column prop="specification" label="规格尺寸" min-width="170" align="center" />
      <el-table-column prop="qty" label="库存数量" width="100" align="center" />
      <el-table-column prop="safeStock" label="安全库存" width="90" align="center" />
      <el-table-column prop="unit" label="单位" width="60" align="center" />
      <el-table-column prop="stockStatus" label="状态" width="80" align="center">
        <template #default="{ row }">
          <el-tag :type="{ 正常: 'success', 预警: 'warning', 缺货: 'danger' }[row.stockStatus]" size="small">{{ row.stockStatus }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="location" label="库位" width="100" align="center" />
      <el-table-column prop="updatedAt" label="更新时间" width="160" align="center">
        <template #default="{ row }">{{ fmt(row.updatedAt) }}</template>
      </el-table-column>
    </el-table>
  </el-card>
</template>

<script setup>
import { reactive, ref, onMounted } from 'vue'
import api from '../../api/modules'

const rows = ref([])
const warehouses = ref([])
const loading = ref(false)
const query = reactive({ keyword: '', itemType: '', warehouseId: '' })

async function load() {
  loading.value = true
  try { rows.value = await api.inventory(query) } finally { loading.value = false }
}
function fmt(v) { return v ? String(v).replace('T', ' ').slice(0, 19) : '-' }
onMounted(async () => {
  load()
  warehouses.value = await api.warehouses()
})
</script>
