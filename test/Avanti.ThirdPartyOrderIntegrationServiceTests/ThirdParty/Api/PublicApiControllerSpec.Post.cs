using System;
using System.Globalization;
using Avanti.ThirdPartyOrderIntegrationService.Order;
using Avanti.ThirdPartyOrderIntegrationService.ThirdParty.Api;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Avanti.ThirdPartyOrderIntegrationServiceTests.ThirdParty.Api;

public partial class PublicApiControllerSpec
{
    public class When_PostOrder_Request_Is_Received : PublicApiControllerSpec
    {
        private readonly PublicApiController.PostOrderRequest request = new()
        {
            Id = "53419-01",
            OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture),
            Products = new[]
            {
                new PublicApiController.PostOrderRequest.Product { ProductId = 5, Amount = 1 },
                new PublicApiController.PostOrderRequest.Product { ProductId = 7, Amount = 5 }
            }
        };

        [Fact]
        public async void Should_Return_200_When_Stored()
        {
            progOrderActor.SetResponseForRequest<OrderActor.InsertExternalOrder>(request =>
                new OrderActor.OrderReceived());

            IActionResult result = await Subject.PostOrder(request);

            result.Should().BeOfType<OkResult>();

            progOrderActor.GetRequest<OrderActor.InsertExternalOrder>()
                .Should().BeEquivalentTo(
                    new OrderActor.InsertExternalOrder
                    {
                        Id = "53419-01",
                        OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture),
                        Products = new[]
                        {
                            new OrderActor.InsertExternalOrder.Product { ProductId = 5, Amount = 1 },
                            new OrderActor.InsertExternalOrder.Product { ProductId = 7, Amount = 5 }
                        }
                    });
        }

        [Fact]
        public async void Should_Return_500_When_Failed_To_Store()
        {
            progOrderActor.SetResponseForRequest<OrderActor.InsertExternalOrder>(request =>
                new OrderActor.OrderFailedToReceive());

            IActionResult result = await Subject.PostOrder(request);

            result.Should().BeOfType<StatusCodeResult>()
                .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async void Should_Return_409_When_Already_Processed()
        {
            progOrderActor.SetResponseForRequest<OrderActor.InsertExternalOrder>(request =>
                new OrderActor.OrderAlreadyProcessed());

            IActionResult result = await Subject.PostOrder(request);

            result.Should().BeOfType<OkResult>();
        }
    }
}
