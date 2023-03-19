using AutoMapper;
using Avanti.Core.Microservice.Actors;
using Avanti.ThirdPartyOrderIntegrationService.Order;
using Microsoft.AspNetCore.Mvc;

namespace Avanti.ThirdPartyOrderIntegrationService.ThirdParty.Api;

[Route("/public/order")]
[ApiController]
public partial class PublicApiController
{
    private readonly IMapper mapper;
    private readonly IActorRef orderActorRef;
    private readonly ILogger logger;

    public PublicApiController(
        IActorProvider<OrderActor> orderActorProvider,
        ILogger<PublicApiController> logger,
        IMapper mapper)
    {
        this.orderActorRef = orderActorProvider.Get();
        this.logger = logger;
        this.mapper = mapper;
    }
}
