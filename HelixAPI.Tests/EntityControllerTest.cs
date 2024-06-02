using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelixAPI.Controllers;
using HelixAPI.Model;
using HelixAPI.Data;
using Microsoft.AspNetCore.JsonPatch;
using System.Dynamic;

namespace HelixAPI.Tests
{
    public class EntitiesControllerTests : IDisposable
    {
        private readonly DbContextOptions<HelixContext> _options;

        public EntitiesControllerTests()
        {
            _options = new DbContextOptionsBuilder<HelixContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use a unique database name for each test
                .Options;

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            using var context = new HelixContext(_options);
            var entitys = new List<Entity>
            {
                GenerateEntity(Guid.NewGuid(), "Entity1"),
                GenerateEntity(Guid.NewGuid(), "Entity2")
            };

            context.Entities.AddRange(entitys);
            context.SaveChanges();
        }

        public void Dispose()
        {
            using var context = new HelixContext(_options);
            context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task GetEntities_ReturnsAllEntities()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new EntitiesController(context);

            // Act
            var result = await controller.GetEntities();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Entity>>>(result);
            var returnValue = Assert.IsType<List<Entity>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetEntity_ReturnsEntity()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new EntitiesController(context);
            var id = context.Entities.First().Entity_Id;

            // Act
            var result = await controller.GetEntity(id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Entity>>(result);
            var returnValue = Assert.IsType<Entity>(actionResult.Value);
            Assert.Equal(id, returnValue.Entity_Id);
        }

        [Fact]
        public async Task GetEntity_ReturnsNotFound()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new EntitiesController(context);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.GetEntity(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostEntity_CreatesEntity()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new EntitiesController(context);
            var entity = GenerateEntity(Guid.NewGuid(), "Entity3");

            // Act
            var result = await controller.PostEntity(entity);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Entity>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<Entity>(createdAtActionResult.Value);
            Assert.Equal(entity.Entity_Id, returnValue.Entity_Id);
        }

        [Fact]
        public async Task PutEntity_UpdatesEntity()
        {
            using var context = new HelixContext(_options);
            var controller = new EntitiesController(context);
            var id = context.Entities.First().Entity_Id;
            var entity = GenerateEntity(id, "UpdatedEntity");

            // Act
            var result = await controller.PutEntity(id, entity);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedEntity = await context.Entities.FindAsync(id);
            Assert.Equal("UpdatedEntity", updatedEntity.Name);
        }

        [Fact]
        public async Task PutEntity_ReturnsBadRequest()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new EntitiesController(context);
            var id = Guid.NewGuid();
            var entity = GenerateEntity(Guid.NewGuid(), "Entity1");

            // Act
            var result = await controller.PutEntity(id, entity);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteEntity_DeletesEntity()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new EntitiesController(context);
            var id = context.Entities.First().Entity_Id;

            // Act
            var result = await controller.DeleteEntity(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var deletedEntity = await context.Entities.FindAsync(id);
            Assert.Null(deletedEntity);
        }

        [Fact]
        public async Task DeleteEntity_ReturnsNotFound()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new EntitiesController(context);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.DeleteEntity(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PatchEntity_UpdatesEntity()
        {
            using var context = new HelixContext(_options);
            var controller = new EntitiesController(context);
            var id = context.Entities.First().Entity_Id;
            var patchDoc = new JsonPatchDocument<Entity>();
            patchDoc.Replace(e => e.Name, "UpdatedPublisher");

            // Act
            var result = await controller.PatchEntity(id, patchDoc);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedEntity = await context.Entities.FindAsync(id);
            Assert.Equal("UpdatedPublisher", updatedEntity.Name);
        }

        [Fact]
        public async Task QueryEntities_ReturnsFilteredAndPagedResults()
        {
            using var context = new HelixContext(_options);
            var controller = new EntitiesController(context);

            // Act
            var result = await controller.QueryEntities(type: Catagory.God, size: 1, offset: 0, sortBy: "Name", sortOrder: "desc");

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<List<Entity>>(actionResult.Value);

            Assert.Single(returnValue);
            Assert.Equal("Entity2", returnValue.First().Name);
        }

        [Fact]
        public async Task QueryEntities_ReturnsSelectedFields()
        {
            using var context = new HelixContext(_options);
            var controller = new EntitiesController(context);

            // Act
            var result = await controller.QueryEntities(fields: "Name");

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<ExpandoObject>>(actionResult.Value);

            foreach (var item in returnValue)
            {
                var dict = Assert.IsType<ExpandoObject>(item);
                Assert.Contains("Name", dict);
                Assert.DoesNotContain("Type", dict);
            }
        }

        private static Entity GenerateEntity(Guid id, string publisher)
        {
            var entity = new Entity
            {
                Entity_Id = id,
                Name = publisher,
                Type = Catagory.God
            };

            return entity;
        }
    }
}