using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using HelixAPI.Data;
using HelixAPI.Model;
using HelixAPI.Helpers;
using Microsoft.IdentityModel.Tokens;

namespace HelixAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SourcesController(HelixContext context) : ControllerBase
    {
        private readonly HelixContext _context = context;

        #region Create
        // POST: api/v1/Sources
        [HttpPost]
        public async Task<ActionResult<Source>> PostSource([FromBody] Source source)
        {
            _context.Sources.Add(source);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetSource", new { id = source.Source_Id }, source);
        }
        #endregion

        #region Read
        // GET: api/v1/Sources
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Source>>> GetSources()
        {
            return await _context.Sources.ToListAsync();
        }

        // GET: api/v1/Sources/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Source>> GetSource(Guid id)
        {
            var source = await _context.Sources.FindAsync(id);
            if (source == null)
                return NotFound();

            return source;
        }

        // GET: api/v1/Sources/query
        [HttpGet("query")]
        public async Task<IActionResult> QuerySources(
            [FromQuery] Guid? creator_id = null,
            [FromQuery] DateTime? publication_date = null,
            [FromQuery] string? publisher = null,
            [FromQuery] string? url = null,
            [FromQuery] Branch? branch = null,
            [FromQuery] ContentType? content_type = null,
            [FromQuery] Flag? flags = null,
            [FromQuery] Format? format = null,
            [FromQuery] int size = 100,
            [FromQuery] int offset = 0,
            [FromQuery] string sortBy = "",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] string? fields = null)
        {
            var query = _context.Sources.AsQueryable();

            if (creator_id != null)
                query = query.Where(s => s.Creator_Id == creator_id);

            if (publication_date != null)
                query = query.Where(s => s.Publication_Date == publication_date);

            if (!string.IsNullOrEmpty(publisher))
                query = query.Where(s => s.Publisher.Contains(publisher));

            if (!string.IsNullOrEmpty(url))
                query = query.Where(s => s.Url.Contains(url));

            if (branch != null)
                query = query.Where(s => s.Branch == branch);

            if (content_type != null)
                query = query.Where(s => s.Content_Type == content_type);

            if (flags != null)
                query = query.Where(s => s.Flags == flags);

            if (format != null)
                query = query.Where(s => s.Format == format);

            // Sorting
            query = sortBy.ToLower() switch
            {
                "creator_id" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(s => s.Creator_Id) : query.OrderBy(s => s.Creator_Id),
                "publication_date" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(s => s.Publication_Date) : query.OrderBy(s => s.Publication_Date),
                "publisher" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(s => s.Publisher) : query.OrderBy(s => s.Publisher),
                "url" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(s => s.Url) : query.OrderBy(s => s.Url),
                "branch" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(s => s.Branch) : query.OrderBy(s => s.Branch),
                "content_type" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(s => s.Content_Type) : query.OrderBy(s => s.Content_Type),
                "flags" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(s => s.Flags) : query.OrderBy(s => s.Flags),
                "format" => sortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(s => s.Format) : query.OrderBy(s => s.Format),
                _ => query.OrderBy(s => s.Source_Id),
            };
            var sources = await query.Skip(offset).Take(size).ToListAsync();

            if (sources.Count == 0)
                return NotFound();

            if (string.IsNullOrEmpty(fields))
                return Ok(sources);

            var selectedFields = fields.Split(',').Select(f => f.Trim()).ToList();
            var response = sources.Select(s => ConvertionHelpers.CreateExpandoObject(s, selectedFields));

            return Ok(response);
        }
        #endregion

        #region Update
        // PUT: api/v1/Sources/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSource(Guid id, [FromBody] Source source)
        {
            if (id != source.Source_Id)
                return BadRequest();

            _context.Entry(source).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_context.Sources.Any(s => s.Source_Id == id))
                    throw;
                else
                    return NotFound();
            }

            return NoContent();
        }

        // PATCH: api/v1/Sources/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchSource(Guid id, [FromBody] JsonPatchDocument<Source> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            var source = await _context.Sources.FindAsync(id);
            if (source == null)
                return NotFound();

            patchDoc.ApplyTo(source, (Microsoft.AspNetCore.JsonPatch.Adapters.IObjectAdapter)ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region Delete
        // DELETE: api/v1/Sources/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSource(Guid id)
        {
            var source = await _context.Sources.FindAsync(id);
            if (source == null)
                return NotFound();

            _context.Sources.Remove(source);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion
    }
}