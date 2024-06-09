using System;
using System.Threading.Tasks;
using HelixAPI.Contexts;
using HelixAPI.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Text;
using Microsoft.Extensions.ObjectPool;
using System.Security.Cryptography;
using HelixAPI.Interfaces;

namespace HelixAPI.Services
{
    public class UserService(HelixContext context) : IUserService
    {
        private readonly HelixContext _context = context;

        public string HashPassword(string plainTextPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainTextPassword);
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        public async Task RegisterAsync(string username, string plainTextPassword, string email)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainTextPassword);

            var user = new User
            {
                User_Id = Guid.NewGuid(),
                Username = username,
                Password = hashedPassword,
                Email = email,
                Active = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> LoginAsync(string username, string plainTextPassword)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
                return false;

            return BCrypt.Net.BCrypt.Verify(plainTextPassword, user.Password);
        }
    }
}
