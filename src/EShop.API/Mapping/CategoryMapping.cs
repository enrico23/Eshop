using EShop.Domain.Catalog;
using EShop.Shared.Representations.Category;
using Mapster;

namespace EShop.API.Mapping
{
    public class CategoryMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Category, CategoryDto>();
        }
    }
}
