using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;
using HelixAPI;
using HelixAPI.Model;
using HelixAPI.Data; 

namespace HelixAPI.Tests
{
    public class EntityRelationshipControllerTests : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly WebApplicationFactory<Startup> _factory;

        public EntityRelationshipControllerTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the app's MyDbContext registration.
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType ==
                             typeof(DbContextOptions<HelixContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add MyDbContext using an in-memory database for testing.
                    services.AddDbContext<HelixContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForTesting");
                    });

                    // Build the service provider.
                    var sp = services.BuildServiceProvider();

                    // Create a scope to obtain a reference to the database
                    // context (MyDbContext).
                    using (var scope = sp.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<HelixContext>();

                        // Ensure the database is created.
                        db.Database.EnsureCreated();
                    }
                });
            });

            _client = _factory.CreateClient();
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public void Dispose()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HelixContext>();
                context.EntityRelationships.RemoveRange(context.EntityRelationships);
                context.Entities.RemoveRange(context.Entities);
                context.SaveChanges();
            }
        }

        [Fact]
        public async Task PutEntityRelationship_ShouldUpdateEntityRelationship()
        {
            // Arrange
            var entity1 = new Entity { Entity_Id = Guid.NewGuid(), Name = "Thor", Type = Catagory.God };
            var entity2 = new Entity { Entity_Id = Guid.NewGuid(), Name = "Loki", Type = Catagory.God };
            var newRelationship = new EntityRelationship
            {
                RelationshipId = Guid.NewGuid(),
                Entity1Id = entity1.Entity_Id,
                Entity2Id = entity2.Entity_Id,
                RelationshipType = RelationshipType.Sibling
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HelixContext>();
                context.Entities.AddRange(entity1, entity2);
                context.EntityRelationships.Add(newRelationship);
                context.SaveChanges();
            }

            var updatedRelationship = new EntityRelationship
            {
                RelationshipId = newRelationship.RelationshipId,
                Entity1Id = newRelationship.Entity1Id,
                Entity2Id = newRelationship.Entity2Id,
                RelationshipType = RelationshipType.Enemy
            };

            var updatedContent = JsonSerializer.Serialize(updatedRelationship, _jsonSerializerOptions);
            var content = new StringContent(updatedContent, Encoding.UTF8, "application/json");

            // Log the JSON content
            Console.WriteLine($"Updated JSON content: {updatedContent}");

            // Act
            var response = await _client.PutAsync($"/api/entityrelationships/{newRelationship.RelationshipId}", content);

            // Log the response content for debugging
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response content: {responseString}");

            // Assert
            response.EnsureSuccessStatusCode();
            var returnedRelationship = JsonSerializer.Deserialize<EntityRelationship>(responseString, _jsonSerializerOptions);
            Assert.Equal(updatedRelationship.RelationshipType, returnedRelationship.RelationshipType);
        }

        [Fact]
        public async Task PostEntityRelationship_ShouldReturnCreatedEntityRelationship()
        {
            // Arrange
            var entity1 = new Entity { Entity_Id = Guid.NewGuid(), Name = "Thor", Type = Catagory.God };
            var entity2 = new Entity { Entity_Id = Guid.NewGuid(), Name = "Odin", Type = Catagory.God };

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HelixContext>();
                context.Entities.AddRange(entity1, entity2);
                context.SaveChanges();
            }

            var newRelationship = new EntityRelationship
            {
                RelationshipId = Guid.NewGuid(),
                Entity1Id = entity1.Entity_Id,
                Entity2Id = entity2.Entity_Id,
                RelationshipType = RelationshipType.Child
            };
            var content = new StringContent(JsonSerializer.Serialize(newRelationship, _jsonSerializerOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/entityrelationships", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var returnedRelationship = JsonSerializer.Deserialize<EntityRelationship>(responseString, _jsonSerializerOptions);
            Assert.Equal(newRelationship.Entity1Id, returnedRelationship.Entity1Id);
            Assert.Equal(newRelationship.Entity2Id, returnedRelationship.Entity2Id);
            Assert.Equal(newRelationship.RelationshipType, returnedRelationship.RelationshipType);
        }

        [Fact]
        public async Task GetEntityRelationship_ShouldReturnEntityRelationship()
        {
            // Arrange
            var entity1 = new Entity { Entity_Id = Guid.NewGuid(), Name = "Thor", Type = Catagory.God };
            var entity2 = new Entity { Entity_Id = Guid.NewGuid(), Name = "Odin", Type = Catagory.God };
            var newRelationship = new EntityRelationship
            {
                RelationshipId = Guid.NewGuid(),
                Entity1Id = entity1.Entity_Id,
                Entity2Id = entity2.Entity_Id,
                RelationshipType = RelationshipType.Child
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HelixContext>();
                context.Entities.AddRange(entity1, entity2);
                context.EntityRelationships.Add(newRelationship);
                context.SaveChanges();
            }

            // Act
            var response = await _client.GetAsync($"/api/entityrelationships/{newRelationship.RelationshipId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var returnedRelationship = JsonSerializer.Deserialize<EntityRelationship>(responseString, _jsonSerializerOptions);
            Assert.Equal(newRelationship.Entity1Id, returnedRelationship.Entity1Id);
            Assert.Equal(newRelationship.Entity2Id, returnedRelationship.Entity2Id);
            Assert.Equal(newRelationship.RelationshipType, returnedRelationship.RelationshipType);
        }

        [Fact]
        public async Task GetEntityRelationships_ShouldReturnAllEntityRelationships()
        {
            // Arrange
            var entity1 = new Entity { Entity_Id = Guid.NewGuid(), Name = "Thor", Type = Catagory.God };
            var entity2 = new Entity { Entity_Id = Guid.NewGuid(), Name = "Odin", Type = Catagory.God };
            var entity3 = new Entity { Entity_Id = Guid.NewGuid(), Name = "Loki", Type = Catagory.God };
            var relationship1 = new EntityRelationship
            {
                RelationshipId = Guid.NewGuid(),
                Entity1Id = entity1.Entity_Id,
                Entity2Id = entity2.Entity_Id,
                RelationshipType = RelationshipType.Child
            };
            var relationship2 = new EntityRelationship
            {
                RelationshipId = Guid.NewGuid(),
                Entity1Id = entity2.Entity_Id,
                Entity2Id = entity3.Entity_Id,
                RelationshipType = RelationshipType.Sibling
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HelixContext>();
                context.Entities.AddRange(entity1, entity2);
                context.EntityRelationships.AddRange(relationship1, relationship2);
                context.SaveChanges();
            }

            // Act
            var response = await _client.GetAsync("/api/entityrelationships");

            // Log the response content for debugging
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response content: {responseString}");

            // Assert
            response.EnsureSuccessStatusCode();
            var returnedRelationships = JsonSerializer.Deserialize<List<EntityRelationship>>(responseString, _jsonSerializerOptions);
            Assert.NotNull(returnedRelationships);
            Assert.Equal(2, returnedRelationships.Count);

            Assert.Contains(returnedRelationships, r => r.RelationshipId == relationship1.RelationshipId);
            Assert.Contains(returnedRelationships, r => r.RelationshipId == relationship2.RelationshipId);
        }

        [Fact]
        public async Task DeleteEntityRelationship_ShouldRemoveEntityRelationship()
        {
            // Arrange
            var entity1 = new Entity { Entity_Id = Guid.NewGuid(), Name = "Thor", Type = Catagory.God };
            var entity2 = new Entity { Entity_Id = Guid.NewGuid(), Name = "Odin", Type = Catagory.God };
            var newRelationship = new EntityRelationship
            {
                RelationshipId = Guid.NewGuid(),
                Entity1Id = entity1.Entity_Id,
                Entity2Id = entity2.Entity_Id,
                RelationshipType = RelationshipType.Child
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HelixContext>();
                context.Entities.AddRange(entity1, entity2);
                context.EntityRelationships.Add(newRelationship);
                context.SaveChanges();
            }

            // Act
            var response = await _client.DeleteAsync($"/api/entityrelationships/{newRelationship.RelationshipId}");

            // Assert
            response.EnsureSuccessStatusCode();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HelixContext>();
                var deletedRelationship = await context.EntityRelationships.FindAsync(newRelationship.RelationshipId);
                Assert.Null(deletedRelationship);
            }
        }
    }
}
