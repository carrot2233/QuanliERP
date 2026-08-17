import api from './index'

export default {
  // 认证
  login: data => api.post('/Auth/login', data),
  me: () => api.get('/Auth/me'),

  // 基础数据
  customers: () => api.get('/Customers'),
  suppliers: () => api.get('/Suppliers'),
  materials: () => api.get('/Materials'),
  products: () => api.get('/Products'),
  warehouses: () => api.get('/Warehouses'),
  employees: () => api.get('/Employees'),
  crud: {
    list: (path, params) => api.get(path, { params }),
    create: (path, data) => api.post(path, data),
    update: (path, id, data) => api.put(`${path}/${id}`, data),
    remove: (path, id) => api.delete(`${path}/${id}`)
  },

  // 销售
  salesOrders: params => api.get('/SalesOrders', { params }),
  salesOrder: id => api.get(`/SalesOrders/${id}`),
  createSalesOrder: data => api.post('/SalesOrders', data),
  updateSalesOrder: (id, data) => api.put(`/SalesOrders/${id}`, data),
  salesOrderStatus: (id, status) => api.post(`/SalesOrders/${id}/status`, status, { headers: { 'Content-Type': 'application/json' } }),
  deleteSalesOrder: id => api.delete(`/SalesOrders/${id}`),
  deliveries: params => api.get('/Deliveries', { params }),
  createDelivery: data => api.post('/Deliveries', data),
  deleteDelivery: id => api.delete(`/Deliveries/${id}`),

  // 采购
  purchaseOrders: params => api.get('/PurchaseOrders', { params }),
  createPurchaseOrder: data => api.post('/PurchaseOrders', data),
  updatePurchaseOrder: (id, data) => api.put(`/PurchaseOrders/${id}`, data),
  purchaseOrderStatus: (id, status) => api.post(`/PurchaseOrders/${id}/status`, status, { headers: { 'Content-Type': 'application/json' } }),
  deletePurchaseOrder: id => api.delete(`/PurchaseOrders/${id}`),
  receipts: params => api.get('/PurchaseReceipts', { params }),
  createReceipt: data => api.post('/PurchaseReceipts', data),
  deleteReceipt: id => api.delete(`/PurchaseReceipts/${id}`),

  // 仓库
  inventory: params => api.get('/Inventory', { params }),
  inventoryWarnings: () => api.get('/Inventory/warnings'),
  ledger: params => api.get('/Inventory/ledger', { params }),
  stockInOut: data => api.post('/Inventory/stock', data),
  inventoryAdjust: data => api.post('/Inventory/adjust', data),
  workshopIn: data => api.post('/Inventory/workshop-in', data),

  // 生产
  productionPlans: params => api.get('/ProductionPlans', { params }),
  createProductionPlan: data => api.post('/ProductionPlans', data),
  updateProductionPlan: (id, data) => api.put(`/ProductionPlans/${id}`, data),
  productionPlanStatus: (id, status) => api.post(`/ProductionPlans/${id}/status`, status, { headers: { 'Content-Type': 'application/json' } }),
  deleteProductionPlan: id => api.delete(`/ProductionPlans/${id}`),
  productionOrders: params => api.get('/ProductionOrders', { params }),
  createProductionOrder: data => api.post('/ProductionOrders', data),
  updateProductionOrder: (id, data) => api.put(`/ProductionOrders/${id}`, data),
  deleteProductionOrder: id => api.delete(`/ProductionOrders/${id}`),
  dailyReports: params => api.get('/ProductionDailyReports', { params }),
  createDailyReport: data => api.post('/ProductionDailyReports', data),
  updateDailyReport: (id, data) => api.put(`/ProductionDailyReports/${id}`, data),
  deleteDailyReport: id => api.delete(`/ProductionDailyReports/${id}`),

  // 质量
  inspections: params => api.get('/QualityInspections', { params }),
  createInspection: data => api.post('/QualityInspections', data),
  updateInspection: (id, data) => api.put(`/QualityInspections/${id}`, data),
  deleteInspection: id => api.delete(`/QualityInspections/${id}`),
  measuringTools: params => api.get('/MeasuringTools', { params }),
  createMeasuringTool: data => api.post('/MeasuringTools', data),
  updateMeasuringTool: (id, data) => api.put(`/MeasuringTools/${id}`, data),
  deleteMeasuringTool: id => api.delete(`/MeasuringTools/${id}`),
  calibrationOverdue: () => api.get('/MeasuringTools/calibration-overdue'),
  toolApplies: params => api.get('/ToolApplies', { params }),
  createToolApply: data => api.post('/ToolApplies', data),
  updateToolApply: (id, data) => api.put(`/ToolApplies/${id}`, data),
  deleteToolApply: id => api.delete(`/ToolApplies/${id}`),
  toolScraps: params => api.get('/ToolScraps', { params }),
  createToolScrap: data => api.post('/ToolScraps', data),
  updateToolScrap: (id, data) => api.put(`/ToolScraps/${id}`, data),
  deleteToolScrap: id => api.delete(`/ToolScraps/${id}`),
  toolCalibrations: params => api.get('/ToolCalibrations', { params }),
  createToolCalibration: data => api.post('/ToolCalibrations', data),
  updateToolCalibration: (id, data) => api.put(`/ToolCalibrations/${id}`, data),
  deleteToolCalibration: id => api.delete(`/ToolCalibrations/${id}`),

  // 设备
  equipments: params => api.get('/Equipments', { params }),
  createEquipment: data => api.post('/Equipments', data),
  updateEquipment: (id, data) => api.put(`/Equipments/${id}`, data),
  deleteEquipment: id => api.delete(`/Equipments/${id}`),
  maintenances: params => api.get('/EquipmentMaintenances', { params }),
  createMaintenance: data => api.post('/EquipmentMaintenances', data),
  updateMaintenance: (id, data) => api.put(`/EquipmentMaintenances/${id}`, data),
  deleteMaintenance: id => api.delete(`/EquipmentMaintenances/${id}`),

  // 排班
  shifts: () => api.get('/Shifts'),
  createShift: data => api.post('/Shifts', data),
  updateShift: (id, data) => api.put(`/Shifts/${id}`, data),
  deleteShift: id => api.delete(`/Shifts/${id}`),
  workSchedules: params => api.get('/WorkSchedules', { params }),
  createWorkSchedule: data => api.post('/WorkSchedules', data),
  updateWorkSchedule: (id, data) => api.put(`/WorkSchedules/${id}`, data),
  deleteWorkSchedule: id => api.delete(`/WorkSchedules/${id}`),

  // 模具
  molds: params => api.get('/Molds', { params }),
  createMold: data => api.post('/Molds', data),
  updateMold: (id, data) => api.put(`/Molds/${id}`, data),
  deleteMold: id => api.delete(`/Molds/${id}`),
  moldPlans: params => api.get('/MoldPlans', { params }),
  createMoldPlan: data => api.post('/MoldPlans', data),
  updateMoldPlan: (id, data) => api.put(`/MoldPlans/${id}`, data),
  deleteMoldPlan: id => api.delete(`/MoldPlans/${id}`),

  // 系统
  users: params => api.get('/Users', { params }),
  createUser: data => api.post('/Users', data),
  updateUser: (id, data) => api.put(`/Users/${id}`, data),
  deleteUser: id => api.delete(`/Users/${id}`),

  // 驾驶舱
  overview: () => api.get('/Dashboard/overview'),
  invSummary: () => api.get('/Dashboard/inventory'),
  prodProgress: () => api.get('/Dashboard/production-progress'),
  quality: () => api.get('/Dashboard/quality'),
  salesTrend: () => api.get('/Dashboard/sales-trend'),
  prodTrend: () => api.get('/Dashboard/production-trend'),
  processDist: () => api.get('/Dashboard/process-distribution'),
  activities: () => api.get('/Dashboard/recent-activities')
}
