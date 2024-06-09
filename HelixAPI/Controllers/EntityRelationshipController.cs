using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using HelixAPI.Contexts;
using HelixAPI.Models;
using HelixAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Linq.Dynamic.Core;

namespace HelixAPI.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EntityRelationshipsController(HelixContext context) : ControllerBase
    {
        private readonly HelixContext _context = context;

        #region Create
        // POST: api/v1/EntityRelationships
        [HttpPost]
        public async Task<ActionResult<EntityRelationship>> PostRelationship([FromBody] EntityRelationship relationship)
        {
            _context.EntityRelationships.Add(relationship);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetRelationship", new { id = relationship.Relationship_Id }, relationship);
        }
        #endregion

        #region Read
        // GET: api/v1/EntityRelationships
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetRelationships()
        {
            return await _context.EntityRelationships.ToListAsync();
        }

        // GET: api/v1/EntityRelationships/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EntityRelationship>> GetRelationship(Guid id)
        {
            var relationship = await _context.EntityRelationships.FindAsync(id);
            if (relationship == null)
                return NotFound();

            return relationship;
        }
        #endregion

        #region Query
        // POST: api/v1/EntityRelationships/query
        [HttpPost("query")]
        public async Task<IActionResult> QueryRelationships([FromBody] QueryDto queryDto)
        {
            var relationships = await QueryHelpers.ProcessQueryFilters(queryDto, _context.EntityRelationships).ToListAsync();

            if (relationships.Count == 0)
                return NotFound();

            if (string.IsNullOrEmpty(queryDto.Fields))
                return Ok(relationships);

            return Ok(QueryHelpers.FilterFields(relationships, queryDto));
        }
        #endregion

        #region Update
        // PUT: api/v1/EntityRelationships/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRelationship(Guid id, [FromBody] EntityRelationship relationship)
        {
            if (id != relationship.Relationship_Id)
                return BadRequest();

            var existingRelationship = await _context.EntityRelationships.FindAsync(id);
            if (existingRelationship != null)
                _context.Entry(existingRelationship).State = EntityState.Detached;

            _context.Entry(relationship).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_context.EntityRelationships.Any(r => r.Relationship_Id == id))
                    throw;
                else
                    return NotFound();
            }

            return NoContent();
        }

        // PATCH: api/v1/EntityRelationships/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchRelationship(Guid id, [FromBody] JsonPatchDocument<EntityRelationship> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            var relationship = await _context.EntityRelationships.FindAsync(id);
            if (relationship == null)
                return NotFound();

            patchDoc.ApplyTo(relationship, (error) =>
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
                if (_context.EntityRelationships.Any(r => r.Relationship_Id == id))
                    throw;
                else
                    return NotFound();
            }

            return NoContent();
        }
        #endregion

        #region Delete
        // DELETE: api/v1/EntityRelationships/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRelationship(Guid id)
        {
            var relationship = await _context.EntityRelationships.FindAsync(id);
            if (relationship == null)
                return NotFound();

            _context.EntityRelationships.Remove(relationship);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion
    }
}