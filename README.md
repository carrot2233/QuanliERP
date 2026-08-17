# 鹤壁市全力模具制造有限公司 ERP 管理系统

基于需求文档《erp系统需求文档》构建的一体化制造企业 ERP，覆盖销售、采购、仓库、排班、生产、质量、设备、模具/工装夹具九大业务模块，并内置驾驶舱看板。

## 技术栈

- 后端：C# / .NET 9 + ASP.NET Core Web API + Entity Framework Core + SQL Server
- 前端：Vue 3 + Vite + Element Plus + Pinia + Vue Router + ECharts
- 认证：JWT（Bearer Token），按角色（admin/production/warehouse/quality/sales）控制权限

## 目录结构

```
QuanliERP/
├─ backend/QuanliERP.Api/     # 后端 Web API
├─ frontend/                  # 前端工程
└─ README.md
```

## 快速启动

### 1. 数据库

使用本机 SQL Server（Windows 身份验证），连接串配置在 `backend/QuanliERP.Api/appsettings.json`：

```
Server=.;Database=QuanliERP;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False
```

数据库由后端首次启动时自动创建（`EnsureCreated`），并自动注入示例数据（客户/供应商/材料/产品/仓库/员工/班次/设备/模具/生产计划/库存/质量数据等）。

### 2. 启动后端

```bash
cd backend/QuanliERP.Api
dotnet run --urls http://localhost:5080
```

- Swagger 文档：http://localhost:5080/swagger
- 健康检查：http://localhost:5080/api/Auth/login（POST）

### 3. 启动前端

```bash
cd frontend
npm install
npm run dev
```

访问 http://localhost:5173（开发服务器已配置 `/api` 代理到 5080）。

生产构建：

```bash
npm run build   # 产物输出到 frontend/dist
```

## 默认账号

| 角色 | 用户名 | 密码 |
| --- | --- | --- |
| 系统管理员 | admin | admin123 |
| 生产 | production | 123456 |
| 仓库 | warehouse | 123456 |
| 质量 | quality | 123456 |
| 销售 | sales | 123456 |

## 功能模块

| 模块 | 页面 | 说明 |
| --- | --- | --- |
| 驾驶舱 | 看板 | 指标卡、销售/采购趋势、生产产量/废品趋势、库存结构、工序产量分布、过程质量、生产进度、最近动态 |
| 销售管理 | 销售订单、发货管理 | 订单+明细，发货自动扣减库存并更新订单已发数量 |
| 采购管理 | 采购订单、到货管理 | 采购单+明细，到货入库自动增加材料库存 |
| 仓库管理 | 库存查询、库存流水、出入库操作、库存预警 | 支持采购入库/车间入库/生产领用/销售出库/盘点调整，自动记录流水并预警 |
| 排班管理 | 班次设置、排班计划 | 班次定义与员工排班 |
| 生产管理 | 生产计划、冲压产量单、生产日报 | 制号计划、按工序产量登记、日报+工序明细（落料/拉延/修边/冲孔侧冲） |
| 质量管理 | 质检记录、计量器具台账、量具申购、器具报废、检定处理 | 合格率统计、校准超期提醒 |
| 设备管理 | 设备台账、维护记录 | 保养/维修/点检记录，保养超期提醒 |
| 模具/工装夹具 | 模具台账、模具制造计划 | 模具台账，制造计划含 11 阶段（编程2D→编程3D→2D加工→淬火→投线→线切割→机钳装配→3D精加工→合模装配→研合→调试）进度跟踪 |
| 基础数据 | 客户、供应商、原材料、产品、仓库、员工 | 系统基础资料 |
| 系统管理 | 用户管理 | 仅 admin 可访问，管理登录账号与角色 |

## API 概览

- 鉴权：除 `POST /api/Auth/login` 外，其余接口均需请求头 `Authorization: Bearer <token>`
- 基础数据：`/api/Customers`、`/api/Suppliers`、`/api/Materials`、`/api/Products`、`/api/Warehouses`、`/api/Employees`
- 销售：`/api/SalesOrders`（含 `/{id}/status`）、`/api/Deliveries`
- 采购：`/api/PurchaseOrders`、`/api/PurchaseReceipts`
- 仓库：`/api/Inventory`（含 `/warnings`、`/ledger`、`/stock`、`/adjust`、`/workshop-in`）
- 排班：`/api/Shifts`、`/api/WorkSchedules`
- 生产：`/api/ProductionPlans`、`/api/ProductionOrders`、`/api/ProductionDailyReports`
- 质量：`/api/QualityInspections`、`/api/MeasuringTools`（含 `/calibration-overdue`）、`/api/ToolApplies`、`/api/ToolScraps`、`/api/ToolCalibrations`
- 设备：`/api/Equipments`、`/api/EquipmentMaintenances`
- 模具：`/api/Molds`、`/api/MoldPlans`
- 系统：`/api/Users`（admin）
- 看板：`/api/Dashboard/overview`、`/inventory`、`/production-progress`、`/quality`、`/sales-trend`、`/production-trend`、`/process-distribution`、`/recent-activities`
