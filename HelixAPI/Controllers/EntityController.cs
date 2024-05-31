using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using HelixAPI.Data;
using HelixAPI.Model;

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
        #endregion

        #region Update
        // PUT: api/v1/Entities/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEntity(Guid id, [FromBody] Entity entity)
        {
            if (id != entity.Entity_Id)
                return BadRequest();

            _context.Entry(entity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntityExists(id))
                    return NotFound();
                else
                    throw;
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

            patchDoc.ApplyTo(entity, (Microsoft.AspNetCore.JsonPatch.Adapters.IObjectAdapter)ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _context.SaveChangesAsync();

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

        #region Helpers
        private bool EntityExists(Guid id)
        {
            return _context.Entities.Any(e => e.Entity_Id == id);
        }
        #endregion
    }
}