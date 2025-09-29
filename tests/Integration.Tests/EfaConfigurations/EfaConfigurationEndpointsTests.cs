using System.Net;
using System.Net.Http.Json;
using Domain.EfaConfigs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Integration.Tests.Common;
using Integration.Tests.Helpers;
using Xunit;

namespace Integration.Tests.EfaConfigurations;

public class EfaConfigurationEndpointsTests : BaseIntegrationTest
{
    private const string BaseUrl = "efa-configurations";

    public EfaConfigurationEndpointsTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateEfaConfiguration_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        var request = new
        {
            Year = 2025,
            EfaRate = 0.0525m
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();

        EfaConfiguration? efaConfig = await DbContext.EfaConfigurations
            .FirstOrDefaultAsync(e => e.Id == result.Id);

        efaConfig.Should().NotBeNull();
        efaConfig!.Year.Should().Be(request.Year);
        efaConfig.EfaRate.Should().Be(request.EfaRate);
    }

    [Fact]
    public async Task GetAllEfaConfigurations_ShouldReturnOk_WithEmptyList_WhenNoConfigurationsExist()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<EfaConfigResponse>>();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    private record CreateResponse(Guid Id);
    private record EfaConfigResponse(Guid Id, int Year, decimal EfaRate);
}