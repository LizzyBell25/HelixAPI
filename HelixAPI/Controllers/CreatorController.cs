using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using helixapi.Data;
using helixapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelixAPI.Model;

namespace helixapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreatorsController(HelixContext context) : ControllerBase
    {
        private readonly HelixContext _context = context;

        // GET: api/Creators
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Creator>>> GetCreators()
        {
            return await _context.Creators.ToListAsync();
        }

        // GET: api/Creators/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Creator>> GetCreator(Guid id)
        {
            var Creator = await _context.Creators.FindAsync(id);

            if (Creator == null)
                return NotFound();

            return Creator;
        }

        // PUT: api/Creators/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCreator(Guid id, Creator Creator)
        {
            if (id != Creator.CreatorId)
            {
                return BadRequest();
            }

            _context.Entry(Creator).State = EntityState.Modified;

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

        // POST: api/Creators
        [HttpPost]
        public async Task<ActionResult<Creator>> PostCreator(Creator Creator)
        {
            _context.Creators.Add(Creator);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCreator", new { id = Creator.CreatorId }, Creator);
        }

        // DELETE: api/Creators/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCreator(Guid id)
        {
            var Creator = await _context.Creators.FindAsync(id);
            if (Creator == null)
                return NotFound();

            _context.Creators.Remove(Creator);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CreatorExists(Guid id)
        {
            return _context.Creators.Any(e => e.CreatorId == id);
        }
    }
}