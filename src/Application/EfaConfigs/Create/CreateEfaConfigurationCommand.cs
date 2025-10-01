using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;

namespace Application.EfaConfigs.Create;

// Single item for backward compatibility
public sealed record CreateEfaConfigurationCommand(
    int Year,
    decimal EfaRate,
    Guid UpdatedBy
) : ICommand<Guid>;

// New: Bulk save command
public sealed record CreateBulkEfaConfigurationCommand(
    List<EfaConfigurationItem> Items,
    Guid UpdatedBy
) : ICommand<BulkEfaConfigurationResponse>;

public sealed record EfaConfigurationItem(
    int Year,
    decimal EfaRate
);

public sealed record BulkEfaConfigurationResponse(
    List<EfaConfigurationSummary> Created,
    List<EfaConfigurationSummary> Updated,
    int TotalProcessed
);

public sealed record EfaConfigurationSummary(
    Guid Id,
    int Year,
    decimal EfaRate,
    DateTime UpdatedAt
);
