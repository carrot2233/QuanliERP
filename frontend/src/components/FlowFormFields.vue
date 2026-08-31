<template>
  <div>
    <!-- 表单字段渲染 -->
    <el-form v-if="mode === 'fill'" :model="modelValue" label-width="110px">
      <el-form-item v-for="f in fields" :key="f.key" :label="f.label" :required="f.required">
        <el-select v-if="f.type === 'select'" v-model="modelValue[f.key]" placeholder="请选择" style="width:100%">
          <el-option v-for="o in f.options || []" :key="o" :label="o" :value="o" />
        </el-select>
        <el-date-picker v-else-if="f.type === 'date'" v-model="modelValue[f.key]" type="date" value-format="YYYY-MM-DD" style="width:100%" placeholder="请选择日期" />
        <el-date-picker v-else-if="f.type === 'datetime'" v-model="modelValue[f.key]" type="datetime" value-format="YYYY-MM-DDTHH:mm:ss" style="width:100%" placeholder="请选择日期时间" />
        <el-time-picker v-else-if="f.type === 'time'" v-model="modelValue[f.key]" value-format="HH:mm" style="width:100%" placeholder="请选择时间" />
        <el-input-number v-else-if="f.type === 'number'" v-model="modelValue[f.key]" style="width:100%" :controls="false" />
        <el-input v-else-if="f.type === 'textarea'" v-model="modelValue[f.key]" type="textarea" :rows="3" placeholder="请输入" />
        <el-input v-else v-model="modelValue[f.key]" placeholder="请输入" />
      </el-form-item>
    </el-form>

    <!-- 只读展示 -->
    <el-descriptions v-else-if="mode === 'view'" :column="1" border size="small">
      <el-descriptions-item v-for="f in fields" :key="f.key" :label="f.label">
        {{ viewValue(f, modelValue[f.key]) }}
      </el-descriptions-item>
    </el-descriptions>

    <!-- 字段编辑器（流程设计中配置表单模板） -->
    <div v-if="mode === 'edit'">
      <div v-if="fields.length === 0" style="color:#999;font-size:13px;padding:12px 0">
        当前表单暂无字段，请添加字段以构成申请表模板。
      </div>
      <div v-for="(f, idx) in fields" :key="idx" class="field-row">
        <el-input v-model="f.label" placeholder="字段名称（如：请假天数）" style="width:140px" size="small" />
        <el-select v-model="f.type" style="width:120px" size="small">
          <el-option label="单行文本" value="text" />
          <el-option label="多行文本" value="textarea" />
          <el-option label="数字" value="number" />
          <el-option label="下拉选择" value="select" />
          <el-option label="日期" value="date" />
          <el-option label="日期时间" value="datetime" />
          <el-option label="时间" value="time" />
        </el-select>
        <el-input v-if="f.type === 'select'" v-model="f.optionsText" placeholder="选项，用逗号分隔" style="width:200px" size="small" />
        <el-checkbox v-model="f.required">必填</el-checkbox>
        <el-button link type="danger" size="small" @click="fields.splice(idx, 1)">删除</el-button>
      </div>
      <el-button size="small" type="primary" plain style="margin-top:8px" @click="addField">+ 添加字段</el-button>
    </div>
  </div>
</template>

<script setup>
import { watch } from 'vue'

const props = defineProps({
  mode: { type: String, default: 'fill' }, // fill / view / edit
  modelValue: { type: Object, default: () => ({}) }, // fill: 表单值; view: 表单数据对象
  fields: { type: Array, default: () => [] } // edit/fill/view: 字段定义（edit 直接修改此数组）
})

// 编辑模式下把每个字段补上 optionsText（用于选项输入框的双向绑定），
// 保存时由 getFields 汇总成后端格式。
function ensureOptionsText() {
  ;(props.fields || []).forEach(f => {
    if (f && f.options && f.optionsText == null) f.optionsText = f.options.join(',')
  })
}

function addField() {
  const key = 'field' + Date.now() + Math.floor(Math.random() * 100)
  props.fields.push({ key, label: '', type: 'text', required: false, options: [], optionsText: '' })
}

function viewValue(f, v) {
  if (v == null || v === '') return '-'
  return v
}

// 供父组件在保存时收集字段（编辑模式）
function collectFields() {
  return (props.fields || []).map(f => ({
    key: f.key || 'field' + Date.now() + Math.floor(Math.random() * 100),
    label: f.label,
    type: f.type,
    required: !!f.required,
    options: f.type === 'select' ? (f.optionsText || '').split(/[,，]/).map(s => s.trim()).filter(Boolean) : []
  }))
}

defineExpose({ collectFields })

watch(() => props.fields, ensureOptionsText, { deep: true, immediate: true })
</script>

<style scoped>
.field-row { display: flex; align-items: center; gap: 8px; margin-bottom: 8px; flex-wrap: wrap; }
</style>
