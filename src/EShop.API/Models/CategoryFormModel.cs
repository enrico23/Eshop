using System.ComponentModel.DataAnnotations;

namespace EShop.API.Models;

public record CategoryFormModel(
    [Required] string Name, 
    IFormFile? File, 
    string? ImageAltText);

