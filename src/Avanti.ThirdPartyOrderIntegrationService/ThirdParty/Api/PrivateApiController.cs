using Akka.Actor;
using Avanti.Core.Microservice.Actors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Avanti.ThirdPartyOrderIntegrationService.ThirdParty.Api
{
    [Route("/private/simulate")]
    [ApiController]
    public partial class PrivateApiController
    {
        private readonly IActorRef simulationActorRef;
        private readonly ILogger logger;

        public PrivateApiController(
            IActorProvider<SimulationActor> simulationActorProvider,
            ILogger<PublicApiController> logger)
        {
            this.simulationActorRef = simulationActorProvider.Get();
            this.logger = logger;
        }
    }
}
