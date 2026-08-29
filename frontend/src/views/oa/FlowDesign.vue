<template>
  <el-card shadow="never">
    <template #header>
      <div style="display:flex;align-items:center;justify-content:space-between">
        <span>流程设计</span>
        <el-button type="primary" size="small" @click="openWizard">新增流程</el-button>
      </div>
    </template>

    <el-form :inline="true" class="search-bar">
      <el-form-item label="关键词">
        <el-input v-model="query.keyword" placeholder="流程编号/名称" clearable style="width:180px" @keyup.enter="load" />
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="load">查询</el-button>
        <el-button @click="resetQuery">重置</el-button>
      </el-form-item>
    </el-form>

    <el-table :data="rows" border stripe v-loading="loading" size="default">
      <el-table-column type="index" label="#" width="55" align="center" />
      <el-table-column prop="flowNo" label="流程编号" width="170" align="center" class-name="col-nowrap" />
      <el-table-column prop="flowName" label="流程名称" min-width="130" show-overflow-tooltip />
      <el-table-column prop="remark" label="备注" min-width="150" show-overflow-tooltip />
      <el-table-column prop="sort" label="排序" width="70" align="center" />
      <el-table-column prop="status" label="状态" width="80" align="center">
        <template #default="{ row }">
          <el-tag :type="row.status === '有效' ? 'success' : 'info'" size="small">{{ row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="deptName" label="所属部门" width="100" align="center" />
      <el-table-column prop="nodeCount" label="节点数" width="80" align="center" />
      <el-table-column prop="createdAt" label="创建时间" width="160" align="center">
        <template #default="{ row }">{{ fmtDateTime(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="180" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="viewFlow(row)">预览</el-button>
          <el-button link type="primary" size="small" @click="editFlow(row)">修改</el-button>
          <el-button link type="danger" size="small" @click="remove(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 三步向导对话框 -->
    <el-dialog v-model="wizardVisible" :title="editing ? '修改流程' : '添加流程'" width="720px" destroy-on-close :close-on-click-modal="false">
      <el-steps :active="currentStep" align-center finish-status="success" style="margin-bottom:24px">
        <el-step title="基本信息" />
        <el-step title="选择表单" />
        <el-step title="流程设计" />
      </el-steps>

      <!-- 步骤1: 基本信息 -->
      <div v-show="currentStep === 0">
        <el-form :model="form" label-width="100px">
          <el-form-item label="流程编号" required>
            <el-input v-model="form.flowNo" placeholder="留空自动生成" />
          </el-form-item>
          <el-form-item label="流程名称" required>
            <el-input v-model="form.flowName" placeholder="请输入流程名称" />
          </el-form-item>
          <el-form-item label="排序" required>
            <el-input-number v-model="form.sort" :min="1" style="width:100%" />
          </el-form-item>
          <el-form-item label="选项">
            <el-checkbox v-model="form.statusChecked">有效标识</el-checkbox>
          </el-form-item>
          <el-form-item label="所属部门">
            <el-input v-model="form.deptName" placeholder="请输入所属部门" />
          </el-form-item>
          <el-form-item label="备注">
            <el-input v-model="form.remark" type="textarea" :rows="3" placeholder="请输入备注" />
          </el-form-item>
        </el-form>
      </div>

      <!-- 步骤2: 选择表单 -->
      <div v-show="currentStep === 1">
        <el-form label-width="100px">
          <el-form-item label="关联表单">
            <el-select v-model="form.formType" placeholder="请选择关联表单" style="width:100%">
              <el-option label="通用流程（无表单）" value="" />
              <el-option label="请假申请单" value="请假申请单" />
              <el-option label="加班申请单" value="加班申请单" />
              <el-option label="采购申请单" value="采购申请单" />
              <el-option label="付款申请单" value="付款申请单" />
              <el-option label="报销申请单" value="报销申请单" />
            </el-select>
          </el-form-item>
          <el-form-item label="说明">
            <div style="color:#999;font-size:13px;line-height:1.6">
              选择关联表单后，发起流程时将使用对应的表单模板。<br>
              通用流程适用于无固定格式的审批事项。
            </div>
          </el-form-item>
        </el-form>
      </div>

      <!-- 步骤3: 流程设计（节点配置） -->
      <div v-show="currentStep === 2">
        <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:12px">
          <span style="font-weight:bold">审批节点</span>
          <el-button type="primary" size="small" @click="addNode">添加节点</el-button>
        </div>
        <el-table :data="form.nodes" border size="small">
          <el-table-column type="index" label="#" width="50" align="center" />
          <el-table-column label="节点名称" min-width="150">
            <template #default="{ row }">
              <el-input v-model="row.nodeName" placeholder="如：部门主管审批" size="small" />
            </template>
          </el-table-column>
          <el-table-column label="审批人" min-width="130">
            <template #default="{ row }">
              <el-input v-model="row.approver" placeholder="审批人姓名" size="small" />
            </template>
          </el-table-column>
          <el-table-column label="顺序" width="80" align="center">
            <template #default="{ row, $index }">
              <el-tag size="small">{{ $index + 1 }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column label="操作" width="80" align="center">
            <template #default="{ $index }">
              <el-button link type="danger" size="small" @click="removeNode($index)" :disabled="form.nodes.length <= 1">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
        <div style="color:#999;font-size:12px;margin-top:8px">
          审批流程将按节点顺序依次流转，最后一个节点审批通过后流程结束。
        </div>
      </div>

      <template #footer>
        <el-button v-if="currentStep > 0" @click="currentStep--">上一步</el-button>
        <el-button v-if="currentStep < 2" type="primary" @click="nextStep">下一步</el-button>
        <el-button v-if="currentStep === 2" type="primary" @click="save" :loading="saving">保存</el-button>
        <el-button @click="wizardVisible = false">取消</el-button>
      </template>
    </el-dialog>

    <!-- 流程预览对话框 -->
    <el-dialog v-model="previewVisible" title="流程预览" width="600px" destroy-on-close>
      <el-descriptions :column="2" border size="small" style="margin-bottom:16px">
        <el-descriptions-item label="流程编号">{{ preview.flowNo }}</el-descriptions-item>
        <el-descriptions-item label="流程名称">{{ preview.flowName }}</el-descriptions-item>
        <el-descriptions-item label="排序">{{ preview.sort }}</el-descriptions-item>
        <el-descriptions-item label="状态">
          <el-tag :type="preview.status === '有效' ? 'success' : 'info'" size="small">{{ preview.status }}</el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="所属部门">{{ preview.deptName || '-' }}</el-descriptions-item>
        <el-descriptions-item label="备注" :span="2">{{ preview.remark || '-' }}</el-descriptions-item>
      </el-descriptions>
      <div style="font-weight:bold;margin-bottom:8px">审批节点</div>
      <el-steps direction="vertical" :active="(preview.nodes || []).length" style="padding-left:20px">
        <el-step v-for="(node, i) in preview.nodes || []" :key="i" :title="node.nodeName" :description="'审批人：' + node.approver" />
      </el-steps>
    </el-dialog>
  </el-card>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '../../api/modules'

const rows = ref([])
const loading = ref(false)
const query = reactive({ keyword: '' })
const wizardVisible = ref(false)
const previewVisible = ref(false)
const editing = ref(false)
const currentStep = ref(0)
const saving = ref(false)
const form = reactive({
  id: 0, flowNo: '', flowName: '', sort: 1, statusChecked: true,
  deptName: '', remark: '', formType: '', nodes: []
})
const preview = ref({})

async function load() {
  loading.value = true
  try {
    rows.value = await api.crud.list('/FlowDesigns', { ...query })
  } finally {
    loading.value = false
  }
}

function resetQuery() {
  query.keyword = ''
  load()
}

function openWizard() {
  editing.value = false
  currentStep.value = 0
  Object.assign(form, {
    id: 0, flowNo: '', flowName: '', sort: 1, statusChecked: true,
    deptName: '', remark: '', formType: '',
    nodes: [{ nodeName: '', approver: '' }]
  })
  wizardVisible.value = true
}

async function editFlow(row) {
  editing.value = true
  currentStep.value = 0
  const detail = await api.crud.list(`/FlowDesigns/${row.id}`)
  Object.assign(form, {
    id: detail.id,
    flowNo: detail.flowNo,
    flowName: detail.flowName,
    sort: detail.sort,
    statusChecked: detail.status === '有效',
    deptName: detail.deptName,
    remark: detail.remark,
    formType: detail.formType || '',
    nodes: detail.nodes && detail.nodes.length > 0
      ? detail.nodes.map(n => ({ nodeName: n.nodeName, approver: n.approver }))
      : [{ nodeName: '', approver: '' }]
  })
  wizardVisible.value = true
}

function nextStep() {
  if (currentStep.value === 0 && !form.flowName) {
    ElMessage.warning('请输入流程名称')
    return
  }
  currentStep.value++
}

function addNode() {
  form.nodes.push({ nodeName: '', approver: '' })
}

function removeNode(index) {
  form.nodes.splice(index, 1)
}

async function save() {
  if (!form.flowName) { ElMessage.warning('请输入流程名称'); return }
  if (form.nodes.some(n => !n.nodeName || !n.approver)) {
    ElMessage.warning('请完善所有节点的名称和审批人')
    return
  }
  saving.value = true
  try {
    const payload = {
      id: form.id,
      flowNo: form.flowNo,
      flowName: form.flowName,
      sort: form.sort,
      status: form.statusChecked ? '有效' : '无效',
      deptName: form.deptName,
      remark: form.remark,
      formType: form.formType,
      nodes: form.nodes.map((n, i) => ({ nodeName: n.nodeName, approver: n.approver, sort: i + 1 }))
    }
    if (editing.value) {
      await api.crud.update('/FlowDesigns', form.id, payload)
      ElMessage.success('修改成功')
    } else {
      await api.crud.create('/FlowDesigns', payload)
      ElMessage.success('新增成功')
    }
    wizardVisible.value = false
    load()
  } finally {
    saving.value = false
  }
}

async function viewFlow(row) {
  preview.value = await api.crud.list(`/FlowDesigns/${row.id}`)
  previewVisible.value = true
}

async function remove(row) {
  await ElMessageBox.confirm('确定删除该流程？', '提示', { type: 'warning' })
  await api.crud.remove('/FlowDesigns', row.id)
  ElMessage.success('删除成功')
  load()
}

function fmtDateTime(v) { return v ? String(v).replace('T', ' ').slice(0, 19) : '-' }

onMounted(load)
</script>

<style scoped>
.search-bar { margin-bottom: 12px; }
</style>
