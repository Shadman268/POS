using AutoMapper;
using Backend.DTOs;
using Backend.Models;

namespace Backend.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Product, ProductDto>().ReverseMap();

            // Receipt mapping
            CreateMap<Receipt, ReceiptDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            // ReceiptItem mapping
            CreateMap<ReceiptItem, ReceiptItemDto>().ReverseMap();
        }
    }
}
