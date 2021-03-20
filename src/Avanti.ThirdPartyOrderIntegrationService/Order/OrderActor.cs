using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using AutoMapper;
using Avanti.Core.Microservice;
using Avanti.Core.Microservice.Extensions;
using Avanti.Core.Processor;
using Avanti.Core.RelationalData;

namespace Avanti.ThirdPartyOrderIntegrationService.Order
{
    public partial class OrderActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly IRelationalDataStoreActorProvider relationalDataStoreActorProvider;
        private readonly IMapper mapper;
        private readonly IClock clock;
        private IActorRef? processorActor;

        public OrderActor(
            IRelationalDataStoreActorProvider datastoreActorProvider,
            IMapper mapper,
            IClock clock)
        {
            this.relationalDataStoreActorProvider = datastoreActorProvider;
            this.mapper = mapper;
            this.clock = clock;

            ReceiveAsync<InsertExternalOrder>(m => Handle(m).AsyncReplyTo(this.Sender));
        }

        private async Task<IResponse> Handle(InsertExternalOrder m)
        {
            this.log.Info($"Incoming request to queue third party order with external id '{m.Id}'");

            string newHash = CalculateHash(m);
            Result<string>? existingOrder = await GetExistingOrderByExternalIdentifier(m.Id);
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

                case IsFailure:
                    this.log.Error($"Could not process order with external id '{m.Id}'");
                    return new OrderFailedToReceive();
            }

            if (await this.processorActor.Ask(
                new SequentialProcessingActor.Queue<ExternalOrderProcessor.ExternalOrderQueueItem>(m.Id, this.mapper.Map<ExternalOrderProcessor.ExternalOrderQueueItem>(m))) is not SequentialProcessingActor.Queued)
            {
                return new OrderFailedToReceive();
            }

            Result? result = await this.relationalDataStoreActorProvider.ExecuteCommand(
                DataStoreStatements.InsertOrderHash,
                new
                {
                    ExternalId = m.Id,
                    Hash = newHash,
                    Now = this.clock.Now()
                });

            return result switch
            {
                IsSuccess => new OrderReceived(),
                _ => new OrderFailedToReceive(),
            };
        }

        private static string CalculateHash(object input)
        {
            string inputSerialized = JsonSerializer.Serialize(input);
            using var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(inputSerialized));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2", CultureInfo.InvariantCulture));
            }

            return hash.ToString();
        }

        private async Task<Result<string>> GetExistingOrderByExternalIdentifier(string externalId) =>
            await this.relationalDataStoreActorProvider.ExecuteScalar<string>(
                DataStoreStatements.GetOrderHashByExternalId,
                new
                {
                    ExternalId = externalId
                });

        protected override void PreStart() => this.processorActor = GetSequentualProcessor();
        protected virtual IActorRef GetSequentualProcessor() => Context.CreateSequentialProcessingActor<ExternalOrderProcessor.ExternalOrderQueueItem>("external-order-processing-actor");
    }
}
