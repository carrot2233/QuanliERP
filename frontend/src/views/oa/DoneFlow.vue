<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>已办事项</span>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="关键词">
        <el-input v-model="query.keyword" placeholder="请输入关键词" clearable style="width:180px" @keyup.enter="load" />
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="load">查询</el-button>
        <el-button @click="resetQuery">重置</el-button>
      </el-form-item>
    </el-form>

    <el-table :data="rows" border stripe v-loading="loading" size="default">
      <el-table-column type="index" label="#" width="55" align="center" />
      <el-table-column prop="instanceNo" label="实例编号" width="170" align="center" class-name="col-nowrap" />
      <el-table-column prop="instanceName" label="实例名称" min-width="180" show-overflow-tooltip />
      <el-table-column prop="nodeName" label="审批节点" width="120" align="center" />
      <el-table-column prop="status" label="审批结果" width="90" align="center">
        <template #default="{ row }">
          <el-tag :type="row.status === '已同意' ? 'success' : 'danger'" size="small">{{ row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="comment" label="审批意见" min-width="120" show-overflow-tooltip />
      <el-table-column prop="flowStatus" label="流程状态" width="100" align="center">
        <template #default="{ row }">
          <el-tag :type="flowStatusType(row.flowStatus)" size="small">{{ row.flowStatus }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="creator" label="发起人" width="100" align="center" />
      <el-table-column prop="handledAt" label="处理时间" width="160" align="center">
        <template #default="{ row }">{{ fmtDateTime(row.handledAt) }}</template>
      </el-table-column>
    </el-table>
  </el-card>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import api from '../../api/modules'
import { useAuthStore } from '../../stores/auth'

const auth = useAuthStore()
const rows = ref([])
const loading = ref(false)
const query = reactive({ keyword: '' })

async function load() {
  loading.value = true
  try {
    rows.value = await api.flowDone({ approver: auth.displayName, ...query })
  } finally {
    loading.value = false
  }
}

function resetQuery() {
  query.keyword = ''
  load()
}

function flowStatusType(s) {
  return { 审批中: 'warning', 审批通过: 'success', 审批拒绝: 'danger' }[s] || 'info'
}
function fmtDateTime(v) { return v ? String(v).replace('T', ' ').slice(0, 19) : '-' }

onMounted(load)
</script>

<style scoped>
.search-bar { margin-bottom: 12px; }
</style>
