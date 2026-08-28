<template>
  <CrudPage title="员工档案" path="/Employees"
    :columns="columns" :form-fields="formFields"
    :search-fields="[{ prop: 'keyword', label: '关键词' }]" />
</template>

<script setup>
import CrudPage from '../../components/CrudPage.vue'

const columns = [
  { prop: 'code', label: '工号', width: 100, className: 'col-nowrap' },
  { prop: 'name', label: '姓名', width: 90 },
  { prop: 'dept', label: '部门', width: 100 },
  { prop: 'position', label: '岗位', width: 110 },
  { prop: 'status', label: '状态', width: 80, type: 'tag' },
  { prop: 'createdAt', label: '创建时间', width: 160, type: 'datetime' }
]

const formFields = [
  { prop: 'code', label: '工号', required: true },
  { prop: 'name', label: '姓名', required: true },
  { prop: 'gender', label: '性别', type: 'select', options: ['男', '女'].map(v => ({ label: v, value: v })) },
  { prop: 'dept', label: '部门' },
  { prop: 'position', label: '岗位' },
  { prop: 'phone', label: '电话' },
  { prop: 'status', label: '状态', type: 'select', options: ['在职', '离职', '试用', '请假'].map(v => ({ label: v, value: v })) },
  { prop: 'hireDate', label: '入职日期', type: 'date' },
  { prop: 'remark', label: '备注' }
]
</script>
