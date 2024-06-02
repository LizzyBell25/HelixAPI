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
    public class IndexesControllerTests : IDisposable
    {
        private readonly DbContextOptions<HelixContext> _options;

        public IndexesControllerTests()
        {
            _options = new DbContextOptionsBuilder<HelixContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use a unique database name for each test
                .Options;

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            using var context = new HelixContext(_options);
            var indexs = new List<Model.Index>
            {
                GenerateIndex(Guid.NewGuid(), Guid.NewGuid(), Subject.Jotun),
                GenerateIndex(Guid.NewGuid(), Guid.NewGuid(), Subject.Jotun)
            };

            context.Indexes.AddRange(indexs);
            context.SaveChanges();
        }

        public void Dispose()
        {
            using var context = new HelixContext(_options);
            context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task GetIndexes_ReturnsAllIndexes()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);

            // Act
            var result = await controller.GetIndexes();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Model.Index>>>(result);
            var returnValue = Assert.IsType<List<Model.Index>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetIndex_ReturnsIndex()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);
            var id = context.Indexes.First().Index_Id;

            // Act
            var result = await controller.GetIndex(id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Model.Index>>(result);
            var returnValue = Assert.IsType<Model.Index>(actionResult.Value);
            Assert.Equal(id, returnValue.Index_Id);
        }

        [Fact]
        public async Task GetIndex_ReturnsNotFound()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.GetIndex(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostIndex_CreatesIndex()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);
            var index = GenerateIndex(Guid.NewGuid(), Guid.NewGuid(), Subject.Luck);

            // Act
            var result = await controller.PostIndex(index);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Model.Index>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<Model.Index>(createdAtActionResult.Value);
            Assert.Equal(index.Index_Id, returnValue.Index_Id);
        }

        [Fact]
        public async Task PutIndex_UpdatesIndex()
        {
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);
            var id = context.Indexes.First().Index_Id;
            var index = GenerateIndex(id, Guid.NewGuid(), Subject.Luck);

            // Act
            var result = await controller.PutIndex(id, index);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedIndex = await context.Indexes.FindAsync(id);
            Assert.Equal(Subject.Luck, updatedIndex.Subject);
        }

        [Fact]
        public async Task PutIndex_ReturnsBadRequest()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);
            var id = Guid.NewGuid();
            var index = GenerateIndex(Guid.NewGuid(), Guid.NewGuid(), Subject.Luck);

            // Act
            var result = await controller.PutIndex(id, index);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteIndex_DeletesIndex()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);
            var id = context.Indexes.First().Index_Id;

            // Act
            var result = await controller.DeleteIndex(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var deletedIndex = await context.Indexes.FindAsync(id);
            Assert.Null(deletedIndex);
        }

        [Fact]
        public async Task DeleteIndex_ReturnsNotFound()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.DeleteIndex(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PatchIndex_UpdatesIndex()
        {
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);
            var id = context.Indexes.First().Index_Id;
            var patchDoc = new JsonPatchDocument<Model.Index>();
            patchDoc.Replace(c => c.Subject, Subject.Magic);

            // Act
            var result = await controller.PatchIndex(id, patchDoc);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedIndex = await context.Indexes.FindAsync(id);
            Assert.Equal(Subject.Magic, updatedIndex.Subject);
        }

        [Fact]
        public async Task QueryIndexes_ReturnsFilteredAndPagedResults()
        {
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);

            // Act
            var result = await controller.QueryIndexes(subject: Subject.Jotun, size: 1, offset: 0, sortBy: "Subject", sortOrder: "desc");

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<List<Model.Index>>(actionResult.Value);

            Assert.Single(returnValue);
            Assert.Equal(Subject.Jotun, returnValue.First().Subject);
        }

        [Fact]
        public async Task QueryIndexes_ReturnsSelectedFields()
        {
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);

            // Act
            var result = await controller.QueryIndexes(fields: "Subject");

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<ExpandoObject>>(actionResult.Value);

            foreach (var item in returnValue)
            {
                var dict = Assert.IsType<ExpandoObject>(item);
                Assert.Contains("Subject", dict);
                Assert.DoesNotContain("Entity_Id", dict);
            }
        }

        private static Model.Index GenerateIndex(Guid id, Guid Entity, Subject subject)
        {
            var index = new Model.Index
            {
                Index_Id = id,
                Entity_Id = Entity,
                Indexed_By = Guid.NewGuid(),
                Source_Id = Guid.NewGuid(),
                Location = "test",
                Subject = subject
            };

            return index;
        }
    }
}