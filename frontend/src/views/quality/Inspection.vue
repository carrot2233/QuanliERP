<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>质检记录</span>
        <el-button type="primary" size="small" @click="openCreate">新增质检记录</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="关键词"><el-input v-model="query.keyword" clearable style="width:180px" @keyup.enter="load" /></el-form-item>
      <el-form-item label="结果">
        <el-select v-model="query.result" clearable style="width:110px">
          <el-option v-for="r in results" :key="r" :label="r" :value="r" />
        </el-select>
      </el-form-item>
      <el-form-item><el-button type="primary" @click="load">查询</el-button></el-form-item>
    </el-form>

    <el-table :data="rows" border stripe v-loading="loading">
      <el-table-column prop="inspectionNo" label="检验单号" width="140" align="center" />
      <el-table-column prop="inspectDate" label="日期" width="100" align="center">
        <template #default="{ row }">{{ fmt(row.inspectDate) }}</template>
      </el-table-column>
      <el-table-column prop="planNo" label="制号" width="90" align="center" />
      <el-table-column prop="productName" label="产品" min-width="130" align="center" />
      <el-table-column prop="processName" label="工序" width="90" align="center" />
      <el-table-column prop="inspectQty" label="检验数量" width="90" align="center" />
      <el-table-column prop="qualifiedQty" label="合格" width="80" align="center" />
      <el-table-column prop="defectQty" label="不良" width="80" align="center">
        <template #default="{ row }"><span v-if="row.defectQty" style="color:#f56c6c">{{ row.defectQty }}</span><span v-else>-</span></template>
      </el-table-column>
      <el-table-column prop="defectReason" label="不良原因" min-width="140" align="center" />
      <el-table-column prop="result" label="结果" width="90" align="center">
        <template #default="{ row }">
          <el-tag :type="{ 合格: 'success', 不合格: 'danger', 返工: 'warning' }[row.result]" size="small">{{ row.result }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="inspector" label="检验员" width="90" align="center" />
      <el-table-column prop="handler" label="处理人" width="90" align="center" />
      <el-table-column label="操作" width="140" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑质检记录' : '新增质检记录'" width="680px" destroy-on-close>
      <el-form :model="form" label-width="90px">
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="检验单号"><el-input v-model="form.inspectionNo" placeholder="留空自动生成" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="日期">
              <el-date-picker v-model="form.inspectDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="制号"><el-input v-model="form.planNo" /></el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="产品">
              <el-select v-model="form.productId" filterable clearable style="width:100%">
                <el-option v-for="p in products" :key="p.id" :label="p.name" :value="p.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="工序">
              <el-input v-model="form.processName" placeholder="如 落料/拉延/修边" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="检验员"><el-input v-model="form.inspector" /></el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="检验数量"><el-input-number v-model="form.inspectQty" :min="0" style="width:100%" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="合格数量"><el-input-number v-model="form.qualifiedQty" :min="0" style="width:100%" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="不良数量"><el-input-number v-model="form.defectQty" :min="0" style="width:100%" /></el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="不良原因"><el-input v-model="form.defectReason" /></el-form-item>
        <el-form-item label="结果">
          <el-select v-model="form.result" style="width:100%">
            <el-option v-for="r in results" :key="r" :label="r" :value="r" />
          </el-select>
        </el-form-item>
        <el-form-item label="处理人"><el-input v-model="form.handler" /></el-form-item>
        <el-form-item label="备注"><el-input v-model="form.remark" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>
  </el-card>
</template>

<script setup>
import { reactive, ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '../../api/modules'

const results = ['合格', '不合格', '返工']
const rows = ref([])
const products = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref(false)
const query = reactive({ keyword: '', result: '' })
const form = reactive({})

async function load() {
  loading.value = true
  try { rows.value = await api.inspections(query) } finally { loading.value = false }
}
function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { inspectionNo: '', inspectDate: new Date().toISOString().slice(0, 10), planNo: '', productId: null, processName: '', inspectQty: 0, qualifiedQty: 0, defectQty: 0, defectReason: '', result: '合格', inspector: '', handler: '', remark: '' })
  dialogVisible.value = true
}
function openEdit(row) {
  editing.value = true
  Object.assign(form, { ...row, inspectDate: String(row.inspectDate).slice(0, 10) })
  dialogVisible.value = true
}
async function save() {
  if (editing.value) { await api.updateInspection(form.id, form); ElMessage.success('更新成功') }
  else { await api.createInspection(form); ElMessage.success('新增成功') }
  dialogVisible.value = false
  load()
}
async function remove(row) {
  await ElMessageBox.confirm('确定删除该质检记录？', '提示', { type: 'warning' })
  await api.deleteInspection(row.id)
  ElMessage.success('删除成功')
  load()
}
function fmt(v) { return v ? String(v).slice(0, 10) : '-' }
onMounted(async () => {
  load()
  products.value = await api.products()
})
</script>
