using AutoMapper;

namespace Catalog.Profiles;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Domain.Entities.ProductEntity, Models.ProductDto>().ReverseMap();
    }
}
