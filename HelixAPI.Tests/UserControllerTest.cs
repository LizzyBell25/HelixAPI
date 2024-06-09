using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelixAPI.Controllers;
using HelixAPI.Models;
using HelixAPI.Contexts;
using Microsoft.AspNetCore.JsonPatch;
using System.Dynamic;
using Moq;
using HelixAPI.Services;

namespace HelixAPI.Tests
{
    public class UsersControllerTests : IDisposable
    {
        private readonly DbContextOptions<HelixContext> _options;
        private readonly Mock<UserService> _userServiceMock;
        private readonly Mock<TokenService> _tokenServiceMock;

        public UsersControllerTests()
        {
            _options = new DbContextOptionsBuilder<HelixContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use a unique database name for each test
                .Options;

            var context = new HelixContext(_options);

            var jwtSettings = new JwtSettings
            {
                Key = "hnt03QCXbgYDpZyjYzitaY94CQsUiixK",
                Issuer = "http://localhost",
                Audience = "Heathen Education And Resource Treasury Test",
                ExpiryMinutes = 5,
            };

            _userServiceMock = new Mock<UserService>(context);
            _tokenServiceMock = new Mock<TokenService>(jwtSettings);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            using var context = new HelixContext(_options);
            var users = new List<User>
            {
                GenerateUser(Guid.NewGuid(), "User1", true),
                GenerateUser(Guid.NewGuid(), "User2", true)
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }

        public void Dispose()
        {
            using var context = new HelixContext(_options);
            context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task GetUsers_ReturnsAllUsers()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new UsersController(context, _userServiceMock.Object, _tokenServiceMock.Object);

            // Act
            var result = await controller.GetUsers();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<User>>>(result);
            var returnValue = Assert.IsType<List<User>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetUser_ReturnsUser()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new UsersController(context, _userServiceMock.Object, _tokenServiceMock.Object);
            var id = context.Users.First().User_Id;

            // Act
            var result = await controller.GetUser(id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<User>>(result);
            var returnValue = Assert.IsType<User>(actionResult.Value);
            Assert.Equal(id, returnValue.User_Id);
        }

        [Fact]
        public async Task GetUser_ReturnsNotFound()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new UsersController(context, _userServiceMock.Object, _tokenServiceMock.Object);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.GetUser(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task RegisterUser_CreatesUser()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new UsersController(context, _userServiceMock.Object, _tokenServiceMock.Object);
            var user = GenerateUser(Guid.NewGuid(), "User3", true);

            // Act
            var result = await controller.RegisterUser(user);

            // Assert
            var actionResult = Assert.IsType<ActionResult<User>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<User>(createdAtActionResult.Value);
            Assert.Equal(user.User_Id, returnValue.User_Id);
        }

        [Fact]
        public async Task Login_ReturnsToken_WhenCredentialsAreValid()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new UsersController(context, _userServiceMock.Object, _tokenServiceMock.Object);

            var password = "testPassword";
            var hashedPassword = _userServiceMock.Object.HashPassword(password);
            var user = GenerateUser(Guid.NewGuid(), "User4", true);
            user.Email = "user4@example.com";
            user.Password = hashedPassword;

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var loginDto = new LoginDto
            {
                Username = user.Username,
                Password = password
            };

            // Act
            var result = await controller.LoginUser(loginDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Dictionary<string, string>>(actionResult.Value);
            Assert.True(returnValue.ContainsKey("Token"));
            string token = returnValue["Token"];
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new UsersController(context, _userServiceMock.Object, _tokenServiceMock.Object);

            var password = "testPassword";
            var hashedPassword = _userServiceMock.Object.HashPassword(password);
            var user = GenerateUser(Guid.NewGuid(), "User4", true);
            user.Password = hashedPassword;

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var loginDto = new LoginDto
            {
                Username = user.Username,
                Password = "wrongPassword"
            };

            // Act
            var result = await controller.LoginUser(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task PutUser_UpdatesUser()
        {
            using var context = new HelixContext(_options);
            var controller = new UsersController(context, _userServiceMock.Object, _tokenServiceMock.Object);
            var id = context.Users.First().User_Id;
            var user = GenerateUser(id, "User1", false);

            // Act
            var result = await controller.PutUser(id, user);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedUser = await context.Users.FindAsync(id);
            Assert.Equal(false, updatedUser.Active);
        }

        [Fact]
        public async Task PutUser_ReturnsBadRequest()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new UsersController(context, _userServiceMock.Object, _tokenServiceMock.Object);
            var id = Guid.NewGuid();
            var user = GenerateUser(Guid.NewGuid(), "User1", true);

            // Act
            var result = await controller.PutUser(id, user);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteUser_DeletesUser()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new UsersController(context, _userServiceMock.Object, _tokenServiceMock.Object);
            var id = context.Users.First().User_Id;

            // Act
            var result = await controller.DeleteUser(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var deletedUser = await context.Users.FindAsync(id);
            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNotFound()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new UsersController(context, _userServiceMock.Object, _tokenServiceMock.Object);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.DeleteUser(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PatchUser_UpdatesUser()
        {
            using var context = new HelixContext(_options);
            var controller = new UsersController(context, _userServiceMock.Object, _tokenServiceMock.Object);
            var id = context.Users.First().User_Id;
            var patchDoc = new JsonPatchDocument<User>();
            patchDoc.Replace(u => u.Active, false);

            // Act
            var result = await controller.PatchUser(id, patchDoc);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedUser = await context.Users.FindAsync(id);
            Assert.Equal(false, updatedUser.Active);
        }

        [Fact]
        public async Task QueryUsers_ReturnsFilteredAndPagedResults()
        {
            using var context = new HelixContext(_options);
            var controller = new UsersController(context, _userServiceMock.Object, _tokenServiceMock.Object);

            var queryDto = new QueryDto("User_ID")
            {
                Filters =
                [
                    new() { Property = "Active", Operation = "equals", Value = "true" }
                ],
                Size = 1,
                Offset = 0,
                SortBy = "Username",
                SortOrder = "desc",
                Fields = null
            };

            // Act
            var result = await controller.QueryUsers(queryDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<List<User>>(actionResult.Value);

            Assert.Single(returnValue);
            Assert.Equal(true, returnValue.First().Active);
        }

        [Fact]
        public async Task QueryUsers_ReturnsSelectedFields()
        {
            using var context = new HelixContext(_options);
            var controller = new UsersController(context, _userServiceMock.Object, _tokenServiceMock.Object);

            var queryDto = new QueryDto("User_ID")
            {
                Filters = [],
                Size = 100,
                Offset = 0,
                SortBy = "Username",
                SortOrder = "asc",
                Fields = "Username"
            };

            // Act
            var result = await controller.QueryUsers(queryDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<ExpandoObject>>(actionResult.Value);

            foreach (var item in returnValue)
            {
                var dict = Assert.IsType<ExpandoObject>(item);
                Assert.Contains("Username", dict);
                Assert.DoesNotContain("Email", dict);
            }
        }

        private static User GenerateUser(Guid id, string username, bool active)
        {
            var user = new User
            {
                User_Id = id,
                Username = username,
                Email = "test@Email",
                Password = "Test",
                Active = active
            };

            return user;
        }
    }
}
