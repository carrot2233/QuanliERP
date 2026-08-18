<template>
  <CrudPage title="供应商管理" path="/Suppliers"
    :columns="columns" :form-fields="formFields"
    :search-fields="[{ prop: 'keyword', label: '关键词' }]" />
</template>

<script setup>
import CrudPage from '../../components/CrudPage.vue'

const columns = [
  { prop: 'code', label: '供应商编码', width: 130, className: 'col-nowrap' },
  { prop: 'name', label: '供应商名称' },
  { prop: 'contact', label: '联系人', width: 90 },
  { prop: 'phone', label: '电话', width: 130 },
  { prop: 'paymentTerms', label: '结算方式', width: 100 },
  { prop: 'address', label: '地址', minWidth: 160 },
  { prop: 'remark', label: '备注', minWidth: 120 }
]

const formFields = [
  { prop: 'code', label: '供应商编码', required: true },
  { prop: 'name', label: '供应商名称', required: true },
  { prop: 'contact', label: '联系人' },
  { prop: 'phone', label: '电话' },
  { prop: 'paymentTerms', label: '结算方式' },
  { prop: 'address', label: '地址' },
  { prop: 'remark', label: '备注' }
]
</script>
