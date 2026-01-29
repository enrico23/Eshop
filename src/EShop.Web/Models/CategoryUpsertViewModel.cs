using EShop.Shared.Representations.Category;
using Microsoft.AspNetCore.Components.Forms;

namespace EShop.Web.Models;

public class CategoryUpsertViewModel : CategoryUpsertDto
{
    public IBrowserFile? File { get; set; }
}
