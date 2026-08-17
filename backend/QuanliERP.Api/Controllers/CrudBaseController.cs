using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanliERP.Api.Data;

namespace QuanliERP.Api.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class CrudBaseController<T> : ControllerBase where T : class
    {
        protected readonly AppDbContext _db;
        protected readonly DbSet<T> _set;
        public CrudBaseController(AppDbContext db) { _db = db; _set = db.Set<T>(); }

        [HttpGet]
        public virtual async Task<IActionResult> GetAll([FromQuery] string? keyword)
        {
            var list = await _set.AsNoTracking().ToListAsync();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var props = typeof(T).GetProperties()
                    .Where(p => p.PropertyType == typeof(string))
                    .Select(p => p.Name).ToArray();
                list = list.Where(item => props.Any(pn =>
                {
                    var v = typeof(T).GetProperty(pn)?.GetValue(item) as string;
                    return v != null && v.Contains(keyword, StringComparison.OrdinalIgnoreCase);
                })).ToList();
            }
            return Ok(list);
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById(int id)
        {
            var item = await _set.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Create(T item)
        {
            PrepareNew(item);
            _set.Add(item);
            await _db.SaveChangesAsync();
            return Ok(item);
        }

        // 子类可重写以在新增前自动生成编号等
        protected virtual void PrepareNew(T item) { }

        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Update(int id, T item)
        {
            var key = _db.Entry(item).Property("Id").CurrentValue;
            if (key == null || (int)key != id) return BadRequest(new { message = "ID 不匹配" });
            _set.Update(item);
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            var item = await _set.FindAsync(id);
            if (item == null) return NotFound();
            _set.Remove(item);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }
}
