using EShop.Shared.Representations.Product;
using Microsoft.AspNetCore.Components.Forms;

namespace EShop.Web.Models;

public sealed class CreateProductViewModel : ProductUpsertDto
{
    public IBrowserFile? File { get; set; }
}