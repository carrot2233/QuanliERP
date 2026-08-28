<template>
  <CrudPage title="消息中心" path="/Messages"
    :columns="columns" :form-fields="formFields"
    :search-fields="[{ prop: 'keyword', label: '关键词' }]" />
</template>

<script setup>
import CrudPage from '../../components/CrudPage.vue'

const columns = [
  { prop: 'msgType', label: '信息类型', width: 100, type: 'tag' },
  { prop: 'recipient', label: '收件人', width: 100 },
  { prop: 'content', label: '内容', minWidth: 300 },
  { prop: 'createdAt', label: '创建时间', width: 160, type: 'datetime' },
  { prop: 'creator', label: '创建用户', width: 100 }
]

const formFields = [
  { prop: 'msgType', label: '信息类型', type: 'select', options: ['系统消息', '审批消息', '待办消息'].map(v => ({ label: v, value: v })) },
  { prop: 'recipient', label: '收件人', required: true },
  { prop: 'content', label: '内容', required: true },
  { prop: 'creator', label: '创建用户' }
]
</script>
