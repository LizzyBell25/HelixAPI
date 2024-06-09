using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelixAPI.Controllers;
using HelixAPI.Models;
using HelixAPI.Contexts;
using Microsoft.AspNetCore.JsonPatch;
using System.Dynamic;

namespace HelixAPI.Tests
{
    public class RelationshipsControllerTests : IDisposable
    {
        private readonly DbContextOptions<HelixContext> _options;

        public RelationshipsControllerTests()
        {
            _options = new DbContextOptionsBuilder<HelixContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use a unique database name for each test
                .Options;

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            Guid e1 = Guid.NewGuid(), e2 = Guid.NewGuid(), e3 = Guid.NewGuid();
            using var context = new HelixContext(_options);
            var relationships = new List<EntityRelationship>
            {
                GenerateRelationship(Guid.NewGuid(), e1, e2),
                GenerateRelationship(Guid.NewGuid(), e1, e3)
            };

            context.EntityRelationships.AddRange(relationships);
            context.SaveChanges();
        }

        public void Dispose()
        {
            using var context = new HelixContext(_options);
            context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task GetRelationships_ReturnsAllRelationships()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new EntityRelationshipsController(context);

            // Act
            var result = await controller.GetRelationships();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<EntityRelationship>>>(result);
            var returnValue = Assert.IsType<List<EntityRelationship>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetRelationship_ReturnsRelationship()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new EntityRelationshipsController(context);
            var id = context.EntityRelationships.First().Relationship_Id;

            // Act
            var result = await controller.GetRelationship(id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<EntityRelationship>>(result);
            var returnValue = Assert.IsType<EntityRelationship>(actionResult.Value);
            Assert.Equal(id, returnValue.Relationship_Id);
        }

        [Fact]
        public async Task GetRelationship_ReturnsNotFound()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new EntityRelationshipsController(context);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.GetRelationship(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostRelationship_CreatesRelationship()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new EntityRelationshipsController(context);
            var relationship = GenerateRelationship(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            // Act
            var result = await controller.PostRelationship(relationship);

            // Assert
            var actionResult = Assert.IsType<ActionResult<EntityRelationship>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<EntityRelationship>(createdAtActionResult.Value);
            Assert.Equal(relationship.Relationship_Id, returnValue.Relationship_Id);
        }

        [Fact]
        public async Task PutRelationship_UpdatesRelationship()
        {
            using var context = new HelixContext(_options);
            var controller = new EntityRelationshipsController(context);
            var id = context.EntityRelationships.First().Relationship_Id;
            var e3 = Guid.NewGuid();
            var relationship = GenerateRelationship(id, Guid.NewGuid(), e3);

            // Act
            var result = await controller.PutRelationship(id, relationship);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedRelationship = await context.EntityRelationships.FindAsync(id);
            Assert.Equal(e3, updatedRelationship.Entity2_Id);
        }

        [Fact]
        public async Task PutRelationship_ReturnsBadRequest()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new EntityRelationshipsController(context);
            var id = Guid.NewGuid();
            var relationship = GenerateRelationship(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            // Act
            var result = await controller.PutRelationship(id, relationship);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteRelationship_DeletesRelationship()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new EntityRelationshipsController(context);
            var id = context.EntityRelationships.First().Relationship_Id;

            // Act
            var result = await controller.DeleteRelationship(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var deletedRelationship = await context.EntityRelationships.FindAsync(id);
            Assert.Null(deletedRelationship);
        }

        [Fact]
        public async Task DeleteRelationship_ReturnsNotFound()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new EntityRelationshipsController(context);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.DeleteRelationship(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PatchRelationship_UpdatesRelationship()
        {
            using var context = new HelixContext(_options);
            var controller = new EntityRelationshipsController(context);
            var id = context.EntityRelationships.First().Relationship_Id;
            var patchDoc = new JsonPatchDocument<EntityRelationship>();
            patchDoc.Replace(c => c.Relationship_Type, RelationshipType.Spouse);

            // Act
            var result = await controller.PatchRelationship(id, patchDoc);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedRelationship = await context.EntityRelationships.FindAsync(id);
            Assert.Equal(RelationshipType.Spouse, updatedRelationship.Relationship_Type);
        }

        [Fact]
        public async Task QueryRelationships_ReturnsFilteredAndPagedResults()
        {
            using var context = new HelixContext(_options);
            var controller = new EntityRelationshipsController(context);

            var queryDto = new QueryDto("Relationship_Id")
            {
                Filters =
                [
                    new() { Property = "Relationship_Type", Operation = "equals", Value = "Enemy" }
                ],
                Size = 1,
                Offset = 0,
                SortBy = "Relationship_Type",
                SortOrder = "desc",
                Fields = null
            };

            // Act
            var result = await controller.QueryRelationships(queryDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<List<EntityRelationship>>(actionResult.Value);

            Assert.Single(returnValue);
            Assert.Equal(RelationshipType.Enemy, returnValue.First().Relationship_Type);
        }

        [Fact]
        public async Task QueryRelationships_ReturnsSelectedFields()
        {
            using var context = new HelixContext(_options);
            var controller = new EntityRelationshipsController(context);

            var queryDto = new QueryDto("Relationship_Id")
            {
                Filters = [],
                Size = 100,
                Offset = 0,
                SortBy = "Relationship_Type",
                SortOrder = "asc",
                Fields = "Relationship_Type"
            };

            // Act
            var result = await controller.QueryRelationships(queryDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<ExpandoObject>>(actionResult.Value);

            foreach (var item in returnValue)
            {
                var dict = Assert.IsType<ExpandoObject>(item);
                Assert.Contains("Relationship_Type", dict);
                Assert.DoesNotContain("Entity1_Id", dict);
            }
        }

        private static EntityRelationship GenerateRelationship(Guid id, Guid E1, Guid E2)
        {
            var relationship = new EntityRelationship
            {
                Relationship_Id = id,
                Entity1_Id = E1,
                Entity2_Id = E2,
                Relationship_Type = RelationshipType.Enemy
            };

            return relationship;
        }
    }
}