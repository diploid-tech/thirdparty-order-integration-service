using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using Akka.Actor;
using Avanti.Core.EventStream;
using Avanti.Core.RelationalData;
using Avanti.ThirdPartyOrderIntegrationService.Order;
using Avanti.ThirdPartyOrderIntegrationService.Order.Events;
using FluentAssertions;
using Xunit;

namespace Avanti.ThirdPartyOrderIntegrationServiceTests.Order
{
    public partial class OrderActorSpec
    {
        public class When_Insert_External_Order_Is_Received : OrderActorSpec
        {
            private OrderActor.InsertExternalOrder input = new OrderActor.InsertExternalOrder
            {
                Id = "53419-01",
                OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture),
                Products = new[]
                {
                    new OrderActor.InsertExternalOrder.Product { ProductId = 5, Amount = 1 },
                    new OrderActor.InsertExternalOrder.Product { ProductId = 7, Amount = 5 }
                }
            };

            public When_Insert_External_Order_Is_Received()
            {
                this.progDatastoreActor.SetResponseForRequest<RelationalDataStoreActor.ExecuteScalar>(request =>
                    request.SqlCommand == DataStoreStatements.GetOrderHashByExternalId ?
                        new RelationalDataStoreActor.ScalarResult(null) :
                        new RelationalDataStoreActor.ScalarResult(501));

                this.progPlatformEventActor.SetResponseForRequest<PlatformEventActor.SendEvent>(request =>
                    new PlatformEventActor.EventSendSuccess());
            }

            [Fact]
            public void Should_Return_Stored_When_Order_Is_Stored_Successfully_And_Send_Event()
            {
                var document = new InternalOrder
                {
                    OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture),
                    Lines = new[]
                    {
                        new OrderDocument.OrderLine { ProductId = 5, Amount = 1 },
                        new OrderDocument.OrderLine { ProductId = 7, Amount = 5 }
                    },
                    ExternalIdentifiers = new Dictionary<string, string>
                    {
                        { "eCommerceSystem", "53419-01" }
                    }.ToImmutableDictionary()
                };

                Subject.Tell(input);

                Kit.ExpectMsg<OrderActor.OrderReceived>().Should().BeEquivalentTo(
                    new OrderActor.OrderReceived
                    {
                        Id = 501
                    });

                this.progDatastoreActor.GetRequests<RelationalDataStoreActor.ExecuteScalar>()
                    .Should().BeEquivalentTo(new[]
                    {
                        new RelationalDataStoreActor.ExecuteScalar(
                            DataStoreStatements.InsertOrder,
                            new
                            {
                                OrderJson = JsonSerializer.Serialize(document),
                                Now = DateTimeOffset.Parse("2018-04-01T07:00:00Z", CultureInfo.InvariantCulture)
                            }),
                        new RelationalDataStoreActor.ExecuteScalar(
                            DataStoreStatements.GetOrderByExternalId,
                            new
                            {
                                ExternalId = "53419-01",
                                System = "eCommerceSystem"
                            })
                    });

                this.progPlatformEventActor.GetRequest<PlatformEventActor.SendEvent>().Should().BeEquivalentTo(
                    new PlatformEventActor.SendEvent(
                        new OrderInserted
                        {
                            Id = 501,
                            OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture)
                        }));
            }

            [Fact]
            public void Should_Return_Failure_When_Failed_To_Store()
            {
                this.progDatastoreActor.SetResponseForRequest<RelationalDataStoreActor.ExecuteScalar>(request =>
                    new RelationalDataStoreActor.ExecuteFailed());

                Subject.Tell(input);

                Kit.ExpectMsg<OrderActor.OrderFailedToReceive>();
            }

            [Fact]
            public void Should_Return_Failure_When_Failed_To_Send_Event()
            {
                this.progPlatformEventActor.SetResponseForRequest<PlatformEventActor.SendEvent>(request =>
                    new PlatformEventActor.EventSendFailed());

                Subject.Tell(input);

                Kit.ExpectMsg<OrderActor.OrderFailedToReceive>();
            }
        }
    }
}
