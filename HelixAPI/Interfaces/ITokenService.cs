using HelixAPI.Models;

namespace HelixAPI.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
