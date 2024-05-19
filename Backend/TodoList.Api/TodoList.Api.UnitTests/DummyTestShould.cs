using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Api.Controllers;
using TodoList.Api.Models;
using Xunit;

namespace TodoList.Api.UnitTests
{
    public class TodoItemsControllerTests
    {
        private readonly TodoItemsController _controller;
        private readonly TodoContext _context;
        private readonly Mock<ILogger<TodoItemsController>> _mockLogger;

        public TodoItemsControllerTests()
        {
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(databaseName: "TodoListTest")
                .Options;
            _context = new TodoContext(options);

            _mockLogger = new Mock<ILogger<TodoItemsController>>();
            _controller = new TodoItemsController(_context, _mockLogger.Object);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            _context.TodoItems.RemoveRange(_context.TodoItems);
            _context.TodoItems.AddRange(
                new TodoItem { Id = Guid.NewGuid(), Description = "Test Task 1", IsCompleted = false },
                new TodoItem { Id = Guid.NewGuid(), Description = "Test Task 2", IsCompleted = true },
                new TodoItem { Id = Guid.NewGuid(), Description = "Test Task 3", IsCompleted = false }
            );
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetTodoItems_ShouldReturnIncompleteItems()
        {
            // Act
            var result = await _controller.GetTodoItems();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var items = Assert.IsType<List<TodoItem>>(okResult.Value);
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task GetTodoItem_ShouldReturnItem_WhenItemExists()
        {
            // Arrange
            var itemId = _context.TodoItems.First().Id;

            // Act
            var result = await _controller.GetTodoItem(itemId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var item = Assert.IsType<TodoItem>(okResult.Value);
            Assert.Equal(itemId, item.Id);
        }

        [Fact]
        public async Task GetTodoItem_ShouldReturnNotFound_WhenItemDoesNotExist()
        {
            // Act
            var result = await _controller.GetTodoItem(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PutTodoItem_ShouldReturnNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange
            var existingItem = _context.TodoItems.First();
            existingItem.Description = "Updated Description";

            // Act
            var result = await _controller.PutTodoItem(existingItem.Id, existingItem);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updatedItem = await _context.TodoItems.FindAsync(existingItem.Id);
            Assert.Equal("Updated Description", updatedItem.Description);
        }

        [Fact]
        public async Task PutTodoItem_ShouldReturnBadRequest_WhenIdMismatch()
        {
            // Arrange
            var existingItem = _context.TodoItems.First();
            var differentId = Guid.NewGuid();

            // Act
            var result = await _controller.PutTodoItem(differentId, existingItem);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Mismatch of ID", badRequestResult.Value);
        }

        [Fact]
        public async Task PutTodoItem_ShouldReturnNotFound_WhenItemDoesNotExist()
        {
            // Arrange
            var nonExistentItem = new TodoItem { Id = Guid.NewGuid(), Description = "Non-existent Item" };

            // Act
            var result = await _controller.PutTodoItem(nonExistentItem.Id, nonExistentItem);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Item not found", notFoundResult.Value);
        }

        [Fact]
        public async Task PostTodoItem_ShouldCreateNewItem_WhenValid()
        {
            // Arrange
            var newItem = new TodoItem { Id = Guid.NewGuid(), Description = "New Task", IsCompleted = false };

            // Act
            var result = await _controller.PostTodoItem(newItem);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var createdItem = Assert.IsType<TodoItem>(createdAtActionResult.Value);
            Assert.Equal(newItem.Description, createdItem.Description);
            Assert.Equal(newItem.Id, createdItem.Id);
            Assert.False(createdItem.IsCompleted);
        }

        [Fact]
        public async Task PostTodoItem_ShouldReturnBadRequest_WhenItemIsNull()
        {
            // Act
            var result = await _controller.PostTodoItem(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Item is empty", badRequestResult.Value);
        }

        [Fact]
        public async Task PostTodoItem_ShouldReturnBadRequest_WhenIdAlreadyExists()
        {
            // Arrange
            var existingItem = _context.TodoItems.First();
            var newItem = new TodoItem { Id = existingItem.Id, Description = "Duplicate GUID Task" };

            // Act
            var result = await _controller.PostTodoItem(newItem);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("GUID already exists in DB", badRequestResult.Value);
        }

        [Fact]
        public async Task PostTodoItem_ShouldReturnBadRequest_WhenDescriptionIsNullOrEmpty()
        {
            // Arrange
            var newItem = new TodoItem { Id = Guid.NewGuid(), Description = "" };

            // Act
            var result = await _controller.PostTodoItem(newItem);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Description is required", badRequestResult.Value);
        }

        [Fact]
        public async Task PostTodoItem_ShouldReturnBadRequest_WhenDescriptionAlreadyExists()
        {
            // Arrange
            var existingItem = _context.TodoItems.First(x => !x.IsCompleted);
            var newItem = new TodoItem { Id = Guid.NewGuid(), Description = existingItem.Description };

            // Act
            var result = await _controller.PostTodoItem(newItem);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Description already exists", badRequestResult.Value);
        }
    }
}
