using AutoMapper;
using Avanti.Core.Microservice.Actors;
using Avanti.Core.Unittests;
using Avanti.ThirdPartyOrderIntegrationService.Order;
using Avanti.ThirdPartyOrderIntegrationService.Order.Mappings;
using Avanti.ThirdPartyOrderIntegrationService.ThirdParty.Api;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Avanti.ThirdPartyOrderIntegrationServiceTests.ThirdParty.Api
{
    public partial class PublicApiControllerSpec : WithSubject<PublicApiController>
    {
        private readonly ProgrammableActor<OrderActor> progOrderActor;

        private PublicApiControllerSpec()
        {
            progOrderActor = Kit.CreateProgrammableActor<OrderActor>("order-actor");
            IActorProvider<OrderActor> orderActorProvider = An<IActorProvider<OrderActor>>();
            orderActorProvider.Get().Returns(progOrderActor.TestProbe);

            var config = new MapperConfiguration(cfg => cfg.AddProfile(new OrderMapping()));
            config.AssertConfigurationIsValid();

            Subject = new PublicApiController(
                orderActorProvider,
                An<ILogger<PublicApiController>>(),
                config.CreateMapper());
        }
    }
}
