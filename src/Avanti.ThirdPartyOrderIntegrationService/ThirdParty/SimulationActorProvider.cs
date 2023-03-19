using Avanti.Core.Microservice.Actors;
using Avanti.Core.Microservice.Extensions;

namespace Avanti.ThirdPartyOrderIntegrationService.ThirdParty;

public class SimulationActorProvider : BaseActorProvider<SimulationActor>
{
    public SimulationActorProvider(ActorSystem actorSystem)
    {
        this.ActorRef = actorSystem.ActorOfWithDI<SimulationActor>("simulation-actor");
    }
}
