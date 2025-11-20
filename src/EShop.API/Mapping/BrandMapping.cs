using EShop.Domain;
using EShop.Shared.Representations.Brand;
using Mapster;

namespace EShop.API.Mapping
{
    public class BrandMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Brand, BrandDto>();
        }
    }
}
