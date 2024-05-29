using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelixAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelixAPI.Model;

namespace HelixAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntitiesController(HelixContext context) : ControllerBase
    {
        private readonly HelixContext _context = context;

        // GET: api/Entities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Entity>>> GetEntities()
        {
            return await _context.Entities.ToListAsync();
        }

        // GET: api/Entities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Entity>> GetEntity(Guid id)
        {
            var entity = await _context.Entities.FindAsync(id);

            if (entity == null)
                return NotFound();

            return entity;
        }

        // PUT: api/Entities/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEntity(Guid id, Entity entity)
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

            // Return the updated entity
            return Ok(entity);
        }


        // POST: api/Entities
        [HttpPost]
        public async Task<ActionResult<Entity>> PostEntity(Entity entity)
        {
            try
            {
                _context.Entities.Add(entity);
                await _context.SaveChangesAsync();
            } 
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return CreatedAtAction("GetEntity", new { id = entity.Entity_Id }, entity);
        }

        // DELETE: api/Entities/5
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

        private bool EntityExists(Guid id)
        {
            return _context.Entities.Any(e => e.Entity_Id == id);
        }
    }
}