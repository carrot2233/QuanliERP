<template>
  <CrudPage title="流程设计" path="/FlowDesigns"
    :columns="columns" :form-fields="formFields"
    :search-fields="[{ prop: 'keyword', label: '关键词' }]"
    :defaults="{ status: '有效', sort: 1 }" />
</template>

<script setup>
import CrudPage from '../../components/CrudPage.vue'

const columns = [
  { prop: 'flowNo', label: '流程编号', width: 160, className: 'col-nowrap' },
  { prop: 'flowName', label: '流程名称', minWidth: 120 },
  { prop: 'remark', label: '备注', minWidth: 150 },
  { prop: 'sort', label: '排序', width: 70 },
  { prop: 'status', label: '状态', width: 80, type: 'tag' },
  { prop: 'deptName', label: '所属部门', width: 100 },
  { prop: 'nodeCount', label: '节点数', width: 80 }
]

const formFields = [
  { prop: 'flowNo', label: '流程编号' },
  { prop: 'flowName', label: '流程名称', required: true },
  { prop: 'remark', label: '备注' },
  { prop: 'sort', label: '排序', type: 'number' },
  { prop: 'status', label: '状态', type: 'select', options: ['有效', '无效'].map(v => ({ label: v, value: v })) },
  { prop: 'deptName', label: '所属部门' }
]
</script>
