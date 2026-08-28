<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>培训管理</span>
        <el-button type="primary" size="small" @click="openCreate">新建</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="关键词">
        <el-input v-model="query.keyword" placeholder="培训编号/名称" clearable style="width:180px" @keyup.enter="load" />
      </el-form-item>
      <el-form-item label="状态">
        <el-select v-model="query.status" placeholder="全部" clearable style="width:120px">
          <el-option label="计划中" value="计划中" />
          <el-option label="进行中" value="进行中" />
          <el-option label="已完成" value="已完成" />
        </el-select>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="load">查询</el-button>
        <el-button @click="resetQuery">重置</el-button>
      </el-form-item>
    </el-form>

    <el-table :data="rows" border stripe v-loading="loading" size="default">
      <el-table-column type="index" label="#" width="55" align="center" />
      <el-table-column prop="trainNo" label="培训编号" width="140" align="center" class-name="col-nowrap" />
      <el-table-column prop="trainName" label="培训名称" min-width="150" show-overflow-tooltip />
      <el-table-column prop="trainType" label="培训类型" width="100" align="center" />
      <el-table-column prop="trainer" label="培训讲师" width="100" align="center" />
      <el-table-column prop="trainDate" label="培训日期" width="110" align="center">
        <template #default="{ row }">{{ fmtDate(row.trainDate) }}</template>
      </el-table-column>
      <el-table-column prop="location" label="培训地点" width="110" align="center" />
      <el-table-column prop="status" label="状态" width="90" align="center">
        <template #default="{ row }">
          <el-tag :type="statusType(row.status)" size="small">{{ row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="participantCount" label="参与人数" width="90" align="center" />
      <el-table-column label="操作" width="180" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="viewDetail(row)">详情</el-button>
          <el-button link type="primary" size="small" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 新建/编辑对话框 -->
    <el-dialog v-model="dialogVisible" :title="editing ? '编辑' : '新建'" width="600px" destroy-on-close>
      <el-form :model="form" label-width="100px">
        <el-row :gutter="10">
          <el-col :span="12"><el-form-item label="培训编号"><el-input v-model="form.trainNo" placeholder="留空自动生成" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="培训名称" required><el-input v-model="form.trainName" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="培训类型"><el-select v-model="form.trainType" style="width:100%"><el-option label="内部培训" value="内部培训" /><el-option label="外部培训" value="外部培训" /></el-select></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="培训讲师"><el-input v-model="form.trainer" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="培训日期"><el-date-picker v-model="form.trainDate" type="date" value-format="YYYY-MM-DD" style="width:100%" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="培训地点"><el-input v-model="form.location" /></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="状态"><el-select v-model="form.status" style="width:100%"><el-option label="计划中" value="计划中" /><el-option label="进行中" value="进行中" /><el-option label="已完成" value="已完成" /></el-select></el-form-item></el-col>
          <el-col :span="12"><el-form-item label="备注"><el-input v-model="form.remark" /></el-form-item></el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>

    <!-- 培训详情对话框 -->
    <el-dialog v-model="detailVisible" title="培训详情" width="700px" destroy-on-close>
      <el-descriptions :column="2" border size="small" style="margin-bottom:16px">
        <el-descriptions-item label="培训编号">{{ detail.trainNo }}</el-descriptions-item>
        <el-descriptions-item label="培训名称">{{ detail.trainName }}</el-descriptions-item>
        <el-descriptions-item label="培训类型">{{ detail.trainType }}</el-descriptions-item>
        <el-descriptions-item label="培训讲师">{{ detail.trainer }}</el-descriptions-item>
        <el-descriptions-item label="培训日期">{{ fmtDate(detail.trainDate) }}</el-descriptions-item>
        <el-descriptions-item label="培训地点">{{ detail.location }}</el-descriptions-item>
        <el-descriptions-item label="状态">
          <el-tag :type="statusType(detail.status)" size="small">{{ detail.status }}</el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="备注">{{ detail.remark || '-' }}</el-descriptions-item>
      </el-descriptions>

      <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:8px">
        <span style="font-weight:bold">参与人员</span>
        <el-button type="primary" size="small" @click="participantVisible = true">添加参与人</el-button>
      </div>
      <el-table :data="detail.participants || []" border size="small">
        <el-table-column type="index" label="#" width="50" align="center" />
        <el-table-column prop="empCode" label="工号" width="100" align="center" />
        <el-table-column prop="empName" label="姓名" width="100" align="center" />
        <el-table-column prop="result" label="考核结果" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="resultType(row.result)" size="small">{{ row.result }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="remark" label="备注" min-width="120" show-overflow-tooltip />
        <el-table-column label="操作" width="120" align="center">
          <template #default="{ row }">
            <el-button link type="primary" size="small" @click="editParticipant(row)">考核</el-button>
            <el-button link type="danger" size="small" @click="removeParticipant(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-dialog>

    <!-- 添加参与人对话框 -->
    <el-dialog v-model="participantVisible" title="添加参与人" width="400px" destroy-on-close>
      <el-form :model="participantForm" label-width="80px">
        <el-form-item label="工号" required><el-input v-model="participantForm.empCode" /></el-form-item>
        <el-form-item label="姓名" required><el-input v-model="participantForm.empName" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="participantVisible = false">取消</el-button>
        <el-button type="primary" @click="addParticipant">确定</el-button>
      </template>
    </el-dialog>

    <!-- 考核对话框 -->
    <el-dialog v-model="evalVisible" title="考核结果" width="400px" destroy-on-close>
      <el-form :model="evalForm" label-width="80px">
        <el-form-item label="考核结果">
          <el-select v-model="evalForm.result" style="width:100%">
            <el-option label="合格" value="合格" />
            <el-option label="不合格" value="不合格" />
            <el-option label="待考核" value="待考核" />
          </el-select>
        </el-form-item>
        <el-form-item label="备注"><el-input v-model="evalForm.remark" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="evalVisible = false">取消</el-button>
        <el-button type="primary" @click="saveEvaluation">保存</el-button>
      </template>
    </el-dialog>
  </el-card>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '../../api/modules'

const rows = ref([])
const loading = ref(false)
const query = reactive({ keyword: '', status: '' })
const dialogVisible = ref(false)
const editing = ref(false)
const form = reactive({})
const detailVisible = ref(false)
const detail = ref({})
const participantVisible = ref(false)
const participantForm = reactive({ empCode: '', empName: '' })
const evalVisible = ref(false)
const evalForm = reactive({ id: 0, result: '', remark: '' })

async function load() {
  loading.value = true
  try {
    rows.value = await api.trainings({ ...query })
  } finally {
    loading.value = false
  }
}

function resetQuery() {
  query.keyword = ''
  query.status = ''
  load()
}

function openCreate() {
  editing.value = false
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, { trainType: '内部培训', status: '计划中' })
  dialogVisible.value = true
}

function openEdit(row) {
  editing.value = true
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, JSON.parse(JSON.stringify(row)))
  dialogVisible.value = true
}

async function save() {
  if (editing.value) {
    await api.updateTraining(form.id, form)
    ElMessage.success('更新成功')
  } else {
    await api.createTraining(form)
    ElMessage.success('新建成功')
  }
  dialogVisible.value = false
  load()
}

async function remove(row) {
  await ElMessageBox.confirm('确定删除该培训记录？', '提示', { type: 'warning' })
  await api.deleteTraining(row.id)
  ElMessage.success('删除成功')
  load()
}

async function viewDetail(row) {
  detail.value = await api.training(row.id)
  detailVisible.value = true
}

async function addParticipant() {
  if (!participantForm.empCode || !participantForm.empName) { ElMessage.warning('请填写工号和姓名'); return }
  await api.addTrainingParticipant(detail.value.id, participantForm)
  ElMessage.success('添加成功')
  participantVisible.value = false
  participantForm.empCode = ''
  participantForm.empName = ''
  detail.value = await api.training(detail.value.id)
  load()
}

function editParticipant(row) {
  evalForm.id = row.id
  evalForm.result = row.result
  evalForm.remark = row.remark
  evalVisible.value = true
}

async function saveEvaluation() {
  await api.updateTrainingParticipant(evalForm.id, { result: evalForm.result, remark: evalForm.remark })
  ElMessage.success('保存成功')
  evalVisible.value = false
  detail.value = await api.training(detail.value.id)
}

async function removeParticipant(row) {
  await ElMessageBox.confirm('确定删除该参与人？', '提示', { type: 'warning' })
  await api.removeTrainingParticipant(row.id)
  ElMessage.success('删除成功')
  detail.value = await api.training(detail.value.id)
  load()
}

function statusType(s) {
  return { 计划中: 'info', 进行中: 'warning', 已完成: 'success' }[s] || 'info'
}
function resultType(s) {
  return { 合格: 'success', 不合格: 'danger', 待考核: 'warning' }[s] || 'info'
}
function fmtDate(v) { return v ? String(v).slice(0, 10) : '-' }

onMounted(load)
</script>

<style scoped>
.search-bar { margin-bottom: 12px; }
</style>
