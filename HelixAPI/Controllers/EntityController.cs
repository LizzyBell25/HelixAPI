using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using HelixAPI.Data;
using HelixAPI.Model;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using HelixAPI.Helpers;

namespace HelixAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EntitiesController(HelixContext context) : ControllerBase
    {
        private readonly HelixContext _context = context;

        #region Create
        // POST: api/v1/Entities
        [HttpPost]
        public async Task<ActionResult<Entity>> PostEntity([FromBody] Entity entity)
        {
            _context.Entities.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetEntity", new { id = entity.Entity_Id }, entity);
        }
        #endregion

        #region Read
        // GET: api/v1/Entities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Entity>>> GetEntities()
        {
            return await _context.Entities.ToListAsync();
        }

        // GET: api/v1/Entities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Entity>> GetEntity(Guid id)
        {
            var entity = await _context.Entities.FindAsync(id);
            if (entity == null)
                return NotFound();
            
            return entity;
        }

        // GET: api/v1/Entities/query
        [HttpGet("query")]
        public async Task<IActionResult> QueryEntities(
            [FromQuery] string? name = null,
            [FromQuery] string? description = null,
            [FromQuery] Catagory? type = null,
            [FromQuery] int size = 100,
            [FromQuery] int offset = 0,
            [FromQuery] string sortBy = "",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] string? fields = null)
        {
            var query = _context.Entities.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(e => e.Name.Contains(name));

            if (!string.IsNullOrEmpty(description))
                query = query.Where(e => e.Description.Contains(description));

            if (type != null)
                query = query.Where(e => e.Type == type);

            // Sorting
            query = sortBy.ToLower() switch
            {
                "name" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
                "description" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(e => e.Description) : query.OrderBy(e => e.Description),
                "type" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(e => e.Type) : query.OrderBy(e => e.Type),
                _ => query.OrderBy(e => e.Entity_Id),
            };
            var entities = await query.Skip(offset).Take(size).ToListAsync();

            if (entities.Count == 0)
                return NotFound();

            if (string.IsNullOrEmpty(fields))
                return Ok(entities);

            var selectedFields = fields.Split(',').Select(f => f.Trim()).ToList();
            var response = entities.Select(e => ConvertionHelpers.CreateExpandoObject<Entity>(e, selectedFields));

            return Ok(response);
        }
        #endregion

        #region Update
        // PUT: api/v1/Entities/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEntity(Guid id, [FromBody] Entity entity)
        {
            if (id != entity.Entity_Id)
                return BadRequest();

            var existingEntity = await _context.Entities.FindAsync(id);
            if (existingEntity != null)
                _context.Entry(existingEntity).State = EntityState.Detached;

            _context.Entry(entity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_context.Entities.Any(e => e.Entity_Id == id))
                    throw;
                else
                    return NotFound();
            }

            return NoContent();
        }

        // PATCH: api/v1/Entities/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchEntity(Guid id, [FromBody] JsonPatchDocument<Entity> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            var entity = await _context.Entities.FindAsync(id);
            if (entity == null)
                return NotFound();

            patchDoc.ApplyTo(entity, (error) =>
            {
                ModelState.TryAddModelError(error.AffectedObject.ToString(), error.ErrorMessage);
            });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_context.Entities.Any(e => e.Entity_Id == id))
                    throw;
                else
                    return NotFound();
            }

            return NoContent();
        }
        #endregion

        #region Delete
        // DELETE: api/v1/Entities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntity(Guid id)
        {
            var entity = await _context.Entities.FindAsync(id);
            if (entity == null)
                return NotFound();

            _context.Entities.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion
    }
}