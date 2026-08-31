namespace QuanliERP.Api.Services
{
    // 菜单目录定义（与前端左侧菜单一致），权限码为最小粒度单位（一级+二级菜单）
    public static class MenuCatalog
    {
        public class MenuDef
        {
            public string Code { get; set; } = "";
            public string Title { get; set; } = "";
            public string? Path { get; set; }
            public List<MenuDef> Children { get; set; } = new();
        }

        public static readonly List<MenuDef> Menus = new()
        {
            new MenuDef { Code = "dashboard", Title = "驾驶舱管理", Path = "/dashboard" },
            new MenuDef { Code = "base", Title = "基础数据", Children = new List<MenuDef>
            {
                new MenuDef { Code = "base:customer", Title = "客户管理", Path = "/base/customers" },
                new MenuDef { Code = "base:supplier", Title = "供应商管理", Path = "/base/suppliers" },
                new MenuDef { Code = "base:material", Title = "原材料管理", Path = "/base/materials" },
                new MenuDef { Code = "base:product", Title = "产品管理", Path = "/base/products" },
                new MenuDef { Code = "base:warehouse", Title = "仓库管理", Path = "/base/warehouses" },
                new MenuDef { Code = "base:employee", Title = "员工管理", Path = "/base/employees" }
            }},
            new MenuDef { Code = "sales", Title = "销售管理", Children = new List<MenuDef>
            {
                new MenuDef { Code = "sales:order", Title = "销售订单", Path = "/sales/orders" },
                new MenuDef { Code = "sales:delivery", Title = "发货管理", Path = "/sales/deliveries" }
            }},
            new MenuDef { Code = "purchase", Title = "采购管理", Children = new List<MenuDef>
            {
                new MenuDef { Code = "purchase:order", Title = "采购订单", Path = "/purchase/orders" },
                new MenuDef { Code = "purchase:receipt", Title = "到货管理", Path = "/purchase/receipts" }
            }},
            new MenuDef { Code = "warehouse", Title = "仓库管理", Children = new List<MenuDef>
            {
                new MenuDef { Code = "warehouse:inventory", Title = "库存查询", Path = "/warehouse/inventory" },
                new MenuDef { Code = "warehouse:stock-in", Title = "入库管理", Path = "/warehouse/stock-in" },
                new MenuDef { Code = "warehouse:stock-out", Title = "出库管理", Path = "/warehouse/stock-out" },
                new MenuDef { Code = "warehouse:ledger", Title = "库存流水", Path = "/warehouse/ledger" },
                new MenuDef { Code = "warehouse:warning", Title = "库存预警", Path = "/warehouse/warnings" }
            }},
            new MenuDef { Code = "schedule", Title = "排班管理", Children = new List<MenuDef>
            {
                new MenuDef { Code = "schedule:shift", Title = "班次设置", Path = "/schedule/shifts" },
                new MenuDef { Code = "schedule:work", Title = "排班计划", Path = "/schedule/work" }
            }},
            new MenuDef { Code = "production", Title = "生产管理", Children = new List<MenuDef>
            {
                new MenuDef { Code = "production:plan", Title = "生产计划", Path = "/production/plans" },
                new MenuDef { Code = "production:order", Title = "冲压产量单", Path = "/production/orders" },
                new MenuDef { Code = "production:daily", Title = "生产日报", Path = "/production/daily" }
            }},
            new MenuDef { Code = "mold", Title = "模具/工装夹具管理", Children = new List<MenuDef>
            {
                new MenuDef { Code = "mold:list", Title = "模具台账", Path = "/mold/list" },
                new MenuDef { Code = "mold:plan", Title = "模具制造计划", Path = "/mold/plans" }
            }},
            new MenuDef { Code = "quality", Title = "质量管理", Children = new List<MenuDef>
            {
                new MenuDef { Code = "quality:inspection", Title = "质检记录", Path = "/quality/inspections" },
                new MenuDef { Code = "quality:tool", Title = "计量器具台账", Path = "/quality/tools" },
                new MenuDef { Code = "quality:toolapply", Title = "量具申购", Path = "/quality/toolapply" },
                new MenuDef { Code = "quality:toolscrap", Title = "器具报废", Path = "/quality/toolscrap" },
                new MenuDef { Code = "quality:calibration", Title = "检定处理", Path = "/quality/calibration" }
            }},
            new MenuDef { Code = "equipment", Title = "设备管理", Children = new List<MenuDef>
            {
                new MenuDef { Code = "equipment:list", Title = "设备台账", Path = "/equipment/list" },
                new MenuDef { Code = "equipment:maintenance", Title = "维护记录", Path = "/equipment/maintenance" }
            }},
            new MenuDef { Code = "oa", Title = "协同办公", Children = new List<MenuDef>
            {
                new MenuDef { Code = "oa:notice", Title = "通知公告", Path = "/oa/notices" },
                new MenuDef { Code = "oa:message", Title = "消息中心", Path = "/oa/messages" },
                new MenuDef { Code = "oa:myflow", Title = "我的流程", Path = "/oa/my-flow" },
                new MenuDef { Code = "oa:todo", Title = "待办事项", Path = "/oa/todo" },
                new MenuDef { Code = "oa:flowdesign", Title = "流程设计", Path = "/oa/flow-design" },
                new MenuDef { Code = "oa:done", Title = "已办事项", Path = "/oa/done" },
                new MenuDef { Code = "oa:file", Title = "文件管理", Path = "/oa/files" }
            }},
            new MenuDef { Code = "hr", Title = "人力资源管理", Children = new List<MenuDef>
            {
                new MenuDef { Code = "hr:employee", Title = "员工档案", Path = "/hr/employees" },
                new MenuDef { Code = "hr:attendance", Title = "考勤管理", Path = "/hr/attendance" },
                new MenuDef { Code = "hr:leave", Title = "请假管理", Path = "/hr/leave" },
                new MenuDef { Code = "hr:payroll", Title = "薪资管理", Path = "/hr/payroll" },
                new MenuDef { Code = "hr:training", Title = "培训管理", Path = "/hr/training" }
            }},
            new MenuDef { Code = "system", Title = "系统管理", Children = new List<MenuDef>
            {
                new MenuDef { Code = "system:user", Title = "用户管理", Path = "/system/users" },
                new MenuDef { Code = "system:role", Title = "角色权限", Path = "/system/roles" }
            }}
        };

        public static List<string> AllCodes() => Menus.SelectMany(Flatten).Select(m => m.Code).ToList();

        public static List<MenuDef> Flatten(MenuDef m)
        {
            var list = new List<MenuDef> { m };
            foreach (var c in m.Children) list.AddRange(Flatten(c));
            return list;
        }
    }
}
