using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Akka.Actor;
using AutoMapper;
using Avanti.Core.Http;
using Avanti.Core.Microservice;
using Avanti.Core.Microservice.Actors;
using Avanti.Core.Processor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Avanti.ThirdPartyOrderIntegrationService.Order
{
    public class ExternalOrderProcessor : IQueueProcessor<ExternalOrderProcessor.ExternalOrderQueueItem>
    {
        private readonly IActorRef httpRequestActor;
        private readonly ServiceSettings serviceSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public ExternalOrderProcessor(
            IActorProvider<HttpRequestActor> httpRequestActorProvider,
            IOptions<ServiceSettings> serviceSettings,
            IMapper mapper,
            ILogger<ExternalOrderProcessor> logger)
        {
            this.httpRequestActor = httpRequestActorProvider.Get();
            this.serviceSettings = serviceSettings.Value;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result> Process(ExternalOrderQueueItem item)
        {
            var orderServiceUri = this.serviceSettings.OrderServiceUri ?? new Uri("http://unknown/");
            var result = await this.httpRequestActor.Ask(
                new HttpRequestActor.Post
                {
                    ServiceUrl = orderServiceUri.Port == 443 ? $"https://{orderServiceUri.Host}" : $"http://{orderServiceUri.Host}:{orderServiceUri.Port}",
                    Path = "/private/order",
                    Data = this.mapper.Map<Models.InternalOrder>((item, this.serviceSettings.ExternalSystemName))
                });

            return result switch
            {
                HttpRequestActor.ReceivedSuccessServiceResponse _ => new Success(),
                HttpRequestActor.ReceivedNonSuccessServiceResponse r when r.StatusCode == HttpStatusCode.Conflict => new Success(),
                HttpRequestActor.ReceivedNonSuccessServiceResponse _ => $"Remote service doesn't accept order {item.Id}".Failure(),
                _ => $"Could not send order to order service".Failure()
            };
        }

        public class ExternalOrderQueueItem : QueueItem
        {
            public string Id { get; set; } = "unknown";

            public DateTimeOffset OrderDate { get; set; }

            public IEnumerable<Product> Products { get; set; } = Array.Empty<Product>();

            public class Product
            {
                public int ProductId { get; set; }
                public int Amount { get; set; }
            }
        }
    }
}