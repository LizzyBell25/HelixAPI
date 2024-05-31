using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using HelixAPI.Data;
using HelixAPI.Model;

namespace HelixAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UsersController(HelixContext context) : ControllerBase
    {
        private readonly HelixContext _context = context;

        #region Create
        // POST: api/v1/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser([FromBody] User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetUser", new { id = user.User_Id }, user);
        }
        #endregion

        #region Read
        // GET: api/v1/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/v1/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return user;
        }
        #endregion

        #region Update
        // PUT: api/v1/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, [FromBody] User user)
        {
            if (id != user.User_Id)
                return BadRequest();

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // PATCH: api/v1/Users/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchUser(Guid id, [FromBody] JsonPatchDocument<User> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            patchDoc.ApplyTo(user, (Microsoft.AspNetCore.JsonPatch.Adapters.IObjectAdapter)ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region Delete
        // DELETE: api/v1/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region Helpers
        private bool UserExists(Guid id)
        {
            return _context.Users.Any(u => u.User_Id == id);
        }
        #endregion
    }
}