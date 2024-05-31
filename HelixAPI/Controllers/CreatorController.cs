using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using HelixAPI.Data;
using HelixAPI.Model;

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
                if (!CreatorExists(id))
                    return NotFound();
                else
                    throw;
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

        #region Helpers
        private bool CreatorExists(Guid id)
        {
            return _context.Creators.Any(c => c.Creator_Id == id);
        }
        #endregion
    }
}