using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using HelixAPI.Contexts;
using HelixAPI.Model;
using HelixAPI.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace HelixAPI.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class IndexesController(HelixContext context) : ControllerBase
    {
        private readonly HelixContext _context = context;

        #region Create
        // POST: api/v1/Indexes
        [HttpPost]
        public async Task<ActionResult<Model.Index>> PostIndex([FromBody] Model.Index index)
        {
            _context.Indexes.Add(index);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetIndex", new { id = index.Index_Id }, index);
        }
        #endregion

        #region Read
        // GET: api/v1/Indexes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Model.Index>>> GetIndexes()
        {
            return await _context.Indexes.ToListAsync();
        }

        // GET: api/v1/Indexes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Model.Index>> GetIndex(Guid id)
        {
            var index = await _context.Indexes.FindAsync(id);
            if (index == null)
                return NotFound();

            return index;
        }

        // GET: api/v1/Indexes/query
        [HttpGet("query")]
        public async Task<IActionResult> QueryIndexes(
            [FromQuery] Guid? entity_id = null,
            [FromQuery] Guid? indexed_by = null,
            [FromQuery] Guid? source_id = null,
            [FromQuery] string? location = null,
            [FromQuery] Subject? subject = null,
            [FromQuery] int size = 100,
            [FromQuery] int offset = 0,
            [FromQuery] string sortBy = "",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] string? fields = null)
        {
            var query = _context.Indexes.AsQueryable();

            if (entity_id != null)
                query = query.Where(i => i.Entity_Id == entity_id);

            if (indexed_by != null)
                query = query.Where(i => i.Indexed_By == indexed_by);

            if (source_id != null)
                query = query.Where(i => i.Source_Id == source_id);

            if (!string.IsNullOrEmpty(location))
                query = query.Where(i => i.Location.Contains(location));

            if (subject != null)
                query = query.Where(i => i.Subject == subject);

            // Sorting
            query = sortBy.ToLower() switch
            {
                "entity_id" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(i => i.Entity_Id) : query.OrderBy(i => i.Entity_Id),
                "indexed_by" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(i => i.Indexed_By) : query.OrderBy(i => i.Indexed_By),
                "source_id" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(i => i.Source_Id) : query.OrderBy(i => i.Source_Id),
                "location" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(i => i.Location) : query.OrderBy(i => i.Location),
                "subject" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(i => i.Subject) : query.OrderBy(i => i.Subject),
                _ => query.OrderBy(i => i.Entity_Id),
            };
            var indexes = await query.Skip(offset).Take(size).ToListAsync();

            if (indexes.Count == 0)
                return NotFound();

            if (string.IsNullOrEmpty(fields))
                return Ok(indexes);

            var selectedFields = fields.Split(',').Select(f => f.Trim()).ToList();
            var response = indexes.Select(i => ConvertionHelpers.CreateExpandoObject(i, selectedFields));

            return Ok(response);
        }
        #endregion

        #region Update
        // PUT: api/v1/Indexes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIndex(Guid id, [FromBody] Model.Index index)
        {
            if (id != index.Index_Id)
                return BadRequest();

            var existingIndex = await _context.Indexes.FindAsync(id);
            if (existingIndex != null)
                _context.Entry(existingIndex).State = EntityState.Detached;

            _context.Entry(index).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_context.Indexes.Any(i => i.Index_Id == id))
                    throw;
                else
                    return NotFound();
            }

            return NoContent();
        }

        // PATCH: api/v1/Indexes/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchIndex(Guid id, [FromBody] JsonPatchDocument<Model.Index> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            var index = await _context.Indexes.FindAsync(id);
            if (index == null)
                return NotFound();

            patchDoc.ApplyTo(index, (error) =>
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
                if (_context.Indexes.Any(i => i.Index_Id == id))
                    throw;
                else
                    return NotFound();
            }

            return NoContent();
        }
        #endregion

        #region Delete
        // DELETE: api/v1/Indexes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIndex(Guid id)
        {
            var index = await _context.Indexes.FindAsync(id);
            if (index == null)
                return NotFound();

            _context.Indexes.Remove(index);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion
    }
}