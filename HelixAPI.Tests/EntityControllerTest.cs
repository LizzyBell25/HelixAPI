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
    public class EntityControllerTests : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly WebApplicationFactory<Startup> _factory;

        public EntityControllerTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the app's HelixContext registration.
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType ==
                             typeof(DbContextOptions<HelixContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add HelixContext using an in-memory database for testing.
                    services.AddDbContext<HelixContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForTesting");
                    });

                    // Build the service provider.
                    var sp = services.BuildServiceProvider();

                    // Create a scope to obtain a reference to the database
                    // context (HelixContext).
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
                context.Entities.RemoveRange(context.Entities);
                context.SaveChanges();
            }
        }

        [Fact]
        public async Task PostEntity_ShouldReturnCreatedEntity()
        {
            // Arrange
            var newEntity = new Entity
            {
                Entity_Id = Guid.NewGuid(),
                Name = "Thor",
                Description = "God of Thunder",
                Type = Catagory.God
            };
            var content = new StringContent(JsonSerializer.Serialize(newEntity, _jsonSerializerOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/entities", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var returnedEntity = JsonSerializer.Deserialize<Entity>(responseString, _jsonSerializerOptions);
            Assert.Equal(newEntity.Name, returnedEntity.Name);
            Assert.Equal(newEntity.Type, returnedEntity.Type);
        }

        [Fact]
        public async Task GetEntities_ShouldReturnAllEntities()
        {
            // Arrange
            var entities = new[]
            {
                new Entity
                {
                    Entity_Id = Guid.NewGuid(),
                    Name = "Odin",
                    Description = "Allfather",
                    Type = Catagory.God
                },
                new Entity
                {
                    Entity_Id = Guid.NewGuid(),
                    Name = "Thor",
                    Description = "God of Thunder",
                    Type = Catagory.God
                },
                new Entity
                {
                    Entity_Id = Guid.NewGuid(),
                    Name = "Freya",
                    Description = "Goddess of Love",
                    Type = Catagory.God
                }
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HelixContext>();
                context.Entities.AddRange(entities);
                context.SaveChanges();
            }

            // Act
            var response = await _client.GetAsync("/api/entities");

            // Log the response content for debugging
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response content: {responseString}");

            // Assert
            response.EnsureSuccessStatusCode();
            var returnedEntities = JsonSerializer.Deserialize<List<Entity>>(responseString, _jsonSerializerOptions);
            Assert.NotNull(returnedEntities);
            Assert.Equal(3, returnedEntities.Count);

            foreach (var entity in entities)
            {
                Assert.Contains(returnedEntities, e => e.Entity_Id == entity.Entity_Id && e.Name == entity.Name);
            }
        }
        
        [Fact]
        public async Task GetEntity_ShouldReturnEntity()
        {
            // Arrange
            var newEntity = new Entity
            {
                Entity_Id = Guid.NewGuid(),
                Name = "Odin",
                Description = "Allfather",
                Type = Catagory.God
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HelixContext>();
                context.Entities.Add(newEntity);
                context.SaveChanges();
            }

            // Act
            var response = await _client.GetAsync($"/api/entities/{newEntity.Entity_Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var returnedEntity = JsonSerializer.Deserialize<Entity>(responseString, _jsonSerializerOptions);
            Assert.Equal(newEntity.Name, returnedEntity.Name);
            Assert.Equal(newEntity.Type, returnedEntity.Type);
        }

        [Fact]
        public async Task PutEntity_ShouldUpdateEntity()
        {
            // Arrange
            var newEntity = new Entity
            {
                Entity_Id = Guid.NewGuid(),
                Name = "Loki",
                Description = "Trickster",
                Type = Catagory.God
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HelixContext>();
                context.Entities.Add(newEntity);
                context.SaveChanges();
            }

            var updatedEntity = new Entity
            {
                Entity_Id = newEntity.Entity_Id,
                Name = "Loki",
                Description = "God of Mischief",
                Type = Catagory.God
            };

            var content = new StringContent(JsonSerializer.Serialize(updatedEntity, _jsonSerializerOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/entities/{newEntity.Entity_Id}", content);

            // Log the response content for debugging
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response content: {responseString}");

            // Assert
            response.EnsureSuccessStatusCode();
            var returnedEntity = JsonSerializer.Deserialize<Entity>(responseString, _jsonSerializerOptions);
            Assert.Equal(updatedEntity.Description, returnedEntity.Description);
        }

        [Fact]
        public async Task DeleteEntity_ShouldRemoveEntity()
        {
            // Arrange
            var newEntity = new Entity
            {
                Entity_Id = Guid.NewGuid(),
                Name = "Freya",
                Description = "Goddess of Love",
                Type = Catagory.God
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HelixContext>();
                context.Entities.Add(newEntity);
                context.SaveChanges();
            }

            // Act
            var response = await _client.DeleteAsync($"/api/entities/{newEntity.Entity_Id}");

            // Assert
            response.EnsureSuccessStatusCode();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HelixContext>();
                var deletedEntity = await context.Entities.FindAsync(newEntity.Entity_Id);
                Assert.Null(deletedEntity);
            }
        }

    }
}
