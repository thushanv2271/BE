using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.Abstractions.Messaging;

namespace Application.EfaConfigs.Edit;

public sealed record EditEfaConfigurationCommand(
    Guid Id,
    decimal EfaRate,
    Guid UpdatedBy
) : ICommand<EditEfaConfigurationResponse>;

public sealed record EditEfaConfigurationResponse(
    Guid Id,
    int Year,
    decimal EfaRate,
    DateTime UpdatedAt
);
