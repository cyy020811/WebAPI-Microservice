using System.ComponentModel.DataAnnotations;

namespace WebApi.Dtos
{
    public record ItemDto(string Id, string Name, string Description, [Range(0, 1000)] decimal Price, DateTimeOffset CreatedDate);

    public record CreateItemDto([Required] string Name, string Description, [Range(0, 1000)] decimal Price);

    public record UpdateItemDto([Required] string Name, string Description, [Range(0, 1000)] decimal Price);

}