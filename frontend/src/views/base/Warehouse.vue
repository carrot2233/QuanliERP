<template>
  <CrudPage title="仓库管理" path="/Warehouses"
    :columns="columns" :form-fields="formFields" />
</template>

<script setup>
import CrudPage from '../../components/CrudPage.vue'

const columns = [
  { prop: 'code', label: '仓库编码', width: 130, className: 'col-nowrap' },
  { prop: 'name', label: '仓库名称' },
  { prop: 'location', label: '位置', minWidth: 150 },
  { prop: 'manager', label: '负责人', width: 100 },
  { prop: 'remark', label: '备注', minWidth: 120 }
]

const formFields = [
  { prop: 'code', label: '仓库编码', required: true },
  { prop: 'name', label: '仓库名称', required: true },
  { prop: 'location', label: '位置' },
  { prop: 'manager', label: '负责人' },
  { prop: 'remark', label: '备注' }
]
</script>
