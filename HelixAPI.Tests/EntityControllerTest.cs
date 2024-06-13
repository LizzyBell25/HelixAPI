using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelixAPI.Controllers;
using HelixAPI.Models;
using HelixAPI.Models.ModelHelpers;
using HelixAPI.Contexts;
using Microsoft.AspNetCore.JsonPatch;
using System.Dynamic;

namespace HelixAPI.Tests
{
    public class EntitiesControllerTests : IDisposable
    {
        private readonly DbContextOptions<HelixContext> _options;

        #region Helpers
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

        private static Entity GenerateEntity(Guid id, string name)
        {
            var entity = new Entity
            {
                Entity_Id = id,
                Name = name,
                Type = Catagory.God,
                RowVersion = [0]
            };

            return entity;
        }
        #endregion

        #region GET
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
        public async Task GetUsers_ReturnsEmptyList_WhenNoSourcesExist()
        {
            var emptyDbContextOptions = new DbContextOptionsBuilder<HelixContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new HelixContext(emptyDbContextOptions);
            var controller = new EntitiesController(context);

            // Act
            var result = await controller.GetEntities();
            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Entity>>>(result);
            var returnValue = Assert.IsType<List<Entity>>(actionResult.Value);
            Assert.Empty(returnValue);
        }
        #endregion

        #region Query
        [Fact]
        public async Task QueryEntities_ReturnsFilteredAndPagedResults()
        {
            using var context = new HelixContext(_options);
            var controller = new EntitiesController(context);

            var queryDto = new QueryDto("Entity_Id")
            {
                Filters =
                [
                    new() { Property = "Type", Operation = "equals", Value = "God" }
                ],
                Size = 1,
                Offset = 0,
                SortBy = "Name",
                SortOrder = "desc",
                Fields = null
            };

            // Act
            var result = await controller.QueryEntities(queryDto);

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

            var queryDto = new QueryDto("Entity_Id")
            {
                Filters = [],
                Size = 100,
                Offset = 0,
                SortBy = "Name",
                SortOrder = "asc",
                Fields = "Name"
            };

            // Act
            var result = await controller.QueryEntities(queryDto);

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
        #endregion

        #region POST
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
        public async Task PostEntity_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new EntitiesController(context);
            controller.ModelState.AddModelError("Name", "Required");

            var entity = GenerateEntity(Guid.NewGuid(), string.Empty);

            // Act
            var result = await controller.PostEntity(entity);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Entity>>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }
        #endregion

        #region PUT
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
        #endregion

        #region PATCH
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
        #endregion

        #region Delete
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
        #endregion
    }
}