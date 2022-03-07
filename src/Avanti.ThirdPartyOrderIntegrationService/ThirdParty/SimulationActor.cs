using System;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Avanti.Core.Microservice;
using Avanti.Core.Microservice.Actors;
using Avanti.ThirdPartyOrderIntegrationService.Order;

namespace Avanti.ThirdPartyOrderIntegrationService.ThirdParty
{
    public partial class SimulationActor : ReceiveActor
    {
        private readonly IActorRef orderActorRef;
        private readonly IClock clock;
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly Random random = new();
        private ICancelable? cancelableTimer;

        public SimulationActor(
            IActorProvider<OrderActor> orderActorProvider,
            IClock clock)
        {
            this.orderActorRef = orderActorProvider.Get();
            this.clock = clock;

            Receive<StartOnceSimulation>(m => HandleStartOnceSimulation(m));
            Receive<StartStressSimuliation>(m => HandleStartStressSimuliation(m));
            Receive<SendOrder>(_ => HandleSendOrder());
            Receive<ReceiveTimeout>(_ =>
            {
                this.log.Info("Stopping stress test");
                this.cancelableTimer.CancelIfNotNull();
            });
        }

        private void HandleStartOnceSimulation(StartOnceSimulation m)
        {
            for (int idx = 0; idx < m.Number; idx++)
            {
                HandleSendOrder();
            }

            this.log.Info($"Done with simulating {m.Number} orders");
        }

        private void HandleStartStressSimuliation(StartStressSimuliation m)
        {
            this.log.Info("Starting stress test");
            this.cancelableTimer = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
                TimeSpan.Zero,
                m.Interval,
                this.Self,
                new SendOrder(),
                this.Self);
            Context.SetReceiveTimeout(m.Duration);
        }

        private void HandleSendOrder()
        {
            var order = new OrderActor.InsertExternalOrder
            {
                Id = Guid.NewGuid().ToString(),
                OrderDate = this.clock.Now().AddDays(this.random.Next(0, 5) * -1),
                Products = Enumerable.Range(1, this.random.Next(1, 10)).Select(idx =>
                    new
                    {
                        ProductId = this.random.Next(1, 100),
                        Amount = this.random.Next(1, 10)
                    })
                        .GroupBy(l => l.ProductId)
                        .Select(l => new OrderActor.InsertExternalOrder.Product
                        {
                            ProductId = l.Key,
                            Amount = l.Sum(p => p.Amount)
                        })
            };

            this.orderActorRef.Tell(order);
        }

        protected override void PostStop() => this.cancelableTimer.CancelIfNotNull();
    }
}