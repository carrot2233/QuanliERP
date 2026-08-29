<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>{{ title }}</span>
        <el-button type="primary" size="small" @click="openCreate">新增</el-button>
      </div>
    </template>

    <el-form v-if="searchFields.length" :inline="true" class="search-bar">
      <el-form-item v-for="f in searchFields" :key="f.prop" :label="f.label">
        <el-input v-model="query[f.prop]" :placeholder="'请输入' + f.label" clearable style="width:180px" @keyup.enter="load" />
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="load">查询</el-button>
        <el-button @click="resetQuery">重置</el-button>
      </el-form-item>
    </el-form>

    <el-table :data="displayRows" border stripe v-loading="loading" size="default">
      <el-table-column type="index" label="#" width="55" align="center" />
      <el-table-column v-for="c in columns" :key="c.prop" :prop="c.prop" :label="c.label"
        :width="c.width" :min-width="c.minWidth" :class-name="c.className" align="center" show-overflow-tooltip>
        <template v-if="c.type === 'tag'" #default="{ row }">
          <el-tag :type="tagType(row[c.prop])" size="small">{{ row[c.prop] || '-' }}</el-tag>
        </template>
        <template v-else-if="c.type === 'money'" #default="{ row }">
          {{ fmtMoney(row[c.prop]) }}
        </template>
        <template v-else-if="c.type === 'datetime'" #default="{ row }">
          {{ fmtDateTime(row[c.prop]) }}
        </template>
        <template v-else-if="c.type === 'date'" #default="{ row }">
          {{ fmtDate(row[c.prop]) }}
        </template>
      </el-table-column>
      <el-table-column label="操作" width="150" align="center" fixed="right">
        <template #default="{ row }">
          <slot name="actions" :row="row" />
          <el-button link type="primary" size="small" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div class="pagination-wrap">
      <el-pagination background
        v-model:current-page="currentPage" v-model:page-size="pageSize"
        :page-sizes="pageSizes" :total="total" :small="true"
        layout="total, sizes, prev, pager, next" @size-change="handleSizeChange" @current-change="() => {}" />
    </div>

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑' : '新增'" width="640px" destroy-on-close>
      <el-form :model="form" label-width="110px">
        <el-row :gutter="10">
          <el-col :span="12" v-for="f in formFields" :key="f.prop">
            <el-form-item :label="f.label" :required="f.required">
              <el-select v-if="f.type === 'select'" v-model="form[f.prop]" placeholder="请选择" clearable style="width:100%">
                <el-option v-for="o in f.options || []" :key="o.value" :label="o.label" :value="o.value" />
              </el-select>
              <el-date-picker v-else-if="f.type === 'date'" v-model="form[f.prop]" type="date" value-format="YYYY-MM-DD" style="width:100%" />
              <el-date-picker v-else-if="f.type === 'datetime'" v-model="form[f.prop]" type="datetime" value-format="YYYY-MM-DDTHH:mm:ss" style="width:100%" />
              <el-input-number v-else-if="f.type === 'number'" v-model="form[f.prop]" style="width:100%" :controls="false" />
              <el-upload v-else-if="f.type === 'file'" :auto-upload="false" :limit="1" :on-change="(file, fl) => onFileChange(f, file, fl)" :on-remove="() => onFileRemove(f)">
                <el-button size="small">选择附件</el-button>
                <template #tip>
                  <div class="el-upload__tip">支持上传单个附件</div>
                </template>
              </el-upload>
              <el-input v-else v-model="form[f.prop]" :placeholder="'请输入' + f.label" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>
  </el-card>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '../api/modules'
import { usePagination } from '../composables/usePagination'

const props = defineProps({
  title: { type: String, required: true },
  path: { type: String, required: true },
  columns: { type: Array, default: () => [] },
  formFields: { type: Array, default: () => [] },
  searchFields: { type: Array, default: () => [] },
  defaults: { type: Object, default: () => ({}) }
})

const rows = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref(false)
const query = reactive({})
const form = reactive({})
const fileMap = reactive({})
const { currentPage, pageSize, pageSizes, total, displayRows, resetPage, handleSizeChange } = usePagination(rows)

async function load() {
  loading.value = true
  try {
    rows.value = await api.crud.list(props.path, { ...query })
  } finally {
    loading.value = false
  }
}

function resetQuery() {
  Object.keys(query).forEach(k => delete query[k])
  resetPage()
  load()
}

function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.keys(fileMap).forEach(k => delete fileMap[k])
  Object.assign(form, JSON.parse(JSON.stringify(props.defaults)))
  dialogVisible.value = true
}

function openEdit(row) {
  editing.value = true
  Object.keys(form).forEach(k => delete form[k])
  Object.keys(fileMap).forEach(k => delete fileMap[k])
  Object.assign(form, JSON.parse(JSON.stringify(row)))
  dialogVisible.value = true
}

function onFileChange(field, file) {
  fileMap[field.prop] = file.raw
}

function onFileRemove(field) {
  delete fileMap[field.prop]
}

async function save() {
  const hasFile = props.formFields.some(f => f.type === 'file')
  if (hasFile) {
    const fd = new FormData()
    Object.keys(form).forEach(k => {
      if (form[k] != null) fd.append(k, form[k])
    })
    Object.keys(fileMap).forEach(k => fd.append(k, fileMap[k]))
    if (editing.value) {
      await api.crud.update(props.path, form.id, fd)
      ElMessage.success('更新成功')
    } else {
      await api.crud.create(props.path, fd)
      ElMessage.success('新增成功')
    }
  } else {
    if (editing.value) {
      await api.crud.update(props.path, form.id, form)
      ElMessage.success('更新成功')
    } else {
      await api.crud.create(props.path, form)
      ElMessage.success('新增成功')
    }
  }
  dialogVisible.value = false
  load()
}

async function remove(row) {
  await ElMessageBox.confirm(`确定删除该记录？`, '提示', { type: 'warning' })
  await api.crud.remove(props.path, row.id)
  ElMessage.success('删除成功')
  load()
}

function tagType(v) {
  const map = { 正常: 'success', 预警: 'warning', 缺货: 'danger', 运行: 'success', 维修: 'warning', 停机: 'danger', 完成: 'success', 已发货: 'success' }
  return map[v] || 'info'
}

function fmtMoney(v) { return v == null ? '-' : Number(v).toFixed(2) }
function fmtDateTime(v) { return v ? String(v).replace('T', ' ').slice(0, 19) : '-' }
function fmtDate(v) { return v ? String(v).slice(0, 10) : '-' }

onMounted(load)
</script>

<style scoped>
.search-bar { margin-bottom: 12px; }
:deep(.col-nowrap .cell) { white-space: nowrap !important; overflow: hidden !important; text-overflow: unset !important; }
.pagination-wrap { display: flex; justify-content: flex-end; margin-top: 12px; }
</style>
