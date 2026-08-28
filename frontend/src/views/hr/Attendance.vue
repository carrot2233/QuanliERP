<template>
  <CrudPage title="考勤管理" path="/Attendances"
    :columns="columns" :form-fields="formFields"
    :search-fields="[{ prop: 'keyword', label: '关键词' }]" />
</template>

<script setup>
import CrudPage from '../../components/CrudPage.vue'

const columns = [
  { prop: 'empCode', label: '工号', width: 100, className: 'col-nowrap' },
  { prop: 'empName', label: '姓名', width: 90 },
  { prop: 'attendDate', label: '考勤日期', width: 110, type: 'date' },
  { prop: 'checkIn', label: '上班打卡', width: 90 },
  { prop: 'checkOut', label: '下班打卡', width: 90 },
  { prop: 'status', label: '状态', width: 80, type: 'tag' },
  { prop: 'remark', label: '备注', minWidth: 120 },
  { prop: 'createdAt', label: '创建时间', width: 160, type: 'datetime' }
]

const formFields = [
  { prop: 'empCode', label: '工号', required: true },
  { prop: 'empName', label: '姓名', required: true },
  { prop: 'attendDate', label: '考勤日期', type: 'date', required: true },
  { prop: 'checkIn', label: '上班打卡' },
  { prop: 'checkOut', label: '下班打卡' },
  { prop: 'status', label: '状态', type: 'select', options: ['正常', '迟到', '早退', '缺勤', '请假'].map(v => ({ label: v, value: v })) },
  { prop: 'remark', label: '备注' }
]
</script>
