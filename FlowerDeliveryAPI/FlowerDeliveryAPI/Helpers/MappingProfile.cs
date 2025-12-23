using AutoMapper;
using FlowerDeliveryAPI.Models;
using FlowerDeliveryAPI.DTOs;

namespace FlowerDeliveryAPI.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            
            CreateMap<Customer, CustomerDto>();
            CreateMap<CustomerCreateDto, Customer>();

            CreateMap<Flower, FlowerDto>();
            CreateMap<FlowerCreateDto, Flower>();

           
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : "Неизвестный клиент"));

            
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.FlowerName,
                    opt => opt.MapFrom(src => src.Flower != null ? src.Flower.Name : "Неизвестный цветок"));
        }
    }
}