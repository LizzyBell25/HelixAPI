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
    public class SourcesController(HelixContext context) : ControllerBase
    {
        private readonly HelixContext _context = context;

        #region Create
        // POST: api/v1/Sources
        [HttpPost]
        public async Task<ActionResult<Source>> PostSource([FromBody] Source source)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

        #region Query
        // GET: api/v1/Sources/query
        [HttpGet("query")]
        public async Task<IActionResult> QuerySources([FromBody] QueryDto queryDto)
        {
            var sources = await QueryHelpers.ProcessQueryFilters(queryDto, _context.Sources).ToListAsync();

            if (sources.Count == 0)
                return NotFound();

            if (string.IsNullOrEmpty(queryDto.Fields))
                return Ok(sources);

            return Ok(QueryHelpers.FilterFields(sources, queryDto));
        }
        #endregion

        #region Update
        // PUT: api/v1/Sources/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSource(Guid id, [FromBody] Source source)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != source.Source_Id)
                return BadRequest();

            var existingSource = await _context.Sources.FindAsync(id);
            if (existingSource != null)
                _context.Entry(existingSource).State = EntityState.Detached;

            // Track original RowVersion
            var originalRowVersion = source.RowVersion;

            _context.Entry(source).OriginalValues["RowVersion"] = originalRowVersion;
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

            // Track original RowVersion
            var originalRowVersion = source.RowVersion;

            // Apply the patch
            patchDoc.ApplyTo(source, (error) =>
            {
                ModelState.TryAddModelError(error.AffectedObject.ToString(), error.ErrorMessage);
            });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Entry(source).Property("RowVersion").OriginalValue = originalRowVersion;

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