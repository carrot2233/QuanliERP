using Microsoft.AspNetCore.Mvc;
using QuanliERP.Api.Authorization;
using QuanliERP.Api.Data;
using QuanliERP.Api.Models;

namespace QuanliERP.Api.Controllers
{
    // 基础数据：读接口对所有登录用户开放（供各业务页面下拉引用），
    // 增删改接口按 base:* 权限码校验。
    [Route("api/[controller]")]
    public class CustomersController : CrudBaseController<Customer>
    {
        public CustomersController(AppDbContext db) : base(db) { }
        [RequirePermission("base:customer")] public override Task<IActionResult> Create(Customer item) => base.Create(item);
        [RequirePermission("base:customer")] public override Task<IActionResult> Update(int id, Customer item) => base.Update(id, item);
        [RequirePermission("base:customer")] public override Task<IActionResult> Delete(int id) => base.Delete(id);
    }

    [Route("api/[controller]")]
    public class SuppliersController : CrudBaseController<Supplier>
    {
        public SuppliersController(AppDbContext db) : base(db) { }
        [RequirePermission("base:supplier")] public override Task<IActionResult> Create(Supplier item) => base.Create(item);
        [RequirePermission("base:supplier")] public override Task<IActionResult> Update(int id, Supplier item) => base.Update(id, item);
        [RequirePermission("base:supplier")] public override Task<IActionResult> Delete(int id) => base.Delete(id);
    }

    [Route("api/[controller]")]
    public class MaterialsController : CrudBaseController<Material>
    {
        public MaterialsController(AppDbContext db) : base(db) { }
        [RequirePermission("base:material")] public override Task<IActionResult> Create(Material item) => base.Create(item);
        [RequirePermission("base:material")] public override Task<IActionResult> Update(int id, Material item) => base.Update(id, item);
        [RequirePermission("base:material")] public override Task<IActionResult> Delete(int id) => base.Delete(id);
    }

    [Route("api/[controller]")]
    public class ProductsController : CrudBaseController<Product>
    {
        public ProductsController(AppDbContext db) : base(db) { }
        [RequirePermission("base:product")] public override Task<IActionResult> Create(Product item) => base.Create(item);
        [RequirePermission("base:product")] public override Task<IActionResult> Update(int id, Product item) => base.Update(id, item);
        [RequirePermission("base:product")] public override Task<IActionResult> Delete(int id) => base.Delete(id);
    }

    [Route("api/[controller]")]
    public class WarehousesController : CrudBaseController<Warehouse>
    {
        public WarehousesController(AppDbContext db) : base(db) { }
        [RequirePermission("base:warehouse")] public override Task<IActionResult> Create(Warehouse item) => base.Create(item);
        [RequirePermission("base:warehouse")] public override Task<IActionResult> Update(int id, Warehouse item) => base.Update(id, item);
        [RequirePermission("base:warehouse")] public override Task<IActionResult> Delete(int id) => base.Delete(id);
    }

    [Route("api/[controller]")]
    public class EmployeesController : CrudBaseController<Employee>
    {
        public EmployeesController(AppDbContext db) : base(db) { }
        [RequirePermission("base:employee,hr:employee")] public override Task<IActionResult> Create(Employee item) => base.Create(item);
        [RequirePermission("base:employee,hr:employee")] public override Task<IActionResult> Update(int id, Employee item) => base.Update(id, item);
        [RequirePermission("base:employee,hr:employee")] public override Task<IActionResult> Delete(int id) => base.Delete(id);
    }
}
