<template>
  <CrudPage title="客户管理" path="/Customers"
    :columns="columns" :form-fields="formFields"
    :search-fields="[{ prop: 'keyword', label: '关键词' }]" />
</template>

<script setup>
import CrudPage from '../../components/CrudPage.vue'

const columns = [
  { prop: 'code', label: '客户编码', width: 90 },
  { prop: 'name', label: '客户名称' },
  { prop: 'contact', label: '联系人', width: 90 },
  { prop: 'phone', label: '电话', width: 130 },
  { prop: 'address', label: '地址', minWidth: 160 },
  { prop: 'remark', label: '备注', minWidth: 120 }
]

const formFields = [
  { prop: 'code', label: '客户编码', required: true },
  { prop: 'name', label: '客户名称', required: true },
  { prop: 'contact', label: '联系人' },
  { prop: 'phone', label: '电话' },
  { prop: 'address', label: '地址' },
  { prop: 'remark', label: '备注' }
]
</script>
