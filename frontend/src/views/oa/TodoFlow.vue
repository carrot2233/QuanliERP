<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>待办事项</span>
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
      <el-table-column prop="nodeName" label="当前节点" width="120" align="center" />
      <el-table-column prop="creator" label="发起人" width="100" align="center" />
      <el-table-column prop="createdAt" label="创建时间" width="160" align="center">
        <template #default="{ row }">{{ fmtDateTime(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="150" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="success" size="small" @click="openApprove(row, true)">同意</el-button>
          <el-button link type="danger" size="small" @click="openApprove(row, false)">拒绝</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div v-if="rows.length === 0 && !loading" style="text-align:center;color:#999;padding:40px 0">无数据</div>

    <!-- 审批对话框 -->
    <el-dialog v-model="approveVisible" :title="approveForm.approved ? '同意审批' : '拒绝审批'" width="480px" destroy-on-close>
      <el-form :model="approveForm" label-width="80px">
        <el-form-item label="审批意见">
          <el-input v-model="approveForm.comment" type="textarea" :rows="3" placeholder="请输入审批意见" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="approveVisible = false">取消</el-button>
        <el-button :type="approveForm.approved ? 'success' : 'danger'" @click="submitApprove" :loading="submitting">
          {{ approveForm.approved ? '同意' : '拒绝' }}
        </el-button>
      </template>
    </el-dialog>
  </el-card>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import api from '../../api/modules'
import { useAuthStore } from '../../stores/auth'

const auth = useAuthStore()
const rows = ref([])
const loading = ref(false)
const query = reactive({ keyword: '' })
const approveVisible = ref(false)
const approveForm = reactive({ taskId: 0, approved: true, comment: '' })
const submitting = ref(false)

async function load() {
  loading.value = true
  try {
    rows.value = await api.flowTodo({ approver: auth.displayName, ...query })
  } finally {
    loading.value = false
  }
}

function resetQuery() {
  query.keyword = ''
  load()
}

function openApprove(row, approved) {
  approveForm.taskId = row.id
  approveForm.approved = approved
  approveForm.comment = ''
  approveVisible.value = true
}

async function submitApprove() {
  submitting.value = true
  try {
    await api.flowApprove(approveForm.taskId, { approved: approveForm.approved, comment: approveForm.comment })
    ElMessage.success(approveForm.approved ? '已同意' : '已拒绝')
    approveVisible.value = false
    load()
  } finally {
    submitting.value = false
  }
}

function fmtDateTime(v) { return v ? String(v).replace('T', ' ').slice(0, 19) : '-' }

onMounted(load)
</script>

<style scoped>
.search-bar { margin-bottom: 12px; }
</style>
