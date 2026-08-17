import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  { path: '/login', name: 'Login', component: () => import('../views/Login.vue'), meta: { public: true } },
  {
    path: '/',
    component: () => import('../layout/Layout.vue'),
    redirect: '/dashboard',
    children: [
      { path: 'dashboard', name: 'Dashboard', component: () => import('../views/Dashboard.vue'), meta: { title: '驾驶舱管理' } },
      // 销售管理
      { path: 'sales/orders', name: 'SalesOrders', component: () => import('../views/sales/SalesOrder.vue'), meta: { title: '销售订单' } },
      { path: 'sales/deliveries', name: 'Deliveries', component: () => import('../views/sales/Delivery.vue'), meta: { title: '发货管理' } },
      // 采购管理
      { path: 'purchase/orders', name: 'PurchaseOrders', component: () => import('../views/purchase/PurchaseOrder.vue'), meta: { title: '采购订单' } },
      { path: 'purchase/receipts', name: 'PurchaseReceipts', component: () => import('../views/purchase/PurchaseReceipt.vue'), meta: { title: '到货管理' } },
      // 仓库管理
      { path: 'warehouse/inventory', name: 'Inventory', component: () => import('../views/warehouse/Inventory.vue'), meta: { title: '库存查询' } },
      { path: 'warehouse/ledger', name: 'Ledger', component: () => import('../views/warehouse/Ledger.vue'), meta: { title: '库存流水' } },
      { path: 'warehouse/stock', name: 'StockOps', component: () => import('../views/warehouse/StockOps.vue'), meta: { title: '出入库操作' } },
      { path: 'warehouse/warnings', name: 'Warnings', component: () => import('../views/warehouse/Warnings.vue'), meta: { title: '库存预警' } },
      // 排班管理
      { path: 'schedule/shifts', name: 'Shifts', component: () => import('../views/schedule/Shift.vue'), meta: { title: '班次设置' } },
      { path: 'schedule/work', name: 'WorkSchedules', component: () => import('../views/schedule/WorkSchedule.vue'), meta: { title: '排班计划' } },
      // 生产管理
      { path: 'production/plans', name: 'ProductionPlans', component: () => import('../views/production/ProductionPlan.vue'), meta: { title: '生产计划' } },
      { path: 'production/orders', name: 'ProductionOrders', component: () => import('../views/production/ProductionOrder.vue'), meta: { title: '冲压产量单' } },
      { path: 'production/daily', name: 'DailyReports', component: () => import('../views/production/DailyReport.vue'), meta: { title: '生产日报' } },
      // 质量管理
      { path: 'quality/inspections', name: 'Inspections', component: () => import('../views/quality/Inspection.vue'), meta: { title: '质检记录' } },
      { path: 'quality/tools', name: 'MeasuringTools', component: () => import('../views/quality/MeasuringTool.vue'), meta: { title: '计量器具台账' } },
      { path: 'quality/toolapply', name: 'ToolApplies', component: () => import('../views/quality/ToolApply.vue'), meta: { title: '量具申购' } },
      { path: 'quality/toolscrap', name: 'ToolScraps', component: () => import('../views/quality/ToolScrap.vue'), meta: { title: '器具报废' } },
      { path: 'quality/calibration', name: 'ToolCalibrations', component: () => import('../views/quality/ToolCalibration.vue'), meta: { title: '检定处理' } },
      // 设备管理
      { path: 'equipment/list', name: 'Equipments', component: () => import('../views/equipment/Equipment.vue'), meta: { title: '设备台账' } },
      { path: 'equipment/maintenance', name: 'Maintenances', component: () => import('../views/equipment/Maintenance.vue'), meta: { title: '维护记录' } },
      // 模具/工装夹具管理
      { path: 'mold/list', name: 'Molds', component: () => import('../views/mold/Mold.vue'), meta: { title: '模具台账' } },
      { path: 'mold/plans', name: 'MoldPlans', component: () => import('../views/mold/MoldPlan.vue'), meta: { title: '模具制造计划' } },
      // 基础数据
      { path: 'base/customers', name: 'Customers', component: () => import('../views/base/Customer.vue'), meta: { title: '客户管理' } },
      { path: 'base/suppliers', name: 'Suppliers', component: () => import('../views/base/Supplier.vue'), meta: { title: '供应商管理' } },
      { path: 'base/materials', name: 'Materials', component: () => import('../views/base/Material.vue'), meta: { title: '原材料管理' } },
      { path: 'base/products', name: 'Products', component: () => import('../views/base/Product.vue'), meta: { title: '产品管理' } },
      { path: 'base/warehouses', name: 'Warehouses', component: () => import('../views/base/Warehouse.vue'), meta: { title: '仓库管理' } },
      { path: 'base/employees', name: 'Employees', component: () => import('../views/base/Employee.vue'), meta: { title: '员工管理' } },
      // 系统管理
      { path: 'system/users', name: 'Users', component: () => import('../views/system/User.vue'), meta: { title: '用户管理', adminOnly: true } }
    ]
  },
  { path: '/:pathMatch(.*)*', redirect: '/dashboard' }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token')
  if (!to.meta.public && !token) {
    next('/login')
  } else if (to.meta.adminOnly) {
    const user = JSON.parse(localStorage.getItem('user') || 'null')
    if (!user || user.role !== 'admin') {
      next('/dashboard')
    } else {
      next()
    }
  } else {
    next()
  }
})

export default router
