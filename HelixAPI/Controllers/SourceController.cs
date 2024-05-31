using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using HelixAPI.Data;
using HelixAPI.Model;

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
                if (!SourceExists(id))
                    return NotFound();
                else
                    throw;
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

        #region Helpers
        private bool SourceExists(Guid id)
        {
            return _context.Sources.Any(s => s.Source_Id == id);
        }
        #endregion
    }
}