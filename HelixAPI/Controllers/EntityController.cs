using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using HelixAPI.Contexts;
using HelixAPI.Models;
using HelixAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Linq.Dynamic.Core;
using HelixAPI.Models.ModelHelpers;

namespace HelixAPI.Controllers
{
    [Authorize]
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

        #region Query
        // GET: api/v1/Entities/query
        [HttpGet("query")]
        public async Task<IActionResult> QueryEntities([FromBody] QueryDto queryDto)
        {
            var entities = await QueryHelpers.ProcessQueryFilters(queryDto, _context.Entities).ToListAsync();

            if (entities.Count == 0)
                return NotFound();

            if (string.IsNullOrEmpty(queryDto.Fields))
                return Ok(entities);

            return Ok(QueryHelpers.FilterFields(entities, queryDto));
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