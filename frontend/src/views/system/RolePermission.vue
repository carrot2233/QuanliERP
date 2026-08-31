<template>
  <el-row :gutter="16">
    <el-col :span="10">
      <el-card shadow="never">
        <template #header>
          <div style="display:flex;align-items:center;justify-content:space-between">
            <span>角色列表</span>
            <el-button type="primary" size="small" @click="openCreate">新增角色</el-button>
          </div>
        </template>
        <el-table :data="roles" border stripe v-loading="loading" highlight-current-row @current-change="onSelectRole">
          <el-table-column prop="name" label="角色名称" width="110" align="center" />
          <el-table-column prop="code" label="编码" width="110" align="center">
            <template #default="{ row }"><el-tag size="small" :type="row.isBuiltIn ? 'warning' : 'info'">{{ row.code }}</el-tag></template>
          </el-table-column>
          <el-table-column prop="description" label="说明" min-width="120" align="center" show-overflow-tooltip />
          <el-table-column label="操作" width="120" align="center" fixed="right">
            <template #default="{ row }">
              <div class="op-btns">
                <el-button link type="primary" @click.stop="openEdit(row)">编辑</el-button>
                <span class="op-sep">|</span>
                <el-button link type="danger" :disabled="row.isBuiltIn" @click.stop="remove(row)">删除</el-button>
              </div>
            </template>
          </el-table-column>
        </el-table>
      </el-card>
    </el-col>
    <el-col :span="14">
      <el-card shadow="never">
        <template #header>
          <div style="display:flex;align-items:center;justify-content:space-between">
            <span>{{ current ? '角色权限配置：' + current.name : '角色权限配置' }}</span>
            <el-button type="primary" size="small" :disabled="!current || current.code === 'admin'" @click="savePermissions">保存权限</el-button>
          </div>
        </template>
        <el-alert v-if="!current" title="请先在左侧选择一个角色" type="info" :closable="false" style="margin-bottom:12px" />
        <el-alert v-else-if="current.code === 'admin'" title="管理员角色拥有全部菜单权限，无需配置" type="warning" :closable="false" style="margin-bottom:12px" />
        <template v-else>
          <el-tree
            ref="treeRef"
            :data="treeData"
            show-checkbox
            node-key="code"
            :props="{ label: 'title', children: 'children' }"
            default-expand-all
            style="border:1px solid #ebeef5;border-radius:4px;padding:8px"
          />
        </template>
      </el-card>
    </el-col>

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑角色' : '新增角色'" width="420px">
      <el-form :model="form" label-width="80px">
        <el-form-item label="角色名称" required><el-input v-model="form.name" /></el-form-item>
        <el-form-item label="角色编码" required>
          <el-input v-model="form.code" :disabled="editing" placeholder="如 finance" />
        </el-form-item>
        <el-form-item label="说明"><el-input v-model="form.description" type="textarea" :rows="2" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>
  </el-row>
</template>

<script setup>
import { ref, reactive, onMounted, nextTick } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '../../api/modules'

const roles = ref([])
const treeData = ref([])
const treeRef = ref(null)
const current = ref(null)
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref(false)
const form = reactive({})

async function load() {
  loading.value = true
  try {
    roles.value = await api.roles()
  } finally { loading.value = false }
}

async function loadMenus() {
  treeData.value = await api.roleMenus()
}

async function onSelectRole(row) {
  current.value = row
  await nextTick()
  if (row.code !== 'admin' && treeRef.value) {
    treeRef.value.setCheckedKeys(row.permissions || [])
  }
}

async function savePermissions() {
  const checked = treeRef.value ? treeRef.value.getCheckedKeys(true) : []
  await api.setRolePermissions(current.value.id, checked)
  ElMessage.success('权限已保存')
  load()
}

function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { name: '', code: '', description: '' })
  dialogVisible.value = true
}

function openEdit(row) {
  editing.value = true
  Object.assign(form, { ...row })
  dialogVisible.value = true
}

async function save() {
  if (!form.name || !form.code) return ElMessage.warning('请填写角色名称和编码')
  if (editing.value) {
    await api.updateRole(form.id, form)
    ElMessage.success('更新成功')
  } else {
    await api.createRole(form)
    ElMessage.success('新增成功')
  }
  dialogVisible.value = false
  load()
}

async function remove(row) {
  await ElMessageBox.confirm(`确定删除角色【${row.name}】？`, '提示', { type: 'warning' })
  await api.deleteRole(row.id)
  ElMessage.success('删除成功')
  current.value = null
  load()
}

onMounted(() => {
  load()
  loadMenus()
})
</script>

<style scoped>
.op-btns { display: inline-flex; align-items: center; gap: 0; white-space: nowrap; }
.op-sep { color: #dcdfe6; margin: 0 6px; font-weight: 300; user-select: none; }
.op-btns :deep(.el-button) { font-size: 14px; margin: 0; }
</style>
