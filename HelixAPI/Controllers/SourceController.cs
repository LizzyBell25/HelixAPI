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
    public class SourcesController(HelixContext context) : ControllerBase
    {
        private readonly HelixContext _context = context;

        // GET: api/Sources
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Source>>> GetSources()
        {
            return await _context.Sources.ToListAsync();
        }

        // GET: api/Sources/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Source>> GetSource(Guid id)
        {
            var Source = await _context.Sources.FindAsync(id);

            if (Source == null)
                return NotFound();

            return Source;
        }

        // PUT: api/Sources/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSource(Guid id, Source Source)
        {
            if (id != Source.SourceId)
            {
                return BadRequest();
            }

            _context.Entry(Source).State = EntityState.Modified;

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

            return Ok(Source);
        }

        // POST: api/Sources
        [HttpPost]
        public async Task<ActionResult<Source>> PostSource(Source Source)
        {
            _context.Sources.Add(Source);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSource", new { id = Source.SourceId }, Source);
        }

        // DELETE: api/Sources/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSource(Guid id)
        {
            var Source = await _context.Sources.FindAsync(id);
            if (Source == null)
                return NotFound();

            _context.Sources.Remove(Source);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SourceExists(Guid id)
        {
            return _context.Sources.Any(e => e.SourceId == id);
        }
    }
}