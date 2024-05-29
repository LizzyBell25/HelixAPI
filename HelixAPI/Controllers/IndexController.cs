using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelixAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelixAPI.Model;

namespace HelixAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndexesController(HelixContext context) : ControllerBase
    {
        private readonly HelixContext _context = context;

        // GET: api/Indexes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HelixAPI.Model.Index>>> GetIndexes()
        {
            return await _context.Indexes.ToListAsync();
        }

        // GET: api/Indexes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HelixAPI.Model.Index>> GetIndex(Guid id)
        {
            var Index = await _context.Indexes.FindAsync(id);

            if (Index == null)
                return NotFound();

            return Index;
        }

        // PUT: api/Indexes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIndex(Guid id, HelixAPI.Model.Index Index)
        {
            if (id != Index.IndexId)
            {
                return BadRequest();
            }

            _context.Entry(Index).State = EntityState.Modified;

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

            return Ok(Index);
        }

        // POST: api/Indexes
        [HttpPost]
        public async Task<ActionResult<HelixAPI.Model.Index>> PostIndex(HelixAPI.Model.Index Index)
        {
            _context.Indexes.Add(Index);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetIndex", new { id = Index.IndexId }, Index);
        }

        // DELETE: api/Indexes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIndex(Guid id)
        {
            var Index = await _context.Indexes.FindAsync(id);
            if (Index == null)
                return NotFound();

            _context.Indexes.Remove(Index);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool IndexExists(Guid id)
        {
            return _context.Indexes.Any(e => e.IndexId == id);
        }
    }
}