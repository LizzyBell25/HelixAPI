using HelixAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace HelixAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController(ILogger<UserController> logger) : ControllerBase
    {
        private readonly ILogger<UserController> _logger = logger;

        //[HttpGet(Name = "GetUserList")]
        //public IEnumerable<User> Get()
        //{
        //    return Enumerable.Range(1,5).Select(index => new User
        //    (
        //        new Guid(),
        //        "test",
        //        "test@test.com",
        //        true
        //    ))
        //    .ToArray();
        //}

        [HttpGet(Name = "GetUser")]
        public IEnumerable<User> Get(Guid ID)
        {
            return new User[1] { new(
                ID,
                "test",
                "test@test.com",
                true
            )};
        }

        [HttpPost(Name = "CreateUser")]
        public IEnumerable<User> Create()
        {
            return new User[1] { new(
                new Guid(),
                "test",
                "test@test.com",
                true
            )};
        }

        [HttpPut(Name = "UpdateUser")]
        public IEnumerable<User> Update(Guid ID, string Name)
        {
            return new User[1] { new(
                ID,
                Name,
                "test@test.com",
                true
            )};
        }

        [HttpDelete(Name = "DeleteUser")]
        public IEnumerable<User> Delete(Guid ID)
        {
            return new User[1] { new(
                ID,
                "test",
                "test@test.com",
                true
            )};
        }
    }
}
