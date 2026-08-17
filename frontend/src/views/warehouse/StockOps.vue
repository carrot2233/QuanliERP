<template>
  <el-row :gutter="16">
    <el-col :span="12">
      <el-card shadow="never">
        <template #header><span>库存出入库操作</span></template>
        <el-form :model="form" label-width="100px">
          <el-form-item label="物料类型" required>
            <el-radio-group v-model="form.itemType">
              <el-radio-button label="材料">材料</el-radio-button>
              <el-radio-button label="产品">产品</el-radio-button>
            </el-radio-group>
          </el-form-item>
          <el-form-item label="物料" required>
            <el-select v-model="form.itemId" filterable placeholder="选择物料" style="width:100%" @change="onItemChange">
              <el-option v-for="it in items" :key="it.id" :label="it.code + ' ' + it.name + ' ' + (it.specification||'')" :value="it.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="仓库" required>
            <el-select v-model="form.warehouseId" style="width:100%">
              <el-option v-for="w in warehouses" :key="w.id" :label="w.name" :value="w.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="单据类型" required>
            <el-select v-model="form.billType" style="width:100%">
              <el-option v-for="t in billTypes" :key="t" :label="t" :value="t" />
            </el-select>
          </el-form-item>
          <el-form-item label="出入方向" required>
            <el-radio-group v-model="direction">
              <el-radio-button label="in">入库</el-radio-button>
              <el-radio-button label="out">出库</el-radio-button>
            </el-radio-group>
          </el-form-item>
          <el-form-item label="数量" required>
            <el-input-number v-model="form.qty" :min="0" style="width:100%" />
          </el-form-item>
          <el-form-item label="单号"><el-input v-model="form.billNo" placeholder="留空自动生成" /></el-form-item>
          <el-form-item label="备注"><el-input v-model="form.remark" /></el-form-item>
          <el-form-item>
            <el-button type="primary" @click="save">提交</el-button>
          </el-form-item>
        </el-form>
      </el-card>
    </el-col>
    <el-col :span="12">
      <el-card shadow="never">
        <template #header><span>生产完工入库（车间入库）</span></template>
        <el-form :model="winForm" label-width="100px">
          <el-form-item label="产品" required>
            <el-select v-model="winForm.productId" filterable placeholder="选择产品" style="width:100%">
              <el-option v-for="p in products" :key="p.id" :label="p.code + ' ' + p.name" :value="p.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="入库仓库" required>
            <el-select v-model="winForm.warehouseId" style="width:100%">
              <el-option v-for="w in warehouses" :key="w.id" :label="w.name" :value="w.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="制号"><el-input v-model="winForm.planNo" placeholder="如 M01" /></el-form-item>
          <el-form-item label="数量" required>
            <el-input-number v-model="winForm.qty" :min="1" style="width:100%" />
          </el-form-item>
          <el-form-item label="备注"><el-input v-model="winForm.remark" /></el-form-item>
          <el-form-item>
            <el-button type="primary" @click="saveWin">提交入库</el-button>
          </el-form-item>
        </el-form>
      </el-card>
    </el-col>
  </el-row>
</template>

<script setup>
import { reactive, ref, watch, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import api from '../../api/modules'

const billTypes = ['生产领用', '车间入库', '退件', '销售出库', '盘盈', '盘亏', '其他']
const warehouses = ref([])
const materials = ref([])
const products = ref([])
const items = ref([])
const direction = ref('in')
const form = reactive({ itemType: '材料', itemId: null, warehouseId: null, billType: '生产领用', qty: 1, billNo: '', remark: '' })
const winForm = reactive({ productId: null, warehouseId: null, planNo: '', qty: 1, remark: '' })

watch(() => form.itemType, () => {
  items.value = form.itemType === '材料' ? materials.value : products.value
  form.itemId = null
})

function onItemChange() {}

async function save() {
  if (!form.itemId) return ElMessage.warning('请选择物料')
  if (!form.warehouseId) return ElMessage.warning('请选择仓库')
  if (!form.qty) return ElMessage.warning('请输入数量')
  const payload = {
    warehouseId: form.warehouseId, itemType: form.itemType, itemId: form.itemId,
    itemName: items.value.find(i => i.id === form.itemId)?.name || '',
    specification: items.value.find(i => i.id === form.itemId)?.specification || '',
    billType: form.billType, billNo: form.billNo, remark: form.remark,
    inQty: direction.value === 'in' ? form.qty : 0,
    outQty: direction.value === 'out' ? form.qty : 0
  }
  await api.stockInOut(payload)
  ElMessage.success('操作成功')
}

async function saveWin() {
  if (!winForm.productId) return ElMessage.warning('请选择产品')
  if (!winForm.warehouseId) return ElMessage.warning('请选择仓库')
  await api.workshopIn(winForm)
  ElMessage.success('车间入库成功')
}

onMounted(async () => {
  warehouses.value = await api.warehouses()
  materials.value = await api.materials()
  products.value = await api.products()
  items.value = materials.value
})
</script>
