<template>
  <CrudPage title="产品管理（冲压件）" path="/Products"
    :columns="columns" :form-fields="formFields"
    :search-fields="[{ prop: 'keyword', label: '关键词' }]" />
</template>

<script setup>
import CrudPage from '../../components/CrudPage.vue'

const columns = [
  { prop: 'code', label: '零件编号', width: 130, className: 'col-nowrap' },
  { prop: 'name', label: '产品名称', minWidth: 180 },
  { prop: 'specification', label: '规格尺寸', minWidth: 140 },
  { prop: 'material', label: '材质', width: 110 },
  { prop: 'custPartNo', label: '客户零件号', width: 130, className: 'col-nowrap' },
  { prop: 'moldNo', label: '制号/模具', width: 130, className: 'col-nowrap' },
  { prop: 'oneOutputs', label: '1出几', width: 70 },
  { prop: 'processRoute', label: '工序路线', minWidth: 160 },
  { prop: 'salePrice', label: '销售价', width: 90, type: 'money' }
]

const formFields = [
  { prop: 'code', label: '零件编号', required: true },
  { prop: 'name', label: '产品名称', required: true },
  { prop: 'specification', label: '规格尺寸' },
  { prop: 'material', label: '材质' },
  { prop: 'custPartNo', label: '客户零件号' },
  { prop: 'moldNo', label: '制号/模具号' },
  { prop: 'unit', label: '单位' },
  { prop: 'oneOutputs', label: '1出几', type: 'number' },
  { prop: 'processRoute', label: '工序路线' },
  { prop: 'standardHours', label: '标准工时', type: 'number' },
  { prop: 'salePrice', label: '销售单价', type: 'number' },
  { prop: 'remark', label: '备注' }
]
</script>
