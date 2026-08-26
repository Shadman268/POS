using AutoMapper;
using Backend.DTOs;
using Backend.Models;

namespace Backend.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Receipt, ReceiptDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<ReceiptItem, ReceiptItemDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src =>
                    src.TenantMedicineId ?? (src.MedicineId.HasValue
                        ? Backend.Constants.PosCatalogConstants.ToPosItemId(src.MedicineId.Value)
                        : 0)))
                .ReverseMap();
        }
    }
}
