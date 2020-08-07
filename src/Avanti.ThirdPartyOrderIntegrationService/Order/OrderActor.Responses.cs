namespace Avanti.ThirdPartyOrderIntegrationService.Order
{
    public partial class OrderActor
    {
        public interface IResponse { }

        public class OrderReceived : IResponse { }

        public class OrderAlreadyProcessed : IResponse { }

        public class OrderFailedToReceive : IResponse { }
    }
}
