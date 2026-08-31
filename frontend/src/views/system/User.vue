<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>用户管理</span>
        <el-button type="primary" @click="openCreate">新增用户</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="关键词">
        <el-input v-model="query.keyword" placeholder="用户名/姓名" clearable style="width:200px" @keyup.enter="load" />
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="load">查询</el-button>
      </el-form-item>
      <el-form-item><el-button @click="resetQuery">重置</el-button></el-form-item>
    </el-form>

    <el-table :data="displayRows" border stripe v-loading="loading">
      <el-table-column type="index" label="#" width="55" align="center" />
      <el-table-column prop="username" label="用户名" width="130" align="center" />
      <el-table-column prop="displayName" label="姓名" width="130" align="center" />
      <el-table-column prop="role" label="角色" width="110" align="center">
        <template #default="{ row }">
          <el-tag :type="roleTag[row.role] || 'info'" size="small">{{ roleName[row.role] || row.role }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="phone" label="电话" width="130" align="center" />
      <el-table-column prop="email" label="邮箱" min-width="150" align="center" />
      <el-table-column prop="isActive" label="状态" width="80" align="center">
        <template #default="{ row }">
          <el-tag :type="row.isActive ? 'success' : 'danger'" size="small">{{ row.isActive ? '启用' : '禁用' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="createdAt" label="创建时间" width="160" align="center" class-name="col-nowrap">
        <template #default="{ row }">{{ fmt(row.createdAt) }}</template>
      </el-table-column>
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

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑用户' : '新增用户'" width="480px">
      <el-form :model="form" label-width="90px">
        <el-form-item label="用户名" required>
          <el-input v-model="form.username" :disabled="editing" />
        </el-form-item>
        <el-form-item :label="editing ? '重置密码' : '密码'" required>
          <el-input v-model="form.password" type="password" show-password :placeholder="editing ? '留空则不修改' : '请输入密码'" />
        </el-form-item>
        <el-form-item label="姓名">
          <el-input v-model="form.displayName" />
        </el-form-item>
        <el-form-item label="角色">
          <el-select v-model="form.role" style="width:100%">
            <el-option v-for="(v, k) in roleName" :key="k" :label="v" :value="k" />
          </el-select>
        </el-form-item>
        <el-form-item label="电话">
          <el-input v-model="form.phone" />
        </el-form-item>
        <el-form-item label="邮箱">
          <el-input v-model="form.email" />
        </el-form-item>
        <el-form-item label="状态">
          <el-switch v-model="form.isActive" active-text="启用" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>
  </el-card>
</template>

<script setup>
import { computed, reactive, ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '../../api/modules'

const roleTag = { admin: 'danger', production: 'warning', warehouse: 'primary', quality: 'success', sales: 'info' }
const roles = ref([])

const rows = ref([])
import { usePagination } from '../../composables/usePagination'
const { currentPage, pageSize, pageSizes, total, displayRows, resetPage, handleSizeChange } = usePagination(rows)
const loading = ref(false)
const query = reactive({ keyword: '' })
const _initQuery = { ...query }
const dialogVisible = ref(false)
const editing = ref(false)
const form = reactive({})

const roleName = computed(() => {
  const map = {}
  roles.value.forEach(r => { map[r.code] = r.name })
  return map
})

async function load() {
  loading.value = true
  try { rows.value = await api.users({ keyword: query.keyword }) } finally { loading.value = false }
  try { roles.value = await api.roles() } catch { /* ignore */ }
}

function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { role: 'production', isActive: true, password: '' })
  dialogVisible.value = true
}

function openEdit(row) {
  editing.value = true
  Object.assign(form, { ...row, password: '******' })
  dialogVisible.value = true
}

async function save() {
  if (!form.username) return ElMessage.warning('请输入用户名')
  if (!editing.value && !form.password) return ElMessage.warning('请输入密码')
  if (editing.value) {
    await api.updateUser(form.id, form)
    ElMessage.success('更新成功')
  } else {
    await api.createUser(form)
    ElMessage.success('新增成功')
  }
  dialogVisible.value = false
  load()
}

async function remove(row) {
  await ElMessageBox.confirm('确定删除该用户？', '提示', { type: 'warning' })
  await api.deleteUser(row.id)
  ElMessage.success('删除成功')
  load()
}

function fmt(v) { return v ? String(v).replace('T', ' ').slice(0, 19) : '-' }
function resetQuery() { Object.assign(query, { ..._initQuery }); resetPage(); load() }

onMounted(load)
</script>

<style scoped>
.pagination-wrap { display: flex; justify-content: flex-end; margin-top: 12px; }
:deep(.col-nowrap .cell) { white-space: nowrap !important; overflow: hidden !important; text-overflow: unset !important; }
.op-btns { display: inline-flex; align-items: center; gap: 0; white-space: nowrap; }
.op-sep { color: #dcdfe6; margin: 0 6px; font-weight: 300; user-select: none; }
.op-btns :deep(.el-button) { font-size: 14px; margin: 0; }
</style>
