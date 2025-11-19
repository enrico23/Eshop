using EShop.API.Representations.Brand;
using EShop.Domain;
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
