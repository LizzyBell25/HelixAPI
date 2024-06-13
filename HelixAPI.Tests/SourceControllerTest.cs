using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelixAPI.Controllers;
using HelixAPI.Models;
using HelixAPI.Models.ModelHelpers;
using HelixAPI.Contexts;
using Microsoft.AspNetCore.JsonPatch;
using System.Dynamic;
using System;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;

namespace HelixAPI.Tests
{
    public class SourcesControllerTests : IDisposable
    {
        private readonly DbContextOptions<HelixContext> _options;

        #region Helpers
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

        private void SeedDatabase(DbContextOptions<HelixContext> options)
        {
            using var context = new HelixContext(options);
            if (!context.Sources.Any())
            {
                var sources = new List<Source>
                {
                    GenerateSource(Guid.NewGuid(), "Publisher1"),
                    GenerateSource(Guid.NewGuid(), "Publisher2"),
                };

                context.Sources.AddRange(sources);
                context.SaveChanges();
            }
        }

        public void Dispose()
        {
            using var context = new HelixContext(_options);
            context.Database.EnsureDeleted();
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
                Format = Format.Ebook,
                RowVersion = BitConverter.GetBytes(DateTime.UtcNow.Ticks)
            };

            return source;
        }
        #endregion

        #region GET
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
            var emptyDbContextOptions = new DbContextOptionsBuilder<HelixContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new HelixContext(emptyDbContextOptions);
            var controller = new SourcesController(context);

            // Act
            var result = await controller.GetSources();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Source>>>(result);
            var returnValue = Assert.IsType<List<Source>>(actionResult.Value);
            Assert.Empty(returnValue);
        }
        #endregion

        #region Query
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
        #endregion

        #region POST
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
        #endregion

        #region PUT
        [Fact]
        public async Task PutSource_UpdatesSource()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<HelixContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Seed the database
            SeedDatabase(options);

            using var context = new HelixContext(options);
            var controller = new SourcesController(context);
            var sourceId = context.Sources.First().Source_Id;
            var sourceToUpdate = await context.Sources.FindAsync(sourceId);

            // Ensure the RowVersion is not causing conflicts
            var originalRowVersion = sourceToUpdate.RowVersion;

            sourceToUpdate.Publisher = "UpdatedPublisher";

            // Act
            var result = await controller.PutSource(sourceId, sourceToUpdate);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedSource = await context.Sources.FindAsync(sourceId);
            Assert.Equal("UpdatedPublisher", updatedSource.Publisher);
            Assert.NotEqual(originalRowVersion, updatedSource.RowVersion);
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
        #endregion

        #region PATCH
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
            Assert.Equal("UpdatedPublisher", updatedSource?.Publisher);
        }
        #endregion

        #region Delete
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
        #endregion

        [Fact]
        public async Task ConcurrentPutSource_ThrowsDbUpdateConcurrencyException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<HelixContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Seed the database
            SeedDatabase(options);

            // First context and controller
            using var context1 = new HelixContext(options);
            var controller1 = new SourcesController(context1);
            var sourceId = context1.Sources.First().Source_Id;
            var sourceUpdate1 = await context1.Sources.FindAsync(sourceId);

            // Second context and controller
            using var context2 = new HelixContext(options);
            var controller2 = new SourcesController(context2);
            var sourceUpdate2 = await context2.Sources.FindAsync(sourceId);

            // Update the entities
            sourceUpdate1.Publisher = "UpdatedPublisher1";
            sourceUpdate2.Publisher = "UpdatedPublisher2";

            // Act
            await controller1.PutSource(sourceId, sourceUpdate1);

            // Modify RowVersion to simulate concurrency
            sourceUpdate2.RowVersion = sourceUpdate1.RowVersion;

            // Assert
            var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => controller2.PutSource(sourceId, sourceUpdate2));
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task ConcurrentPatchSource_ThrowsDbUpdateConcurrencyException()
        {
            // Arrange
            var options = _options;
            //var options = new DbContextOptionsBuilder<HelixContext>()
            //    .UseInMemoryDatabase(databaseName: "TestDatabase")
            //    .Options;

            // Seed the database
            SeedDatabase(options);

            // First context and controller
            using var context1 = new HelixContext(options);
            var controller1 = new SourcesController(context1);
            var sourceId = context1.Sources.First().Source_Id;
            var sourceUpdate1 = await context1.Sources.FindAsync(sourceId);

            // Second context and controller
            using var context2 = new HelixContext(options);
            var controller2 = new SourcesController(context2);
            var sourceUpdate2 = await context2.Sources.FindAsync(sourceId);

            // Create a JsonPatchDocument
            var patchDoc1 = new JsonPatchDocument<Source>();
            patchDoc1.Replace(s => s.Publisher, "UpdatedPublisher1");

            var patchDoc2 = new JsonPatchDocument<Source>();
            patchDoc2.Replace(s => s.Publisher, "UpdatedPublisher2");

            // Act
            await controller1.PatchSource(sourceId, patchDoc1);

            // Modify RowVersion to simulate concurrency
            sourceUpdate2.RowVersion = sourceUpdate1.RowVersion;

            // Assert
            var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => controller2.PatchSource(sourceId, patchDoc2));
            Assert.NotNull(exception);
        }
    }
}