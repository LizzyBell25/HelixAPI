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
    public class CreatorsController(HelixContext context) : ControllerBase
    {
        private readonly HelixContext _context = context;

        #region Create
        // POST: api/v1/Creators
        [HttpPost]
        public async Task<ActionResult<Creator>> PostCreator([FromBody] Creator creator)
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

        #region Query
        // POST: api/v1/Creators/query
        [HttpPost("query")]
        public async Task<IActionResult> QueryCreators([FromBody] QueryDto queryDto)
        {
            var creators = await QueryHelpers.ProcessQueryFilters(queryDto, _context.Creators).ToListAsync();

            if (creators.Count == 0)
                return NotFound();

            if (string.IsNullOrEmpty(queryDto.Fields))
                return Ok(creators);

            return Ok(QueryHelpers.FilterFields(creators, queryDto));
        }
        #endregion

        #region Update
        // PUT: api/v1/Creators/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCreator(Guid id, [FromBody] Creator creator)
        {
            if (id != creator.Creator_Id)
                return BadRequest();

            var existingCreator = await _context.Creators.FindAsync(id);
            if (existingCreator != null)
                _context.Entry(existingCreator).State = EntityState.Detached;

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

            patchDoc.ApplyTo(creator, (error) =>
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
                if (_context.Creators.Any(c => c.Creator_Id == id))
                    throw;
                else
                    return NotFound();
            }

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