using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using HelixAPI.Contexts;
using HelixAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Linq.Dynamic.Core;
using HelixAPI.Models.ModelHelpers;

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
        public async Task<ActionResult<Models.Index>> PostIndex([FromBody] Models.Index index)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Indexes.Add(index);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetIndex", new { id = index.Index_Id }, index);
        }
        #endregion

        #region Read
        // GET: api/v1/Indexes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.Index>>> GetIndexes()
        {
            return await _context.Indexes.ToListAsync();
        }

        // GET: api/v1/Indexes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Index>> GetIndex(Guid id)
        {
            var index = await _context.Indexes.FindAsync(id);
            if (index == null)
                return NotFound();

            return index;
        }
        #endregion

        #region Query
        // GET: api/v1/Indexes/query
        [HttpGet("query")]
        public async Task<IActionResult> QueryIndexes([FromBody] QueryDto queryDto)
        {
            var indexes = await QueryHelpers.ProcessQueryFilters(queryDto, _context.Indexes).ToListAsync();

            if (indexes.Count == 0)
                return NotFound();

            if (string.IsNullOrEmpty(queryDto.Fields))
                return Ok(indexes);

            return Ok(QueryHelpers.FilterFields(indexes, queryDto));
        }
        #endregion

        #region Update
        // PUT: api/v1/Indexes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIndex(Guid id, [FromBody] Models.Index index)
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
        public async Task<IActionResult> PatchIndex(Guid id, [FromBody] JsonPatchDocument<Models.Index> patchDoc)
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