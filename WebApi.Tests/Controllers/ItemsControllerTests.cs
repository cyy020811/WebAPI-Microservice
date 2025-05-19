using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Controllers;
using WebApi.Dtos;
using WebApi.Entities;
using WebApi.Repositories;

namespace WebApi.Tests.Controllers
{
    public class ItemControllerTests
    {
        private readonly Mock<IRepository<Item>> _itemRepository;
        private readonly ItemController _itemController;

        public ItemControllerTests()
        {
            _itemRepository = new Mock<IRepository<Item>>();
            _itemController = new ItemController(_itemRepository.Object);
        }

        [Fact]
        public async Task GetAsync_WhenRepositoryHasItems_ReturnsListOfItemDtos()
        {
            // Arrange
            var items = new List<Item>
            {
                new() { Id = Guid.NewGuid().ToString(), Name = "Item1", Description = "Dummy description", Price = 10, CreatedDate = DateTimeOffset.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Name = "Item2", Description = "Dummy description", Price = 20, CreatedDate = DateTimeOffset.UtcNow }
            };

            _itemRepository.Setup(repo => repo.GetAllAsync())
                    .ReturnsAsync(items);

            // Act
            var result = await _itemController.GetAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.Equal(items[0].Id, resultList[0].Id);
            Assert.Equal(items[0].Name, resultList[0].Name);
            Assert.Equal(items[1].Id, resultList[1].Id);
            Assert.Equal(items[1].Name, resultList[1].Name);
        }

        [Fact]
        public async Task GetByIdAsync_WhenItemExists_ReturnsItemDto()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var item = new Item { Id = id, Name = "Item", Description = "Dummy description", Price = 1, CreatedDate = DateTimeOffset.UtcNow };

            _itemRepository.Setup(repo => repo.GetAsync(id))
                    .ReturnsAsync(item);

            // Act
            var result = await _itemController.GetByIdAsync(id);

            // Assert
            var okResult = Assert.IsType<ActionResult<ItemDto>>(result);
            var dto = Assert.IsType<ItemDto>(okResult.Value);
            Assert.Equal(id, dto.Id);
            Assert.Equal("Item", dto.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WhenItemDoesntExist_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var item = new Item { Id = id, Name = "Item", Description = "Dummy description", Price = 1, CreatedDate = DateTimeOffset.UtcNow };

            _itemRepository.Setup(repo => repo.GetAsync(id))
                    .ReturnsAsync(() => null);

            // Act
            var result = await _itemController.GetByIdAsync(id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ItemDto>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task PostAsync_ValidItem_ReturnsCreatedAtAction()
        {
            // Arrange
            var createItemDto = new CreateItemDto("Test Item", "Test Description", 9.99m);

            Item? capturedItem = null;

            _itemRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<Item>()))
                .Callback<Item>(item => capturedItem = item)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _itemController.PostAsync(createItemDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnItem = Assert.IsType<Item>(createdAtActionResult.Value);

            Assert.Equal(nameof(ItemController.GetByIdAsync), createdAtActionResult.ActionName);
            Assert.Equal(createItemDto.Name, returnItem.Name);
            Assert.Equal(createItemDto.Description, returnItem.Description);
            Assert.Equal(createItemDto.Price, returnItem.Price);
            Assert.False(string.IsNullOrEmpty(returnItem.Id));
            Assert.True((DateTimeOffset.UtcNow - returnItem.CreatedDate).TotalSeconds < 5);

            _itemRepository.Verify(repo => repo.CreateAsync(It.IsAny<Item>()), Times.Once);
        }

        [Fact]
        public async Task PutAsync_WhenItemExists_UpdatesAndReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var existingItem = new Item
            {
                Id = id,
                Name = "Old Name",
                Description = "Old Description",
                Price = 10m
            };

            _itemRepository.Setup(repo => repo.GetAsync(id)).ReturnsAsync(existingItem);
            _itemRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Item>()))
                    .Returns(Task.CompletedTask);

            var updateItemDto = new UpdateItemDto("New Name", "New Description", 99m);

            // Act
            var result = await _itemController.PutAsync(id, updateItemDto);

            // Assert
            Assert.IsType<NoContentResult>(result);

            _itemRepository.Verify(repo => repo.UpdateAsync(It.Is<Item>(item =>
                item.Id == id &&
                item.Name == updateItemDto.Name &&
                item.Description == updateItemDto.Description &&
                item.Price == updateItemDto.Price
            )), Times.Once);
        }

        [Fact]
        public async Task PutAsync_WhenItemDoesntExist_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();

            _itemRepository.Setup(repo => repo.GetAsync(id)).ReturnsAsync(() => null);

            var updateItemDto = new UpdateItemDto("New Name", "New Description", 99m);

            // Act
            var result = await _itemController.PutAsync(id, updateItemDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenItemExists_RemovesAndReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var existingItem = new Item
            {
                Id = id,
                Name = "Name",
                Description = "Description",
                Price = 10m
            };

            _itemRepository.Setup(repo => repo.GetAsync(id)).ReturnsAsync(existingItem);
            _itemRepository.Setup(repo => repo.RemoveAsync(It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

            // Act
            var result = await _itemController.DeleteAsync(id);

            // Assert
            Assert.IsType<NoContentResult>(result);

            _itemRepository.Verify(repo => repo.RemoveAsync(It.Is<string>(removedId => removedId == id)), Times.Once);
        }
        
        [Fact]
        public async Task DeleteAsync_WhenItemDoesntExist_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();

            _itemRepository.Setup(repo => repo.GetAsync(id)).ReturnsAsync(() => null);

            // Act
            var result = await _itemController.DeleteAsync(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}