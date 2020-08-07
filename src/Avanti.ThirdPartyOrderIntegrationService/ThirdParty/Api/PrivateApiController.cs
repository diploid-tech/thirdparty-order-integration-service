using System;
using Akka.Actor;
using AutoMapper;
using Avanti.Core.Microservice;
using Avanti.Core.Microservice.Actors;
using Avanti.ThirdPartyOrderIntegrationService.Order;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Avanti.ThirdPartyOrderIntegrationService.ThirdParty.Api
{
    [Route("/private/simulate")]
    [ApiController]
    public partial class PrivateApiController
    {
        private readonly IMapper mapper;
        private readonly IActorRef orderActorRef;
        private readonly ILogger logger;
        private readonly IClock clock;
        private readonly Random random = new Random();

        public PrivateApiController(
            IActorProvider<OrderActor> orderActorProvider,
            ILogger<PublicApiController> logger,
            IMapper mapper,
            IClock clock)
        {
            this.orderActorRef = orderActorProvider.Get();
            this.logger = logger;
            this.mapper = mapper;
            this.clock = clock;
        }
    }
}
