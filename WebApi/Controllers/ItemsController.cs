using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemController : ControllerBase
    {
        private static readonly List<ItemDto> _items = new()
        {
            new ItemDto(Guid.NewGuid(), "Shoes", "Nike running shoes", 12, DateTimeOffset.UtcNow),
            new ItemDto(Guid.NewGuid(), "T-Shirt", "Adidas shirt", 10, DateTimeOffset.UtcNow),
            new ItemDto(Guid.NewGuid(), "Socks", "Reebok socks", 5, DateTimeOffset.UtcNow)
        };

        [HttpGet]
        public IEnumerable<ItemDto> Get()
        {
            return _items;
        }

        [HttpGet("{id}")]
        public ActionResult<ItemDto> GetByID(Guid id)
        {
            var item = _items.Where(item => item.Id == id).FirstOrDefault();

            if (item == null) return NotFound();

            return item;
        }

        [HttpPost]
        public ActionResult<ItemDto> Post(CreateItemDto createItemDto)
        {
            var item = new ItemDto(Guid.NewGuid(), createItemDto.Name, createItemDto.Description, createItemDto.Price, DateTimeOffset.UtcNow);
            _items.Add(item);

            return CreatedAtAction(nameof(GetByID), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public IActionResult Put(Guid id, UpdateItemDto updateItemDto)
        {
            var existingItem = _items.FirstOrDefault(item => item.Id == id);

            if (existingItem == null) return NotFound();

            var updatedItem = existingItem with
            {
                Name = updateItemDto.Name,
                Description = updateItemDto.Description,
                Price = updateItemDto.Price
            };

            var index = _items.FindIndex(item => item.Id == id);
            _items[index] = updatedItem;

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var index = _items.FindIndex(item => item.Id == id); 

            if (index < 0) return NotFound();

            _items.RemoveAt(index);

            return NoContent();
        }
    }
}

