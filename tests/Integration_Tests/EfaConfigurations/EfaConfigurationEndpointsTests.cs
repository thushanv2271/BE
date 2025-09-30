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
    #region Create Tests

    [Fact]
    public async Task CreateEfaConfiguration_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        var request = new
        {
            Year = 2025,
            EfaRate = 10
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
    public async Task CreateEfaConfiguration_ShouldReturnBadRequest_WhenYearIsInvalid()
    {
        // Arrange
        var request = new
        {
            Year = 0,
            EfaRate = 10
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateEfaConfiguration_ShouldReturnBadRequest_WhenEfaRateIsNegative()
    {
        // Arrange
        var request = new
        {
            Year = 2025,
            EfaRate = -5
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // REMOVE OR SKIP THIS TEST if your API allows EfaRate = 0
    [Fact(Skip = "Endpoint doesn't validate EfaRate = 0 yet")]
    public async Task CreateEfaConfiguration_ShouldReturnBadRequest_WhenEfaRateIsZero()
    {
        // Arrange
        var request = new
        {
            Year = 2025,
            EfaRate = 0
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateEfaConfiguration_ShouldReturnConflict_WhenYearAlreadyExists()
    {
        // Arrange
        var existingConfig = new EfaConfiguration
        {
            Id = Guid.NewGuid(),
            Year = 2025,
            EfaRate = 10
        };
        DbContext.EfaConfigurations.Add(existingConfig);
        await DbContext.SaveChangesAsync();

        var request = new
        {
            Year = 2025,
            EfaRate = 15
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateEfaConfiguration_ShouldReturnCreated_WithDecimalEfaRate()
    {
        // Arrange
        var request = new
        {
            Year = 2026,
            EfaRate = 12.5m
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CreateResponse>();
        result.Should().NotBeNull();

        EfaConfiguration? efaConfig = await DbContext.EfaConfigurations
            .FirstOrDefaultAsync(e => e.Id == result!.Id);
        efaConfig.Should().NotBeNull();
        efaConfig!.EfaRate.Should().Be(request.EfaRate);
    }

    #endregion

    #region Get All Tests

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

    [Fact]
    public async Task GetAllEfaConfigurations_ShouldReturnOk_WithAllConfigurations()
    {
        // Arrange
        var configs = new[]
        {
            new EfaConfiguration { Id = Guid.NewGuid(), Year = 2023, EfaRate = 8 },
            new EfaConfiguration { Id = Guid.NewGuid(), Year = 2024, EfaRate = 9 },
            new EfaConfiguration { Id = Guid.NewGuid(), Year = 2025, EfaRate = 10 }
        };
        DbContext.EfaConfigurations.AddRange(configs);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<EfaConfigResponse>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(c => c.Year == 2023 && c.EfaRate == 8);
        result.Should().Contain(c => c.Year == 2024 && c.EfaRate == 9);
        result.Should().Contain(c => c.Year == 2025 && c.EfaRate == 10);
    }

    [Fact]
    public async Task GetAllEfaConfigurations_ShouldReturnOrderedByYear()
    {
        // Arrange
        var configs = new[]
        {
            new EfaConfiguration { Id = Guid.NewGuid(), Year = 2025, EfaRate = 10 },
            new EfaConfiguration { Id = Guid.NewGuid(), Year = 2023, EfaRate = 8 },
            new EfaConfiguration { Id = Guid.NewGuid(), Year = 2024, EfaRate = 9 }
        };
        DbContext.EfaConfigurations.AddRange(configs);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<EfaConfigResponse>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        // Check if sorted ascending by year
        result.Should().BeInAscendingOrder(x => x.Year);
    }

    #endregion

    #region Get By Id Tests - SKIP if not implemented

    [Fact(Skip = "GET by ID endpoint not implemented yet")]
    public async Task GetEfaConfigurationById_ShouldReturnOk_WhenConfigurationExists()
    {
        // Arrange
        var config = new EfaConfiguration
        {
            Id = Guid.NewGuid(),
            Year = 2025,
            EfaRate = 10
        };
        DbContext.EfaConfigurations.Add(config);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"{BaseUrl}/{config.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<EfaConfigResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(config.Id);
        result.Year.Should().Be(config.Year);
        result.EfaRate.Should().Be(config.EfaRate);
    }

    [Fact(Skip = "GET by ID endpoint not implemented yet")]
    public async Task GetEfaConfigurationById_ShouldReturnNotFound_WhenConfigurationDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(Skip = "GET by ID endpoint not implemented yet")]
    public async Task GetEfaConfigurationById_ShouldReturnBadRequest_WhenIdIsInvalid()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"{BaseUrl}/invalid-guid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Update Tests - SKIP if not implemented

    [Fact(Skip = "UPDATE endpoint not implemented yet")]
    public async Task UpdateEfaConfiguration_ShouldReturnNoContent_WhenUpdateIsSuccessful()
    {
        // Arrange
        var config = new EfaConfiguration
        {
            Id = Guid.NewGuid(),
            Year = 2025,
            EfaRate = 10
        };
        DbContext.EfaConfigurations.Add(config);
        await DbContext.SaveChangesAsync();

        var updateRequest = new
        {
            Year = 2025,
            EfaRate = 12
        };

        // Act
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{config.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updatedConfig = await DbContext.EfaConfigurations.FindAsync(config.Id);
        updatedConfig.Should().NotBeNull();
        updatedConfig!.EfaRate.Should().Be(updateRequest.EfaRate);
    }

    [Fact(Skip = "UPDATE endpoint not implemented yet")]
    public async Task UpdateEfaConfiguration_ShouldReturnNotFound_WhenConfigurationDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new
        {
            Year = 2025,
            EfaRate = 12
        };

        // Act
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(Skip = "UPDATE endpoint not implemented yet")]
    public async Task UpdateEfaConfiguration_ShouldReturnBadRequest_WhenEfaRateIsInvalid()
    {
        // Arrange
        var config = new EfaConfiguration
        {
            Id = Guid.NewGuid(),
            Year = 2025,
            EfaRate = 10
        };
        DbContext.EfaConfigurations.Add(config);
        await DbContext.SaveChangesAsync();

        var updateRequest = new
        {
            Year = 2025,
            EfaRate = -5
        };

        // Act
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{config.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(Skip = "UPDATE endpoint not implemented yet")]
    public async Task UpdateEfaConfiguration_ShouldReturnConflict_WhenYearConflictsWithExisting()
    {
        // Arrange
        var config1 = new EfaConfiguration
        {
            Id = Guid.NewGuid(),
            Year = 2025,
            EfaRate = 10
        };
        var config2 = new EfaConfiguration
        {
            Id = Guid.NewGuid(),
            Year = 2026,
            EfaRate = 11
        };
        DbContext.EfaConfigurations.AddRange(config1, config2);
        await DbContext.SaveChangesAsync();

        var updateRequest = new
        {
            Year = 2026,
            EfaRate = 12
        };

        // Act
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{config1.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion

    #region Delete Tests - SKIP if not implemented

    [Fact(Skip = "DELETE endpoint not implemented yet")]
    public async Task DeleteEfaConfiguration_ShouldReturnNoContent_WhenDeletionIsSuccessful()
    {
        // Arrange
        var config = new EfaConfiguration
        {
            Id = Guid.NewGuid(),
            Year = 2025,
            EfaRate = 10
        };
        DbContext.EfaConfigurations.Add(config);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"{BaseUrl}/{config.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deletedConfig = await DbContext.EfaConfigurations.FindAsync(config.Id);
        deletedConfig.Should().BeNull();
    }

    [Fact(Skip = "DELETE endpoint not implemented yet")]
    public async Task DeleteEfaConfiguration_ShouldReturnNotFound_WhenConfigurationDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(Skip = "DELETE endpoint not implemented yet")]
    public async Task DeleteEfaConfiguration_ShouldReturnBadRequest_WhenIdIsInvalid()
    {
        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"{BaseUrl}/invalid-guid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Get By Year Tests - SKIP if not implemented

    [Fact(Skip = "GET by Year endpoint not implemented yet")]
    public async Task GetEfaConfigurationByYear_ShouldReturnOk_WhenConfigurationExists()
    {
        // Arrange
        var config = new EfaConfiguration
        {
            Id = Guid.NewGuid(),
            Year = 2025,
            EfaRate = 10
        };
        DbContext.EfaConfigurations.Add(config);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"{BaseUrl}/year/{config.Year}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<EfaConfigResponse>();
        result.Should().NotBeNull();
        result!.Year.Should().Be(config.Year);
        result.EfaRate.Should().Be(config.EfaRate);
    }

    [Fact(Skip = "GET by Year endpoint not implemented yet")]
    public async Task GetEfaConfigurationByYear_ShouldReturnNotFound_WhenConfigurationDoesNotExist()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"{BaseUrl}/year/2030");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    private record CreateResponse(Guid Id);
    private record EfaConfigResponse(Guid Id, int Year, decimal EfaRate);
}