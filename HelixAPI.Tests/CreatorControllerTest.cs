using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelixAPI.Controllers;
using HelixAPI.Model;
using HelixAPI.Contexts;
using Microsoft.AspNetCore.JsonPatch;
using System.Dynamic;

namespace HelixAPI.Tests
{
    public class CreatorsControllerTests : IDisposable
    {
        private readonly DbContextOptions<HelixContext> _options;

        public CreatorsControllerTests()
        {
            _options = new DbContextOptionsBuilder<HelixContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use a unique database name for each test
                .Options;

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            using var context = new HelixContext(_options);
            var creators = new List<Creator>
            {
                GenerateCreator(Guid.NewGuid(), "User1", "Last1"),
                GenerateCreator(Guid.NewGuid(), "User2", "Last1")
            };

            context.Creators.AddRange(creators);
            context.SaveChanges();
        }

        public void Dispose()
        {
            using var context = new HelixContext(_options);
            context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task GetCreators_ReturnsAllCreators()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new CreatorsController(context);

            // Act
            var result = await controller.GetCreators();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Creator>>>(result);
            var returnValue = Assert.IsType<List<Creator>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetCreator_ReturnsCreator()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new CreatorsController(context);
            var id = context.Creators.First().Creator_Id;

            // Act
            var result = await controller.GetCreator(id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Creator>>(result);
            var returnValue = Assert.IsType<Creator>(actionResult.Value);
            Assert.Equal(id, returnValue.Creator_Id);
        }

        [Fact]
        public async Task GetCreator_ReturnsNotFound()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new CreatorsController(context);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.GetCreator(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostCreator_CreatesCreator()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new CreatorsController(context);
            var creator = GenerateCreator(Guid.NewGuid(), "User3", "Last3");

            // Act
            var result = await controller.PostCreator(creator);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Creator>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<Creator>(createdAtActionResult.Value);
            Assert.Equal(creator.Creator_Id, returnValue.Creator_Id);
        }

        [Fact]
        public async Task PutCreator_UpdatesCreator()
        {
            using var context = new HelixContext(_options);
            var controller = new CreatorsController(context);
            var id = context.Creators.First().Creator_Id;
            var creator = GenerateCreator(id, "User1", "Last2");

            // Act
            var result = await controller.PutCreator(id, creator);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedCreator = await context.Creators.FindAsync(id);
            Assert.Equal("Last2", updatedCreator.Last_Name);
        }

        [Fact]
        public async Task PutCreator_ReturnsBadRequest()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new CreatorsController(context);
            var id = Guid.NewGuid();
            var creator = GenerateCreator(Guid.NewGuid(), "User1", "Last2");

            // Act
            var result = await controller.PutCreator(id, creator);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteCreator_DeletesCreator()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new CreatorsController(context);
            var id = context.Creators.First().Creator_Id;

            // Act
            var result = await controller.DeleteCreator(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var deletedCreator = await context.Creators.FindAsync(id);
            Assert.Null(deletedCreator);
        }

        [Fact]
        public async Task DeleteCreator_ReturnsNotFound()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new CreatorsController(context);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.DeleteCreator(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PatchCreator_UpdatesCreator()
        {
            using var context = new HelixContext(_options);
            var controller = new CreatorsController(context);
            var id = context.Creators.First().Creator_Id;
            var patchDoc = new JsonPatchDocument<Creator>();
            patchDoc.Replace(c => c.Last_Name, "Last2");

            // Act
            var result = await controller.PatchCreator(id, patchDoc);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedCreator = await context.Creators.FindAsync(id);
            Assert.Equal("Last2", updatedCreator.Last_Name);
        }

        [Fact]
        public async Task QueryCreators_ReturnsFilteredAndPagedResults()
        {
            using var context = new HelixContext(_options);
            var controller = new CreatorsController(context);

            // Act
            var result = await controller.QueryCreators(last_name: "Last1", size: 1, offset: 0, sortBy: "Last_Name", sortOrder: "desc");

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<List<Creator>>(actionResult.Value);

            Assert.Single(returnValue);
            Assert.Equal("Last1", returnValue.First().Last_Name);
        }

        [Fact]
        public async Task QueryCreators_ReturnsSelectedFields()
        {
            using var context = new HelixContext(_options);
            var controller = new CreatorsController(context);

            // Act
            var result = await controller.QueryCreators(fields: "First_Name");

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<ExpandoObject>>(actionResult.Value);

            foreach (var item in returnValue)
            {
                var dict = Assert.IsType<ExpandoObject>(item);
                Assert.Contains("First_Name", dict);
                Assert.DoesNotContain("Last_Name", dict);
            }
        }

        private static Creator GenerateCreator(Guid id, string first, string last)
        {
            var creator = new Creator
            {
                Creator_Id = id,
                First_Name = first,
                Last_Name = last,
                Sort_Name = "test"
            };

            return creator;
        }
    }
}