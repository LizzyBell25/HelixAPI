using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using HelixAPI.Contexts;
using HelixAPI.Models;
using HelixAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using HelixAPI.Services;
using System.Linq;
using System.Linq.Dynamic.Core;
using HelixAPI.Interfaces;

namespace HelixAPI.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UsersController(HelixContext context, IUserService userService, ITokenService tokenService) : ControllerBase
    {
        private readonly HelixContext _context = context;
        private readonly IUserService _userService = userService;
        private readonly ITokenService _tokenService = tokenService;

        #region Create
        // POST: api/v1/Users/register
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<User>> RegisterUser([FromBody] User user)
        {
            if (user == null)
                return BadRequest();

            user.Password = _userService.HashPassword(user.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.User_Id }, user);
        }

        // POST: api/v1/Users/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == loginDto.Username);
            if (user == null || !_userService.VerifyPassword(user.Password, loginDto.Password))
                return Unauthorized("Invalid email or password.");

            var token = _tokenService.GenerateToken(user);
            var tokenResponse = new Dictionary<string, string>
            {
                { "Token", token }
            };

            return Ok(tokenResponse);
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

        #region Query
        // POST: api/v1/Users/query
        [HttpPost("query")]
        public async Task<IActionResult> QueryUsers([FromBody] QueryDto queryDto)
        {
            var users = await QueryHelpers.ProcessQueryFilters(queryDto, _context.Users).ToListAsync();

            if (users.Count == 0)
                return NotFound();

            if (string.IsNullOrEmpty(queryDto.Fields))
                return Ok(users);

            return Ok(QueryHelpers.FilterFields(users, queryDto));
        }
        #endregion

        #region Update
        // PUT: api/v1/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, [FromBody] User user)
        {
            if (id != user.User_Id)
                return BadRequest();

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser != null)
                _context.Entry(existingUser).State = EntityState.Detached;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_context.Users.Any(u => u.User_Id == id))
                    throw;
                else
                    return NotFound();
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

            patchDoc.ApplyTo(user, (error) =>
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
                if (_context.Users.Any(u => u.User_Id == id))
                    throw;
                else
                    return NotFound();
            }

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
    }
}