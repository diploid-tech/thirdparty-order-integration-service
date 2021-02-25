using Akka.Actor;
using Avanti.Core.Microservice.Actors;
using Avanti.Core.Microservice.AkkaSupport;

namespace Avanti.ThirdPartyOrderIntegrationService.Order
{
    public class OrderActorProvider : BaseActorProvider<OrderActor>
    {
        public OrderActorProvider(ActorSystem actorSystem)
        {
            this.ActorRef = actorSystem.ActorOfWithDI<OrderActor>("order-actor");
        }
    }
}
