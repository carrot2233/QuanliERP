<template>
  <CrudPage title="原材料管理" path="/Materials"
    :columns="columns" :form-fields="formFields"
    :search-fields="[{ prop: 'keyword', label: '关键词' }]" />
</template>

<script setup>
import CrudPage from '../../components/CrudPage.vue'

const columns = [
  { prop: 'code', label: '编码', width: 80 },
  { prop: 'name', label: '材料名称' },
  { prop: 'specification', label: '规格尺寸', minWidth: 170 },
  { prop: 'materialType', label: '材质', width: 110 },
  { prop: 'category', label: '类别', width: 90 },
  { prop: 'unit', label: '单位', width: 60 },
  { prop: 'unitPrice', label: '单价', width: 80, type: 'money' },
  { prop: 'minStock', label: '安全库存', width: 80 },
  { prop: 'remark', label: '备注', minWidth: 120 }
]

const formFields = [
  { prop: 'code', label: '材料编码', required: true },
  { prop: 'name', label: '材料名称', required: true },
  { prop: 'specification', label: '规格尺寸' },
  { prop: 'materialType', label: '材质' },
  { prop: 'category', label: '类别', type: 'select', options: ['板材', '刀具材料', '量具', '辅料'].map(v => ({ label: v, value: v })) },
  { prop: 'unit', label: '单位' },
  { prop: 'unitPrice', label: '单价', type: 'number' },
  { prop: 'minStock', label: '安全库存', type: 'number' },
  { prop: 'remark', label: '备注' }
]
</script>
