﻿using Xunit;
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
    public class IndexesControllerTests : IDisposable
    {
        private readonly DbContextOptions<HelixContext> _options;

        #region Helpers
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
            var indexs = new List<Models.Index>
            {
                GenerateIndex(Guid.NewGuid(), "Test1", Subject.Jotun),
                GenerateIndex(Guid.NewGuid(), "Test2", Subject.Jotun)
            };

            context.Indexes.AddRange(indexs);
            context.SaveChanges();
        }

        public void Dispose()
        {
            using var context = new HelixContext(_options);
            context.Database.EnsureDeleted();
        }

        private static Models.Index GenerateIndex(Guid id, string Location, Subject subject)
        {
            var index = new Models.Index
            {
                Index_Id = id,
                Entity_Id = Guid.NewGuid(),
                Indexed_By = Guid.NewGuid(),
                Source_Id = Guid.NewGuid(),
                Location = Location,
                Subject = subject,
                RowVersion = [0]
            };

            return index;
        }
        #endregion

        #region GET
        [Fact]
        public async Task GetIndexes_ReturnsAllIndexes()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);

            // Act
            var result = await controller.GetIndexes();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Models.Index>>>(result);
            var returnValue = Assert.IsType<List<Models.Index>>(actionResult.Value);
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
            var ActionResult = Assert.IsType<ActionResult<Models.Index>>(result);
            var ReturnValue = Assert.IsType<Models.Index>(ActionResult.Value);
            Assert.Equal(id, ReturnValue.Index_Id);
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
        public async Task GetIndexes_ReturnsEmptyList_WhenNoSourcesExist()
        {
            var emptyDbContextOptions = new DbContextOptionsBuilder<HelixContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new HelixContext(emptyDbContextOptions);
            var controller = new IndexesController(context);

            // Act
            var result = await controller.GetIndexes();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Models.Index>>>(result);
            var returnValue = Assert.IsType<List<Models.Index>>(actionResult.Value);
            Assert.Empty(returnValue);
        }
        #endregion

        #region Query
        [Fact]
        public async Task QueryIndexes_ReturnsFilteredAndPagedResults()
        {
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);

            var queryDto = new QueryDto("Index_ID")
            {
                Filters =
                [
                    new() { Property = "Subject", Operation = "equals", Value = "Jotun" }
                ],
                Size = 1,
                Offset = 0,
                SortBy = "Index_ID",
                SortOrder = "desc",
                Fields = null
            };

            // Act
            var result = await controller.QueryIndexes(queryDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<List<Models.Index>>(actionResult.Value);

            Assert.Single(returnValue);
            Assert.Equal(Subject.Jotun, returnValue.First().Subject);
        }

        [Fact]
        public async Task QueryIndexes_ReturnsSelectedFields()
        {
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);

            var queryDto = new QueryDto("Index_ID")
            {
                Filters = [],
                Size = 100,
                Offset = 0,
                SortBy = "Subject",
                SortOrder = "asc",
                Fields = "Subject"
            };

            // Act
            var result = await controller.QueryIndexes(queryDto);

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
        #endregion

        #region POST
        [Fact]
        public async Task PostIndex_CreatesIndex()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);
            var index = GenerateIndex(Guid.NewGuid(), "Test3", Subject.Luck);

            // Act
            var result = await controller.PostIndex(index);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Models.Index>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<Models.Index>(createdAtActionResult.Value);
            Assert.Equal(index.Index_Id, returnValue.Index_Id);
        }

        [Fact]
        public async Task PostSource_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);
            controller.ModelState.AddModelError("Publisher", "Required");

            var index = GenerateIndex(Guid.NewGuid(), string.Empty, Subject.Ancestors);

            // Act
            var result = await controller.PostIndex(index);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Models.Index>>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }
        #endregion

        #region PUT
        [Fact]
        public async Task PutIndex_UpdatesIndex()
        {
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);
            var id = context.Indexes.First().Index_Id;
            var index = GenerateIndex(id, "Test3", Subject.Luck);

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
            var index = GenerateIndex(Guid.NewGuid(), "Test3", Subject.Luck);

            // Act
            var result = await controller.PutIndex(id, index);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }
        #endregion

        #region PATCH
        [Fact]
        public async Task PatchIndex_UpdatesIndex()
        {
            using var context = new HelixContext(_options);
            var controller = new IndexesController(context);
            var id = context.Indexes.First().Index_Id;
            var patchDoc = new JsonPatchDocument<Models.Index>();
            patchDoc.Replace(c => c.Subject, Subject.Magic);

            // Act
            var result = await controller.PatchIndex(id, patchDoc);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedIndex = await context.Indexes.FindAsync(id);
            Assert.Equal(Subject.Magic, updatedIndex.Subject);
        }
        #endregion

        #region Delete
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
        #endregion
    }
}