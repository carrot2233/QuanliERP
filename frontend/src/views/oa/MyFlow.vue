<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>我的流程</span>
        <el-button type="primary" size="small" @click="openApply">申请</el-button>
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
      <el-table-column prop="flowStatus" label="流程状态" width="100" align="center">
        <template #default="{ row }">
          <el-tag :type="statusType(row.flowStatus)" size="small">{{ row.flowStatus }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="instanceNo" label="实例编号" width="170" align="center" class-name="col-nowrap" />
      <el-table-column prop="instanceName" label="实例名称" min-width="180" show-overflow-tooltip />
      <el-table-column prop="currentNode" label="当前节点名称" width="120" align="center" />
      <el-table-column prop="remark" label="实例备注" min-width="150" show-overflow-tooltip />
      <el-table-column prop="createdAt" label="创建时间" width="160" align="center">
        <template #default="{ row }">{{ fmtDateTime(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column prop="creator" label="创建人" width="100" align="center" />
      <el-table-column label="操作" width="150" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="viewDetail(row)">详情</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 发起流程对话框 -->
    <el-dialog v-model="applyVisible" title="发起流程" width="560px" destroy-on-close>
      <el-form :model="applyForm" label-width="100px">
        <el-form-item label="流程定义" required>
          <el-select v-model="applyForm.flowDesignId" placeholder="请选择流程" style="width:100%">
            <el-option v-for="d in flowDesigns" :key="d.id" :label="d.flowName" :value="d.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="实例名称">
          <el-input v-model="applyForm.instanceName" placeholder="留空则自动生成" />
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="applyForm.remark" type="textarea" :rows="3" placeholder="请输入备注" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="applyVisible = false">取消</el-button>
        <el-button type="primary" @click="submitApply" :loading="submitting">提交</el-button>
      </template>
    </el-dialog>

    <!-- 流程详情对话框 -->
    <el-dialog v-model="detailVisible" title="流程详情" width="700px" destroy-on-close>
      <el-descriptions :column="2" border size="small" style="margin-bottom:16px">
        <el-descriptions-item label="实例编号">{{ detail.instanceNo }}</el-descriptions-item>
        <el-descriptions-item label="实例名称">{{ detail.instanceName }}</el-descriptions-item>
        <el-descriptions-item label="流程状态">
          <el-tag :type="statusType(detail.flowStatus)" size="small">{{ detail.flowStatus }}</el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="当前节点">{{ detail.currentNode }}</el-descriptions-item>
        <el-descriptions-item label="创建人">{{ detail.creator }}</el-descriptions-item>
        <el-descriptions-item label="创建时间">{{ fmtDateTime(detail.createdAt) }}</el-descriptions-item>
        <el-descriptions-item label="备注" :span="2">{{ detail.remark || '-' }}</el-descriptions-item>
      </el-descriptions>

      <div style="font-weight:bold;margin-bottom:8px">审批记录</div>
      <el-table :data="detail.tasks || []" border size="small">
        <el-table-column type="index" label="#" width="50" align="center" />
        <el-table-column prop="nodeName" label="节点名称" width="130" align="center" />
        <el-table-column prop="approver" label="审批人" width="100" align="center" />
        <el-table-column prop="status" label="状态" width="90" align="center">
          <template #default="{ row }">
            <el-tag :type="taskStatusType(row.status)" size="small">{{ row.status }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="comment" label="审批意见" min-width="120" show-overflow-tooltip />
        <el-table-column prop="handledAt" label="处理时间" width="160" align="center">
          <template #default="{ row }">{{ fmtDateTime(row.handledAt) }}</template>
        </el-table-column>
      </el-table>
    </el-dialog>
  </el-card>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '../../api/modules'
import { useAuthStore } from '../../stores/auth'

const auth = useAuthStore()
const rows = ref([])
const loading = ref(false)
const query = reactive({ keyword: '' })
const flowDesigns = ref([])
const applyVisible = ref(false)
const applyForm = reactive({ flowDesignId: null, instanceName: '', remark: '' })
const submitting = ref(false)
const detailVisible = ref(false)
const detail = ref({})

async function load() {
  loading.value = true
  try {
    rows.value = await api.flowMy({ creator: auth.displayName, ...query })
  } finally {
    loading.value = false
  }
}

function resetQuery() {
  query.keyword = ''
  load()
}

async function openApply() {
  applyForm.flowDesignId = null
  applyForm.instanceName = ''
  applyForm.remark = ''
  if (flowDesigns.value.length === 0) {
    flowDesigns.value = await api.flowDesigns()
  }
  applyVisible.value = true
}

async function submitApply() {
  if (!applyForm.flowDesignId) { ElMessage.warning('请选择流程'); return }
  submitting.value = true
  try {
    await api.createFlowInstance({ ...applyForm, creator: auth.displayName })
    ElMessage.success('流程已发起')
    applyVisible.value = false
    load()
  } finally {
    submitting.value = false
  }
}

async function viewDetail(row) {
  detail.value = await api.flowInstance(row.id)
  detailVisible.value = true
}

async function remove(row) {
  await ElMessageBox.confirm('确定删除该流程实例？', '提示', { type: 'warning' })
  await api.deleteFlowInstance(row.id)
  ElMessage.success('删除成功')
  load()
}

function statusType(s) {
  return { 审批中: 'warning', 审批通过: 'success', 审批拒绝: 'danger' }[s] || 'info'
}
function taskStatusType(s) {
  return { 待处理: 'warning', 已同意: 'success', 已拒绝: 'danger' }[s] || 'info'
}
function fmtDateTime(v) { return v ? String(v).replace('T', ' ').slice(0, 19) : '-' }

onMounted(load)
</script>

<style scoped>
.search-bar { margin-bottom: 12px; }
</style>
