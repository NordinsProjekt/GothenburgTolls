using System.Net;
using System.Net.Http.Json;
using Entities.Tolls;
using Entities.Types;
using Entities.Vehicels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using UseCases.Dtos;
using UseCases.Interfaces;

namespace Presentation.LogVehicleAPI.Tests;

public class PostTollEventEndpointTests : IClassFixture<WebApplicationFactory<global::LogVehicleAPI.Program>>
{
    private readonly WebApplicationFactory<global::LogVehicleAPI.Program> _factory;

    public PostTollEventEndpointTests(WebApplicationFactory<global::LogVehicleAPI.Program> factory)
    {
        _factory = factory;
    }

    private (HttpClient client, ITollEventService mockService) CreateClientWithMock()
    {
        var mockService = Substitute.For<ITollEventService>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(mockService);
            });
        }).CreateClient();

        return (client, mockService);
    }

    private static VehiclePassageDto CreateValidDto() =>
        new("ABC123", DateTimeOffset.UtcNow.AddMinutes(-5), "ZoneA", VehicleType.Car);

    [Fact]
    public async Task PostTollEvent_WithValidDto_ShouldReturnCreated()
    {
        var (client, mockService) = CreateClientWithMock();
        var dto = CreateValidDto();
        var vehicle = new Car("ABC123");
        var tollEvent = new TollEvent(dto.EventDateTime, dto.Zone, vehicle.Id);

        mockService.RegisterAsync(Arg.Any<VehiclePassageDto>(), Arg.Any<CancellationToken>())
            .Returns(tollEvent);

        HttpResponseMessage response = await client.PostAsJsonAsync("/TollEvent", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostTollEvent_WhenArgumentNullExceptionThrown_ShouldReturnBadRequest()
    {
        var (client, mockService) = CreateClientWithMock();
        var dto = CreateValidDto();

        mockService.RegisterAsync(Arg.Any<VehiclePassageDto>(), Arg.Any<CancellationToken>())
            .Throws(new ArgumentNullException("dto"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/TollEvent", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTollEvent_WhenArgumentExceptionThrown_ShouldReturnBadRequest()
    {
        var (client, mockService) = CreateClientWithMock();
        var dto = CreateValidDto();

        mockService.RegisterAsync(Arg.Any<VehiclePassageDto>(), Arg.Any<CancellationToken>())
            .Throws(new ArgumentException("Zone is required.", "zone"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/TollEvent", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTollEvent_WhenArgumentOutOfRangeExceptionThrown_ShouldReturnBadRequest()
    {
        var (client, mockService) = CreateClientWithMock();
        var dto = CreateValidDto();

        mockService.RegisterAsync(Arg.Any<VehiclePassageDto>(), Arg.Any<CancellationToken>())
            .Throws(new ArgumentOutOfRangeException("eventDateTime", dto.EventDateTime, "Event date time cannot be in the future."));

        HttpResponseMessage response = await client.PostAsJsonAsync("/TollEvent", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTollEvent_WhenDbUpdateExceptionThrown_ShouldReturnConflict()
    {
        var (client, mockService) = CreateClientWithMock();
        var dto = CreateValidDto();

        mockService.RegisterAsync(Arg.Any<VehiclePassageDto>(), Arg.Any<CancellationToken>())
            .Throws(new Microsoft.EntityFrameworkCore.DbUpdateException("Conflict"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/TollEvent", dto);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PostTollEvent_WithValidDto_ShouldCallRegisterAsyncOnService()
    {
        var (client, mockService) = CreateClientWithMock();
        var dto = CreateValidDto();
        var vehicle = new Car("ABC123");
        var tollEvent = new TollEvent(dto.EventDateTime, dto.Zone, vehicle.Id);

        mockService.RegisterAsync(Arg.Any<VehiclePassageDto>(), Arg.Any<CancellationToken>())
            .Returns(tollEvent);

        await client.PostAsJsonAsync("/TollEvent", dto);

        await mockService.Received(1).RegisterAsync(Arg.Any<VehiclePassageDto>(), Arg.Any<CancellationToken>());
    }
}
