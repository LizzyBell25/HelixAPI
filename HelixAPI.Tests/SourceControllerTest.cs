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
    public class SourcesControllerTests : IDisposable
    {
        private readonly DbContextOptions<HelixContext> _options;

        public SourcesControllerTests()
        {
            _options = new DbContextOptionsBuilder<HelixContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use a unique database name for each test
                .Options;

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            using var context = new HelixContext(_options);
            var sources = new List<Source>
            {
                GenerateSource(Guid.NewGuid(), "Publisher1"),
                GenerateSource(Guid.NewGuid(), "Publisher2")
            };

            context.Sources.AddRange(sources);
            context.SaveChanges();
        }

        public void Dispose()
        {
            using var context = new HelixContext(_options);
            context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task GetSources_ReturnsAllSources()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new SourcesController(context);

            // Act
            var result = await controller.GetSources();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Source>>>(result);
            var returnValue = Assert.IsType<List<Source>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetSource_ReturnsSource()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new SourcesController(context);
            var id = context.Sources.First().Source_Id;

            // Act
            var result = await controller.GetSource(id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Source>>(result);
            var returnValue = Assert.IsType<Source>(actionResult.Value);
            Assert.Equal(id, returnValue.Source_Id);
        }

        [Fact]
        public async Task GetSource_ReturnsNotFound()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new SourcesController(context);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.GetSource(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetSources_ReturnsEmptyList_WhenNoSourcesExist()
        {
            using var context = new HelixContext(_options);
            var controller = new SourcesController(context);

            // Act
            var result = await controller.GetSources();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Source>>>(result);
            var returnValue = Assert.IsType<List<Source>>(actionResult.Value);
            Assert.Empty(returnValue);
        }

        [Fact]
        public async Task PostSource_CreatesSource()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new SourcesController(context);
            var source = GenerateSource(Guid.NewGuid(), "Publisher3");

            // Act
            var result = await controller.PostSource(source);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Source>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<Source>(createdAtActionResult.Value);
            Assert.Equal(source.Source_Id, returnValue.Source_Id);
        }

        [Fact]
        public async Task PostSource_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new SourcesController(context);
            controller.ModelState.AddModelError("Publisher", "Required");

            var source = GenerateSource(Guid.NewGuid(), string.Empty);

            // Act
            var result = await controller.PostSource(source);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Source>>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task PutSource_UpdatesSource()
        {
            using var context = new HelixContext(_options);
            var controller = new SourcesController(context);
            var id = context.Sources.First().Source_Id;
            var source = GenerateSource(id, "UpdatedPublisher");

            // Act
            var result = await controller.PutSource(id, source);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedSource = await context.Sources.FindAsync(id);
            Assert.Equal("UpdatedPublisher", updatedSource.Publisher);
        }

        [Fact]
        public async Task PutSource_ReturnsBadRequest()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new SourcesController(context);
            var id = Guid.NewGuid();
            var source = GenerateSource(Guid.NewGuid(), "Publisher1");

            // Act
            var result = await controller.PutSource(id, source);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteSource_DeletesSource()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new SourcesController(context);
            var id = context.Sources.First().Source_Id;

            // Act
            var result = await controller.DeleteSource(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var deletedSource = await context.Sources.FindAsync(id);
            Assert.Null(deletedSource);
        }

        [Fact]
        public async Task DeleteSource_ReturnsNotFound()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new SourcesController(context);
            var id = Guid.NewGuid();

            // Act
            var result = await controller.DeleteSource(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PatchSource_UpdatesSource()
        {
            using var context = new HelixContext(_options);
            var controller = new SourcesController(context);
            var id = context.Sources.First().Source_Id;
            var patchDoc = new JsonPatchDocument<Source>();
            patchDoc.Replace(s => s.Publisher, "UpdatedPublisher");

            // Act
            var result = await controller.PatchSource(id, patchDoc);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedSource = await context.Sources.FindAsync(id);
            Assert.Equal("UpdatedPublisher", updatedSource.Publisher);
        }

        [Fact]
        public async Task QuerySources_ReturnsFilteredAndPagedResults()
        {
            using var context = new HelixContext(_options);
            var controller = new SourcesController(context);

            var queryDto = new QueryDto("Source_ID")
            {
                Filters =
                [
                    new() { Property = "Branch", Operation = "equals", Value = "Norse" }
                ],
                Size = 1,
                Offset = 0,
                SortBy = "publication_date",
                SortOrder = "desc",
                Fields = null
            };

            // Act
            var result = await controller.QuerySources(queryDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<List<Source>>(actionResult.Value);

            Assert.Single(returnValue);
            Assert.Equal(Branch.Norse, returnValue.First().Branch);
        }

        [Fact]
        public async Task QuerySources_ReturnsSelectedFields()
        {
            using var context = new HelixContext(_options);
            var controller = new SourcesController(context);

            var queryDto = new QueryDto("Source_ID")
            {
                Filters = [],
                Size = 100,
                Offset = 0,
                SortBy = "Publisher",
                SortOrder = "asc",
                Fields = "Publisher"
            };

            // Act
            var result = await controller.QuerySources(queryDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<ExpandoObject>>(actionResult.Value);

            foreach (var item in returnValue)
            {
                var dict = Assert.IsType<ExpandoObject>(item);
                Assert.Contains("Publisher", dict);
                Assert.DoesNotContain("Branch", dict);
            }
        }

        private static Source GenerateSource(Guid id, string publisher)
        {
            var source = new Source
            {
                Source_Id = id,
                Creator_Id = Guid.NewGuid(),
                Publication_Date = DateTime.UtcNow,
                Publisher = publisher,
                Url = "test",
                Branch = Branch.Norse,
                Content_Type = ContentType.Reconstruction,
                Format = Format.Ebook
            };

            return source;
        }

        [Fact]
        public async Task ConcurrentUpdates_HandledCorrectly()
        {
            using var context = new HelixContext(_options);
            var controller1 = new SourcesController(context);
            var controller2 = new SourcesController(context);

            var sourceId = context.Sources.First().Source_Id;

            var sourceUpdate1 = await context.Sources.FindAsync(sourceId);
            var sourceUpdate2 = await context.Sources.FindAsync(sourceId);

            sourceUpdate1.Publisher = "UpdatedPublisher1";
            sourceUpdate2.Publisher = "UpdatedPublisher2";

            var result1 = await controller1.PutSource(sourceId, sourceUpdate1);
            var result2 = await controller2.PutSource(sourceId, sourceUpdate2);

            // Assert
            Assert.IsType<NoContentResult>(result1);
            // Depending on how your system handles concurrency, you might expect different results here:
            Assert.IsType<NoContentResult>(result2); // if last write wins
            Assert.IsType<ConflictResult>(result2);  // if there's a concurrency check causing conflict
        }
    }
}