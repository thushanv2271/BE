using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;

namespace Application.EfaConfigs.Create;

//save command
public sealed record CreateEfaConfigurationCommand(
    List<EfaConfigurationItem> Items,
    Guid UpdatedBy
) : ICommand<EfaConfigurationResponse>;

public sealed record EfaConfigurationItem(
    int Year,
    decimal EfaRate
);

public sealed record EfaConfigurationResponse(
    List<EfaConfigurationSummary> Created,
    List<EfaConfigurationSummary> Updated
);

public sealed record EfaConfigurationSummary(
    Guid Id,
    int Year,
    decimal EfaRate,
    DateTime UpdatedAt
);
