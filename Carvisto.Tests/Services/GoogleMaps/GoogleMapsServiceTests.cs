using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Carvisto.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System.Threading;
using System.Text.Json;
using Route = Carvisto.Services.Route;

namespace Carvisto.Tests.Services.GoogleMaps
{
    [TestFixture]
    public class GoogleMapsServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private Mock<IConfiguration> _mockConfiguration;
        private HttpClient _httpClient;
        private IGoogleMapsService _googleMapsService;
        private const string ApiKey = "test-api-key";

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["GoogleMaps:ApiKey"]).Returns(ApiKey);

            _googleMapsService = new GoogleMapsService(_httpClient, _mockConfiguration.Object);
        }

        [Test]
        [Category("GoogleMaps.GetRouteInfo")]
        public async Task GetRouteInfoAsync_WithValidResponse_ShouldReturnRouteInfo()
        {
            // Arrange
            var origin = "Bialystok";
            var destination = "Warsaw";
            var expectedDistance = "200 km";
            var expectedDuration = "2 hours 30 mins";

            var response = new DirectionsResponce
            {
                Routes = new List<Route>
                {
                    new Route
                    {
                        Legs = new List<Leg>
                        {
                            new Leg
                            {
                                Distance = new TextValue { Text = expectedDistance },
                                Duration = new TextValue { Text = expectedDuration }
                            }
                        }
                    }
                }
            };

            SetupMockHttpMessageHandler(
                JsonSerializer.Serialize(response),
                $"https://maps.googleapis.com/maps/api/directions/json?origin={origin}&destination={destination}&key={ApiKey}"
            );

            // Act
            var result = await _googleMapsService.GetRouteInfoAsync(origin, destination);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Distance, Is.EqualTo(expectedDistance));
            Assert.That(result.Duration, Is.EqualTo(expectedDuration));

            VerifyHttpClientCall(origin, destination);
        }

        [Test]
        [Category("GoogleMaps.GetRouteInfo")]
        public async Task GetRouteInfoAsync_WithEmptyResponse_ShouldReturnEmptyRouteInfo()
        {
            // Arrange
            var origin = "Bialystok";
            var destination = "Warsaw";
            var emptyResponse = new DirectionsResponce
            {
                Routes = new List<Route>()
            };

            SetupMockHttpMessageHandler(
                JsonSerializer.Serialize(emptyResponse),
                $"https://maps.googleapis.com/maps/api/directions/json?origin={origin}&destination={destination}&key={ApiKey}"
            );

            // Act
            var result = await _googleMapsService.GetRouteInfoAsync(origin, destination);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Distance, Is.Null);
            Assert.That(result.Duration, Is.Null);

            VerifyHttpClientCall(origin, destination);
        }

        [Test]
        [Category("GoogleMaps.GetRouteInfo")]
        public void GetRouteInfoAsync_WhenHttpRequestFails_ShouldThrowException()
        {
            // Arrange
            var origin = "Bialystok";
            var destination = "Warsaw";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _googleMapsService.GetRouteInfoAsync(origin, destination));
        }

        [Test]
        [Category("GoogleMaps.GetRouteInfo")]
        public void GetRouteInfoAsync_WhenApiReturnsError_ShouldThrowException()
        {
            // Arrange
            var origin = "Bialystok";
            var destination = "Warsaw";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Invalid request")
                });

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _googleMapsService.GetRouteInfoAsync(origin, destination));
        }

        private void SetupMockHttpMessageHandler(string content, string expectedUrl)
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(content)
                })
                .Verifiable();
        }

        private void VerifyHttpClientCall(string origin, string destination)
        {
            var encodedOrigin = WebUtility.UrlEncode(origin);
            var encodedDestination = WebUtility.UrlEncode(destination);

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString() ==
                    $"https://maps.googleapis.com/maps/api/directions/json?origin={encodedOrigin}&destination={encodedDestination}&key={ApiKey}"),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}