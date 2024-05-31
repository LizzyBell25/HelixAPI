using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using HelixAPI.Data;
using HelixAPI.Model;
using System.Dynamic;
using System.Reflection;
using HelixAPI.Helpers;

namespace HelixAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CreatorsController(HelixContext context) : ControllerBase
    {
        private readonly HelixContext _context = context;

        #region Create
        // POST: api/v1/Creators
        [HttpPost]
        public async Task<ActionResult<Entity>> PostCreator([FromBody] Creator creator)
        {
            _context.Creators.Add(creator);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetCreator", new { id = creator.Creator_Id }, creator);
        }
        #endregion

        #region Read
        // GET: api/v1/Creators
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Creator>>> GetCreators()
        {
            return await _context.Creators.ToListAsync();
        }

        // GET: api/v1/Creators/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Creator>> GetCreator(Guid id)
        {
            var creator = await _context.Creators.FindAsync(id);
            if (creator == null)
                return NotFound();

            return creator;
        }

        // GET: api/v1/Creators/query
        [HttpGet("query")]
        public async Task<IActionResult> QueryCreators(
            [FromQuery] string? first_name = null,
            [FromQuery] string? last_name = null,
            [FromQuery] string? sort_name = null,
            [FromQuery] int size = 100,
            [FromQuery] int offset = 0,
            [FromQuery] string sortBy = "",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] string? fields = null)
        {
            var query = _context.Creators.AsQueryable();

            if (!string.IsNullOrEmpty(first_name))
                query = query.Where(c => c.First_Name.Contains(first_name));

            if (!string.IsNullOrEmpty(last_name))
                query = query.Where(c => c.Last_Name.Contains(last_name));

            if (!string.IsNullOrEmpty(sort_name))
                query = query.Where(c => c.Sort_Name.Contains(sort_name));

            // Sorting
            query = sortBy.ToLower() switch
            {
                "first_name" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(c => c.First_Name) : query.OrderBy(c => c.First_Name),
                "last_name" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(c => c.Last_Name) : query.OrderBy(c => c.Last_Name),
                "sort_name" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(c => c.Sort_Name) : query.OrderBy(c => c.Sort_Name),
                _ => query.OrderBy(c => c.Creator_Id),
            };
            var creators = await query.Skip(offset).Take(size).ToListAsync();

            if (creators.Count == 0)
                return NotFound();

            if (string.IsNullOrEmpty(fields))
                return Ok(creators);

            var selectedFields = fields.Split(',').Select(f => f.Trim()).ToList();
            var response = creators.Select(c => ConvertionHelpers.CreateExpandoObject<Creator>(c, selectedFields));

            return Ok(response);
        }
        #endregion

        #region Update
        // PUT: api/v1/Creators/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCreator(Guid id, [FromBody] Creator creator)
        {
            if (id != creator.Creator_Id)
                return BadRequest();

            _context.Entry(creator).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_context.Creators.Any(c => c.Creator_Id == id))
                    throw;
                else
                    return NotFound();
            }

            return NoContent();
        }

        // PATCH: api/v1/Creators/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchCreator(Guid id, [FromBody] JsonPatchDocument<Creator> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            var creator = await _context.Creators.FindAsync(id);
            if (creator == null)
                return NotFound();

            patchDoc.ApplyTo(creator, (Microsoft.AspNetCore.JsonPatch.Adapters.IObjectAdapter)ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region Delete
        // DELETE: api/v1/Creators/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCreator(Guid id)
        {
            var creator = await _context.Creators.FindAsync(id);
            if (creator == null)
                return NotFound();

            _context.Creators.Remove(creator);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion
    }
}