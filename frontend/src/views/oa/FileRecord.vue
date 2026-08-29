<template>
  <CrudPage title="文件管理" path="/FileRecords"
    :columns="columns" :form-fields="formFields"
    :search-fields="[{ prop: 'keyword', label: '关键词' }]">
    <template #actions="{ row }">
      <el-button link type="success" size="small" @click="download(row)" :disabled="!row.hasAttachment">下载</el-button>
    </template>
  </CrudPage>
</template>

<script setup>
import { ElMessage } from 'element-plus'
import CrudPage from '../../components/CrudPage.vue'
import api from '../../api/modules'

const columns = [
  { prop: 'fileName', label: '文件名称', minWidth: 200 },
  { prop: 'fileType', label: '文件类型', width: 90 },
  { prop: 'category', label: '文件所属', width: 100 },
  { prop: 'deptName', label: '所属部门', width: 110 },
  { prop: 'status', label: '状态', width: 80, type: 'tag' },
  { prop: 'hasAttachment', label: '附件', width: 80 },
  { prop: 'createdAt', label: '创建时间', width: 160, type: 'datetime' },
  { prop: 'creator', label: '创建用户', width: 100 },
  { prop: 'remark', label: '文件备注', minWidth: 150 }
]

const formFields = [
  { prop: 'fileName', label: '文件名称', required: true },
  { prop: 'fileType', label: '文件类型', type: 'select', options: ['图片', '文档', '表格', '视频', '其他'].map(v => ({ label: v, value: v })) },
  { prop: 'category', label: '文件所属' },
  { prop: 'deptName', label: '所属部门' },
  { prop: 'status', label: '状态', type: 'select', options: ['有效', '无效'].map(v => ({ label: v, value: v })) },
  { prop: 'creator', label: '创建用户' },
  { prop: 'remark', label: '文件备注' },
  { prop: 'attachment', label: '附件', type: 'file' }
]

async function download(row) {
  const blob = await api.fileDownload(row.id)
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = row.fileName
  a.click()
  URL.revokeObjectURL(url)
  ElMessage.success('下载成功')
}
</script>
