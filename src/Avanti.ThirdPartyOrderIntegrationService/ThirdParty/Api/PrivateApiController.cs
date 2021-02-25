using System;
using Akka.Actor;
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
        private readonly IActorRef orderActorRef;
        private readonly ILogger logger;
        private readonly IClock clock;
        private readonly Random random = new();

        public PrivateApiController(
            IActorProvider<OrderActor> orderActorProvider,
            ILogger<PublicApiController> logger,
            IClock clock)
        {
            this.orderActorRef = orderActorProvider.Get();
            this.logger = logger;
            this.clock = clock;
        }
    }
}
