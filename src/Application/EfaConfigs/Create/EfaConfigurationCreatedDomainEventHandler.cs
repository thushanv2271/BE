using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.EfaConfigs;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.EfaConfigs.Create;


/// <summary>
/// Handles the event when a new EFA configuration is created.
/// Logs information about the created configuration.
/// </summary>
internal sealed class EfaConfigurationCreatedDomainEventHandler(
    ILogger<EfaConfigurationCreatedDomainEventHandler> logger)
    : IDomainEventHandler<EfaConfigurationCreatedDomainEvent>
{
    public Task Handle(
        EfaConfigurationCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "EFA Configuration created with ID: {EfaConfigurationId}",
            domainEvent.EfaConfigurationId);

        return Task.CompletedTask;
    }
}
