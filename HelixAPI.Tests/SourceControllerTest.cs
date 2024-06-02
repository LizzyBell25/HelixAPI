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

            // Act
            var result = await controller.QuerySources(branch: Branch.Norse, size: 1, offset: 0, sortBy: "publication_date", sortOrder: "desc");

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<List<Source>>(actionResult.Value);

            Assert.Single(returnValue);
            Assert.Equal("Publisher2", returnValue.First().Publisher);
        }

        [Fact]
        public async Task QuerySources_ReturnsSelectedFields()
        {
            using var context = new HelixContext(_options);
            var controller = new SourcesController(context);

            // Act
            var result = await controller.QuerySources(fields: "Publisher");

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
    }
}