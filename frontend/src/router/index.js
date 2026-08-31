import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  { path: '/login', name: 'Login', component: () => import('../views/Login.vue'), meta: { public: true } },
  {
    path: '/',
    component: () => import('../layout/Layout.vue'),
    redirect: '/dashboard',
    children: [
      { path: 'dashboard', name: 'Dashboard', component: () => import('../views/Dashboard.vue'), meta: { title: '驾驶舱管理', permission: 'dashboard' } },
      // 基础数据
      { path: 'base/customers', name: 'Customers', component: () => import('../views/base/Customer.vue'), meta: { title: '客户管理', permission: 'base:customer' } },
      { path: 'base/suppliers', name: 'Suppliers', component: () => import('../views/base/Supplier.vue'), meta: { title: '供应商管理', permission: 'base:supplier' } },
      { path: 'base/materials', name: 'Materials', component: () => import('../views/base/Material.vue'), meta: { title: '原材料管理', permission: 'base:material' } },
      { path: 'base/products', name: 'Products', component: () => import('../views/base/Product.vue'), meta: { title: '产品管理', permission: 'base:product' } },
      { path: 'base/warehouses', name: 'Warehouses', component: () => import('../views/base/Warehouse.vue'), meta: { title: '仓库管理', permission: 'base:warehouse' } },
      { path: 'base/employees', name: 'Employees', component: () => import('../views/base/Employee.vue'), meta: { title: '员工管理', permission: 'base:employee' } },
      // 销售管理
      { path: 'sales/orders', name: 'SalesOrders', component: () => import('../views/sales/SalesOrder.vue'), meta: { title: '销售订单', permission: 'sales:order' } },
      { path: 'sales/deliveries', name: 'Deliveries', component: () => import('../views/sales/Delivery.vue'), meta: { title: '发货管理', permission: 'sales:delivery' } },
      // 采购管理
      { path: 'purchase/orders', name: 'PurchaseOrders', component: () => import('../views/purchase/PurchaseOrder.vue'), meta: { title: '采购订单', permission: 'purchase:order' } },
      { path: 'purchase/receipts', name: 'PurchaseReceipts', component: () => import('../views/purchase/PurchaseReceipt.vue'), meta: { title: '到货管理', permission: 'purchase:receipt' } },
      // 仓库管理
      { path: 'warehouse/inventory', name: 'Inventory', component: () => import('../views/warehouse/Inventory.vue'), meta: { title: '库存查询', permission: 'warehouse:inventory' } },
      { path: 'warehouse/stock-in', name: 'StockIn', component: () => import('../views/warehouse/StockIn.vue'), meta: { title: '入库管理', permission: 'warehouse:stock-in' } },
      { path: 'warehouse/stock-out', name: 'StockOut', component: () => import('../views/warehouse/StockOut.vue'), meta: { title: '出库管理', permission: 'warehouse:stock-out' } },
      { path: 'warehouse/ledger', name: 'Ledger', component: () => import('../views/warehouse/Ledger.vue'), meta: { title: '库存流水', permission: 'warehouse:ledger' } },
      { path: 'warehouse/warnings', name: 'Warnings', component: () => import('../views/warehouse/Warnings.vue'), meta: { title: '库存预警', permission: 'warehouse:warning' } },
      // 排班管理
      { path: 'schedule/shifts', name: 'Shifts', component: () => import('../views/schedule/Shift.vue'), meta: { title: '班次设置', permission: 'schedule:shift' } },
      { path: 'schedule/work', name: 'WorkSchedules', component: () => import('../views/schedule/WorkSchedule.vue'), meta: { title: '排班计划', permission: 'schedule:work' } },
      // 生产管理
      { path: 'production/plans', name: 'ProductionPlans', component: () => import('../views/production/ProductionPlan.vue'), meta: { title: '生产计划', permission: 'production:plan' } },
      { path: 'production/orders', name: 'ProductionOrders', component: () => import('../views/production/ProductionOrder.vue'), meta: { title: '冲压产量单', permission: 'production:order' } },
      { path: 'production/daily', name: 'DailyReports', component: () => import('../views/production/DailyReport.vue'), meta: { title: '生产日报', permission: 'production:daily' } },
      // 模具/工装夹具管理
      { path: 'mold/list', name: 'Molds', component: () => import('../views/mold/Mold.vue'), meta: { title: '模具台账', permission: 'mold:list' } },
      { path: 'mold/plans', name: 'MoldPlans', component: () => import('../views/mold/MoldPlan.vue'), meta: { title: '模具制造计划', permission: 'mold:plan' } },
      // 质量管理
      { path: 'quality/inspections', name: 'Inspections', component: () => import('../views/quality/Inspection.vue'), meta: { title: '质检记录', permission: 'quality:inspection' } },
      { path: 'quality/tools', name: 'MeasuringTools', component: () => import('../views/quality/MeasuringTool.vue'), meta: { title: '计量器具台账', permission: 'quality:tool' } },
      { path: 'quality/toolapply', name: 'ToolApplies', component: () => import('../views/quality/ToolApply.vue'), meta: { title: '量具申购', permission: 'quality:toolapply' } },
      { path: 'quality/toolscrap', name: 'ToolScraps', component: () => import('../views/quality/ToolScrap.vue'), meta: { title: '器具报废', permission: 'quality:toolscrap' } },
      { path: 'quality/calibration', name: 'ToolCalibrations', component: () => import('../views/quality/ToolCalibration.vue'), meta: { title: '检定处理', permission: 'quality:calibration' } },
      // 设备管理
      { path: 'equipment/list', name: 'Equipments', component: () => import('../views/equipment/Equipment.vue'), meta: { title: '设备台账', permission: 'equipment:list' } },
      { path: 'equipment/maintenance', name: 'Maintenances', component: () => import('../views/equipment/Maintenance.vue'), meta: { title: '维护记录', permission: 'equipment:maintenance' } },
      // 系统管理
      { path: 'system/users', name: 'Users', component: () => import('../views/system/User.vue'), meta: { title: '用户管理', permission: 'system:user', adminOnly: true } },
      { path: 'system/roles', name: 'Roles', component: () => import('../views/system/RolePermission.vue'), meta: { title: '角色权限', permission: 'system:role', adminOnly: true } },
      // 协同办公
      { path: 'oa/notices', name: 'Notices', component: () => import('../views/oa/Notice.vue'), meta: { title: '通知公告', permission: 'oa:notice' } },
      { path: 'oa/messages', name: 'Messages', component: () => import('../views/oa/Message.vue'), meta: { title: '消息中心', permission: 'oa:message' } },
      { path: 'oa/my-flow', name: 'MyFlow', component: () => import('../views/oa/MyFlow.vue'), meta: { title: '我的流程', permission: 'oa:myflow' } },
      { path: 'oa/todo', name: 'TodoFlow', component: () => import('../views/oa/TodoFlow.vue'), meta: { title: '待办事项', permission: 'oa:todo' } },
      { path: 'oa/flow-design', name: 'FlowDesign', component: () => import('../views/oa/FlowDesign.vue'), meta: { title: '流程设计', permission: 'oa:flowdesign' } },
      { path: 'oa/done', name: 'DoneFlow', component: () => import('../views/oa/DoneFlow.vue'), meta: { title: '已办事项', permission: 'oa:done' } },
      { path: 'oa/files', name: 'FileRecords', component: () => import('../views/oa/FileRecord.vue'), meta: { title: '文件管理', permission: 'oa:file' } },
      // 人力资源管理
      { path: 'hr/employees', name: 'EmployeeProfiles', component: () => import('../views/hr/EmployeeProfile.vue'), meta: { title: '员工档案', permission: 'hr:employee' } },
      { path: 'hr/attendance', name: 'Attendances', component: () => import('../views/hr/Attendance.vue'), meta: { title: '考勤管理', permission: 'hr:attendance' } },
      { path: 'hr/leave', name: 'LeaveRequests', component: () => import('../views/hr/LeaveRequest.vue'), meta: { title: '请假管理', permission: 'hr:leave' } },
      { path: 'hr/payroll', name: 'Payrolls', component: () => import('../views/hr/Payroll.vue'), meta: { title: '薪资管理', permission: 'hr:payroll' } },
      { path: 'hr/training', name: 'Trainings', component: () => import('../views/hr/Training.vue'), meta: { title: '培训管理', permission: 'hr:training' } }
    ]
  },
  { path: '/:pathMatch(.*)*', redirect: '/dashboard' }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

function hasPerm(perm) {
  const perms = JSON.parse(localStorage.getItem('permissions') || '[]')
  const user = JSON.parse(localStorage.getItem('user') || 'null')
  if (user && user.role === 'admin') return true
  return perms.includes(perm)
}

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
  } else if (to.meta.permission && !hasPerm(to.meta.permission)) {
    next('/dashboard')
  } else {
    next()
  }
})

export default router
