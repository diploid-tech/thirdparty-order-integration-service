using AutoMapper;
using Avanti.ThirdPartyOrderIntegrationService.ThirdParty.Api;

namespace Avanti.ThirdPartyOrderIntegrationService.Order.Mappings;

public class OrderMapping : Profile
{
    public OrderMapping()
    {
        CreateMap<PublicApiController.PostOrderRequest, OrderActor.InsertExternalOrder>();
        CreateMap<PublicApiController.PostOrderRequest.Product, OrderActor.InsertExternalOrder.Product>();
        CreateMap<OrderActor.InsertExternalOrder, ExternalOrderProcessor.ExternalOrderQueueItem>();
        CreateMap<OrderActor.InsertExternalOrder.Product, ExternalOrderProcessor.ExternalOrderQueueItem.Product>();
        CreateMap<(ExternalOrderProcessor.ExternalOrderQueueItem Order, string System), Models.InternalOrder>()
            .ForMember(s => s.ExternalId, o => o.MapFrom(s => s.Order.Id))
            .ForMember(s => s.System, o => o.MapFrom(s => s.System))
            .ForMember(s => s.OrderDate, o => o.MapFrom(s => s.Order.OrderDate))
            .ForMember(s => s.Lines, o => o.MapFrom(s => s.Order.Products));
        CreateMap<ExternalOrderProcessor.ExternalOrderQueueItem.Product, Models.InternalOrder.OrderLine>();
    }
}
