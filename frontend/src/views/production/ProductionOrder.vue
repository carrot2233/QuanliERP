<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>冲压产量单</span>
        <el-button type="primary" size="small" @click="openCreate">新增产量单</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="关键词"><el-input v-model="query.keyword" clearable style="width:180px" @keyup.enter="load" /></el-form-item>
      <el-form-item label="制号">
        <el-select v-model="query.planNo" clearable filterable style="width:120px" @change="load">
          <el-option v-for="p in planNos" :key="p" :label="p" :value="p" />
        </el-select>
      </el-form-item>
      <el-form-item><el-button type="primary" @click="load">查询</el-button></el-form-item>
    </el-form>

    <el-table :data="rows" border stripe v-loading="loading">
      <el-table-column prop="date" label="日期" width="100" align="center">
        <template #default="{ row }">{{ fmt(row.date) }}</template>
      </el-table-column>
      <el-table-column prop="planNo" label="制号" width="80" align="center" />
      <el-table-column prop="processName" label="工序" width="90" align="center" />
      <el-table-column prop="project" label="项目" min-width="120" align="center" />
      <el-table-column prop="orderNo" label="编号" width="150" align="center" />
      <el-table-column prop="finishedQty" label="成品" width="70" align="center" />
      <el-table-column prop="scrapQty" label="废品" width="70" align="center">
        <template #default="{ row }"><span v-if="row.scrapQty" style="color:#f56c6c">{{ row.scrapQty }}</span><span v-else>-</span></template>
      </el-table-column>
      <el-table-column prop="completedQty" label="完成" width="70" align="center" />
      <el-table-column prop="workHours" label="工时" width="70" align="center" />
      <el-table-column prop="machineNo" label="机床" width="90" align="center" />
      <el-table-column label="操作者" min-width="130" align="center">
        <template #default="{ row }">{{ [row.operator1, row.operator2, row.operator3, row.operator4].filter(Boolean).join('、') }}</template>
      </el-table-column>
      <el-table-column prop="shiftName" label="班次" width="90" align="center" />
      <el-table-column label="操作" width="140" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑产量单' : '新增产量单'" width="860px" destroy-on-close>
      <el-form :model="form" label-width="90px">
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="日期">
              <el-date-picker v-model="form.date" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="制号">
              <el-select v-model="form.planNo" filterable allow-create style="width:100%">
                <el-option v-for="p in planNos" :key="p" :label="p" :value="p" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="工序名称">
              <el-select v-model="form.processName" filterable allow-create style="width:100%">
                <el-option v-for="p in processNames" :key="p" :label="p" :value="p" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="项目"><el-input v-model="form.project" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="工序说明"><el-input v-model="form.processDesc" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="班次">
              <el-select v-model="form.shiftId" clearable style="width:100%" @change="v => { const s = shifts.find(x => x.id === v); form.shiftName = s ? s.name : '' }">
                <el-option v-for="s in shifts" :key="s.id" :label="s.name" :value="s.id" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="6">
            <el-form-item label="成品数量"><el-input-number v-model="form.finishedQty" :min="0" style="width:100%" /></el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="废品数量"><el-input-number v-model="form.scrapQty" :min="0" style="width:100%" /></el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="完成数量"><el-input-number v-model="form.completedQty" :min="0" style="width:100%" /></el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="工时"><el-input-number v-model="form.workHours" :min="0" :precision="2" style="width:100%" /></el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-form-item label="机床"><el-input v-model="form.machineNo" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="编号"><el-input v-model="form.orderNo" placeholder="留空自动生成" /></el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="操作者1"><el-input v-model="form.operator1" /></el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="10">
          <el-col :span="8"><el-form-item label="操作者2"><el-input v-model="form.operator2" /></el-form-item></el-col>
          <el-col :span="8"><el-form-item label="操作者3"><el-input v-model="form.operator3" /></el-form-item></el-col>
          <el-col :span="8"><el-form-item label="操作者4"><el-input v-model="form.operator4" /></el-form-item></el-col>
        </el-row>
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

const processNames = ['落料', '拉延', '修边', '冲孔', '侧冲', '翻边', '整形', '折弯', '焊接', '检验']
const rows = ref([])
const shifts = ref([])
const planNos = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref(false)
const query = reactive({ keyword: '', planNo: '' })
const form = reactive({})

async function load() {
  loading.value = true
  try { rows.value = await api.productionOrders(query) } finally { loading.value = false }
  const plans = await api.productionPlans({})
  planNos.value = [...new Set(plans.map(p => p.planNo))].sort()
}
function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { date: new Date().toISOString().slice(0, 10), planNo: '', processName: '', project: '', processDesc: '', finishedQty: 0, scrapQty: 0, completedQty: 0, workHours: 0, machineNo: '', orderNo: '', operator1: '', operator2: '', operator3: '', operator4: '', shiftId: null, shiftName: '', remark: '' })
  dialogVisible.value = true
}
function openEdit(row) {
  editing.value = true
  Object.assign(form, { ...row })
  dialogVisible.value = true
}
async function save() {
  if (editing.value) { await api.updateProductionOrder(form.id, form); ElMessage.success('更新成功') }
  else { await api.createProductionOrder(form); ElMessage.success('新增成功') }
  dialogVisible.value = false
  load()
}
async function remove(row) {
  await ElMessageBox.confirm('确定删除该产量单？', '提示', { type: 'warning' })
  await api.deleteProductionOrder(row.id)
  ElMessage.success('删除成功')
  load()
}
function fmt(v) { return v ? String(v).slice(0, 10) : '-' }
onMounted(async () => {
  load()
  shifts.value = await api.shifts()
})
</script>
