using Microsoft.AspNetCore.Mvc;
using QuanliERP.Api.Data;
using QuanliERP.Api.Models;

namespace QuanliERP.Api.Controllers
{
    [Route("api/[controller]")]
    public class CustomersController : CrudBaseController<Customer>
    {
        public CustomersController(AppDbContext db) : base(db) { }
    }

    [Route("api/[controller]")]
    public class SuppliersController : CrudBaseController<Supplier>
    {
        public SuppliersController(AppDbContext db) : base(db) { }
    }

    [Route("api/[controller]")]
    public class MaterialsController : CrudBaseController<Material>
    {
        public MaterialsController(AppDbContext db) : base(db) { }
    }

    [Route("api/[controller]")]
    public class ProductsController : CrudBaseController<Product>
    {
        public ProductsController(AppDbContext db) : base(db) { }
    }

    [Route("api/[controller]")]
    public class WarehousesController : CrudBaseController<Warehouse>
    {
        public WarehousesController(AppDbContext db) : base(db) { }
    }

    [Route("api/[controller]")]
    public class EmployeesController : CrudBaseController<Employee>
    {
        public EmployeesController(AppDbContext db) : base(db) { }
    }
}
