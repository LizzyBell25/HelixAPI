using HelixAPI.Models;

namespace HelixAPI.Interfaces
{
    public interface IUserService
    {
        string HashPassword(string plainTextPassword);
        bool VerifyPassword(string hashedPassword, string password);
        Task RegisterAsync(string username, string plainTextPassword, string email);
        Task<User> GetUserByUsernameAsync(string username);
        Task<bool> LoginAsync(string username, string plainTextPassword);
    }
}
