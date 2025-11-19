using EShop.API.Representations.Category;
using EShop.Domain;
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
