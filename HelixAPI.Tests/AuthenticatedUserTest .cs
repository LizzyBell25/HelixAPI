using HelixAPI.Contexts;
using HelixAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;

namespace HelixAPI.Tests
{
    public class AuthenticatedUserTest : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var claims = new[] {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            context.HttpContext.User = principal;

            await next();
        }

        // In your test class
        [Fact]
        public async Task GetSources_ReturnsOkResult()
        {
            var options = new DbContextOptionsBuilder<HelixContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Arrange
            using var context = new HelixContext(options);
            var controller = new SourcesController(context);
            var filter = new AuthenticatedUserTest();
            var filters = new List<IFilterMetadata> { filter };

            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
            };

            var controllerContext = new ControllerContext(actionContext)
            {
                HttpContext = new DefaultHttpContext()
            };

            controller.ControllerContext = controllerContext;

            // Act
            var result = await controller.GetSources();

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}