using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using AutoMapper;
using Avanti.Core.EventStream;
using Avanti.Core.Microservice;
using Avanti.Core.Processor;
using Avanti.Core.RelationalData;

namespace Avanti.ThirdPartyOrderIntegrationService.Order
{
    public partial class OrderActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly IRelationalDataStoreActorProvider relationalDataStoreActorProvider;
        private readonly IPlatformEventActorProvider platformEventActorProvider;
        private readonly IMapper mapper;
        private readonly IClock clock;
        private readonly IActorRef processorActor;

        public OrderActor(
            IRelationalDataStoreActorProvider datastoreActorProvider,
            IPlatformEventActorProvider platformEventActorProvider,
            IMapper mapper,
            IClock clock)
        {
            this.relationalDataStoreActorProvider = datastoreActorProvider;
            this.platformEventActorProvider = platformEventActorProvider;
            this.mapper = mapper;
            this.clock = clock;
            this.processorActor = GetSequentualProcessor();

            ReceiveAsync<InsertExternalOrder>(m => Handle(m).PipeTo(Sender));
        }

        private async Task<IResponse> Handle(InsertExternalOrder m)
        {
            this.log.Info($"Incoming request to queue third party order with external id '{m.Id}'");

            var newHash = CalculateHash(m);
            var existingOrder = await GetExistingOrderByExternalIdentifier(m.Id);
            switch (existingOrder)
            {
                case IsSome<string> hash:
                    if (newHash != hash.Value)
                    {
                        this.log.Warning($"Retrieved order with external id '{m.Id}' is different then older version! Skipping ...");
                    }
                    else
                    {
                        this.log.Info($"Already processed order with external id '{m.Id}'. Skipping...");
                    }

                    return new OrderAlreadyProcessed();

                case IsFailure _:
                    this.log.Error($"Could not process order with external id '{m.Id}'");
                    return new OrderFailedToReceive();
            }

            if (!(await this.processorActor.Ask(new SequentialProcessingActor.Queue<ExternalOrderProcessor.ExternalOrderQueueItem>(m.Id, this.mapper.Map<ExternalOrderProcessor.ExternalOrderQueueItem>(m))) is SequentialProcessingActor.Queued))
            {
                return new OrderFailedToReceive();
            }

            var result = await this.relationalDataStoreActorProvider.ExecuteCommand(
                DataStoreStatements.InsertOrderHash,
                new
                {
                    ExternalId = m.Id,
                    Hash = newHash,
                    Now = this.clock.Now()
                });

            switch (result)
            {
                case IsSuccess _:
                    return new OrderReceived();

                default:
                    return new OrderFailedToReceive();
            }
        }

        private static string CalculateHash(object input)
        {
            var inputSerialized = JsonSerializer.Serialize(input);
            using (var crypt = new SHA256Managed())
            {
                var hash = new System.Text.StringBuilder();
                byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(inputSerialized));
                foreach (byte theByte in crypto)
                {
                    hash.Append(theByte.ToString("x2", CultureInfo.InvariantCulture));
                }

                return hash.ToString();
            }
        }

        private async Task<Result<string>> GetExistingOrderByExternalIdentifier(string externalId) =>
            await this.relationalDataStoreActorProvider.ExecuteScalar<string>(
                DataStoreStatements.GetOrderHashByExternalId,
                new
                {
                    ExternalId = externalId
                });

        protected virtual IActorRef GetSequentualProcessor() => Context.CreateSequentialProcessingActor<ExternalOrderProcessor.ExternalOrderQueueItem>("external-order-processing-actor");
    }
}
