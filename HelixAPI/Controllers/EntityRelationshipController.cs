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
    public class EntityRelationshipsController(HelixContext context) : ControllerBase
    {
        private readonly HelixContext _context = context;

        // GET: api/EntityRelationships
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetEntityRelationships()
        {
            return await _context.EntityRelationships.ToListAsync();
        }

        // GET: api/EntityRelationships/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EntityRelationship>> GetEntityRelationship(Guid id)
        {
            var EntityRelationship = await _context.EntityRelationships.FindAsync(id);

            if (EntityRelationship == null)
                return NotFound();

            return EntityRelationship;
        }

        // PUT: api/EntityRelationships/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEntityRelationship(Guid id, EntityRelationship EntityRelationship)
        {
            if (id != EntityRelationship.RelationshipId)
            {
                return BadRequest();
            }

            _context.Entry(EntityRelationship).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntityRelationshipExists(id))
                    return NotFound();
                else
                    throw;
            }

            return Ok(EntityRelationship);
        }

        // POST: api/EntityRelationships
        [HttpPost]
        public async Task<ActionResult<EntityRelationship>> PostEntityRelationship(EntityRelationship EntityRelationship)
        {
            _context.EntityRelationships.Add(EntityRelationship);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEntityRelationship", new { id = EntityRelationship.RelationshipId }, EntityRelationship);
        }

        // DELETE: api/EntityRelationships/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntityRelationship(Guid id)
        {
            var EntityRelationship = await _context.EntityRelationships.FindAsync(id);
            if (EntityRelationship == null)
                return NotFound();

            _context.EntityRelationships.Remove(EntityRelationship);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EntityRelationshipExists(Guid id)
        {
            return _context.EntityRelationships.Any(e => e.RelationshipId == id);
        }
    }
}