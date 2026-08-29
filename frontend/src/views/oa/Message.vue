<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>消息中心</span>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="筛选">
        <el-radio-group v-model="filter" @change="load">
          <el-radio-button label="">全部</el-radio-button>
          <el-radio-button label="unread">未读</el-radio-button>
          <el-radio-button label="read">已读</el-radio-button>
          <el-radio-button label="starred">星标</el-radio-button>
        </el-radio-group>
      </el-form-item>
      <el-form-item label="关键词">
        <el-input v-model="query.keyword" placeholder="内容/类型" clearable style="width:180px" @keyup.enter="load" />
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="load">查询</el-button>
      </el-form-item>
    </el-form>

    <el-table :data="displayRows" border stripe v-loading="loading" size="default" :row-class-name="rowClassName">
      <el-table-column label="置顶" width="70" align="center">
        <template #default="{ row }">
          <el-icon v-if="row.isPinned" color="#f56c6c" :size="16"><Top /></el-icon>
        </template>
      </el-table-column>
      <el-table-column label="星标" width="70" align="center">
        <template #default="{ row }">
          <el-icon :color="row.isStarred ? '#e6a23c' : '#c0c4cc'" :size="16" style="cursor:pointer" @click="toggleStar(row)">
            <StarFilled v-if="row.isStarred" /><Star v-else />
          </el-icon>
        </template>
      </el-table-column>
      <el-table-column label="状态" width="80" align="center">
        <template #default="{ row }">
          <el-tag v-if="row.isRead" type="info" size="small">已读</el-tag>
          <el-tag v-else type="danger" size="small" effect="dark">未读</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="msgType" label="类型" width="100" align="center">
        <template #default="{ row }">
          <el-tag :type="typeColor(row.msgType)" size="small">{{ row.msgType }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="content" label="内容" min-width="280" show-overflow-tooltip>
        <template #default="{ row }">
          <span :class="{ unread: !row.isRead }">{{ row.content }}</span>
        </template>
      </el-table-column>
      <el-table-column prop="creator" label="发送人" width="100" align="center" />
      <el-table-column prop="createdAt" label="时间" width="160" align="center">
        <template #default="{ row }">{{ fmtDateTime(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="190" align="center" fixed="right">
        <template #default="{ row }">
          <el-button v-if="!row.isRead" link type="primary" size="small" @click="markRead(row)">标为已读</el-button>
          <el-button link type="warning" size="small" @click="togglePin(row)">{{ row.isPinned ? '取消置顶' : '置顶' }}</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div v-if="rows.length === 0 && !loading" style="text-align:center;color:#999;padding:40px 0">暂无消息</div>

    <div class="pagination-wrap">
      <el-pagination background
        v-model:current-page="currentPage" v-model:page-size="pageSize"
        :page-sizes="pageSizes" :total="total" :small="true"
        layout="total, sizes, prev, pager, next" @size-change="handleSizeChange" @current-change="() => {}" />
    </div>
  </el-card>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Star, StarFilled, Top } from '@element-plus/icons-vue'
import api from '../../api/modules'
import { useAuthStore } from '../../stores/auth'
import { usePagination } from '../../composables/usePagination'

const auth = useAuthStore()
const rows = ref([])
const loading = ref(false)
const filter = ref('')
const query = reactive({ keyword: '' })
const { currentPage, pageSize, pageSizes, total, displayRows, resetPage, handleSizeChange } = usePagination(rows)

async function load() {
  loading.value = true
  try {
    const params = { recipient: auth.displayName, filter: filter.value, ...query }
    rows.value = await api.messages(params)
  } finally {
    loading.value = false
  }
}

async function markRead(row) {
  await api.messageRead(row.id)
  ElMessage.success('已标记为已读')
  load()
}

async function toggleStar(row) {
  await api.messageStar(row.id)
  load()
}

async function togglePin(row) {
  await api.messagePin(row.id)
  ElMessage.success(row.isPinned ? '已取消置顶' : '已置顶')
  load()
}

async function remove(row) {
  await ElMessageBox.confirm('确定删除该消息？', '提示', { type: 'warning' })
  await api.deleteMessage(row.id)
  ElMessage.success('删除成功')
  load()
}

function rowClassName({ row }) {
  return row.isPinned ? 'pinned-row' : ''
}

function typeColor(t) {
  return { 系统消息: 'info', 审批消息: 'success', 待办消息: 'warning' }[t] || 'primary'
}

function fmtDateTime(v) { return v ? String(v).replace('T', ' ').slice(0, 19) : '-' }

onMounted(load)
</script>

<style scoped>
.search-bar { margin-bottom: 12px; }
.unread { font-weight: bold; }
.pagination-wrap { display: flex; justify-content: flex-end; margin-top: 12px; }
:deep(.pinned-row) { background: #fef0f0 !important; }
</style>
