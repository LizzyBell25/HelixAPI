using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using HelixAPI.Data;
using HelixAPI.Model;

namespace HelixAPI.Controllers
{
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
        #endregion

        #region Update
        // PUT: api/v1/Indexes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIndex(Guid id, [FromBody] Model.Index index)
        {
            if (id != index.Index_Id)
                return BadRequest();

            _context.Entry(index).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IndexExists(id))
                    return NotFound();
                else
                    throw;
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

            patchDoc.ApplyTo(index, (Microsoft.AspNetCore.JsonPatch.Adapters.IObjectAdapter)ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _context.SaveChangesAsync();

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

        #region Helpers
        private bool IndexExists(Guid id)
        {
            return _context.Indexes.Any(i => i.Index_Id == id);
        }
        #endregion
    }
}