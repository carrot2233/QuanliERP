<template>
  <CrudPage title="通知公告" path="/Notices"
    :columns="columns" :form-fields="formFields"
    :search-fields="[{ prop: 'keyword', label: '关键词' }]" />
</template>

<script setup>
import CrudPage from '../../components/CrudPage.vue'

const columns = [
  { prop: 'title', label: '标题', minWidth: 180 },
  { prop: 'content', label: '内容', minWidth: 260 },
  { prop: 'status', label: '状态', width: 80, type: 'tag' },
  { prop: 'creator', label: '创建人', width: 100 },
  { prop: 'createdAt', label: '创建时间', width: 160, type: 'datetime' }
]

const formFields = [
  { prop: 'title', label: '标题', required: true },
  { prop: 'content', label: '内容', required: true },
  { prop: 'status', label: '状态', type: 'select', options: ['有效', '无效'].map(v => ({ label: v, value: v })) },
  { prop: 'creator', label: '创建人' }
]
</script>
