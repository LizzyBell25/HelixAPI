using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using HelixAPI.Data;
using HelixAPI.Model;
using HelixAPI.Helpers;

namespace HelixAPI.Controllers
{
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

        // GET: api/v1/EntityRelationships/query
        [HttpGet("query")]
        public async Task<IActionResult> QueryRelationships(
            [FromQuery] Guid? entity1_id = null,
            [FromQuery] Guid? entity2_id = null,
            [FromQuery] RelationshipType? relationship_type = null,
            [FromQuery] int size = 100,
            [FromQuery] int offset = 0,
            [FromQuery] string sortBy = "",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] string? fields = null)
        {
            var query = _context.EntityRelationships.AsQueryable();

            if (entity1_id != null)
                query = query.Where(r => r.Entity1_Id == entity1_id);

            if (entity2_id != null)
                query = query.Where(r => r.Entity2_Id == entity2_id);

            if (relationship_type != null)
                query = query.Where(r => r.Relationship_Type == relationship_type);

            // Sorting
            query = sortBy.ToLower() switch
            {
                "entity1_id" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(r => r.Entity1_Id) : query.OrderBy(r => r.Entity1_Id),
                "entity2_id" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(r => r.Entity2_Id) : query.OrderBy(r => r.Entity2_Id),
                "relationship_type" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(r => r.Relationship_Type) : query.OrderBy(r => r.Relationship_Type),
                _ => query.OrderBy(e => e.Relationship_Id),
            };
            var relationships = await query.Skip(offset).Take(size).ToListAsync();

            if (relationships.Count == 0)
                return NotFound();

            if (string.IsNullOrEmpty(fields))
                return Ok(relationships);

            var selectedFields = fields.Split(',').Select(f => f.Trim()).ToList();
            var response = relationships.Select(r => ConvertionHelpers.CreateExpandoObject<EntityRelationship>(r, selectedFields));

            return Ok(response);
        }
        #endregion

        #region Update
        // PUT: api/v1/EntityRelationships/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRelationship(Guid id, [FromBody] EntityRelationship relationship)
        {
            if (id != relationship.Relationship_Id)
                return BadRequest();

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

            patchDoc.ApplyTo(relationship, (Microsoft.AspNetCore.JsonPatch.Adapters.IObjectAdapter)ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _context.SaveChangesAsync();

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