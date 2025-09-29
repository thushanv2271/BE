using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;

namespace Application.EfaConfigs.Create;

// This command is used to create a new EfaConfiguration in the system.
public sealed record CreateEfaConfigurationCommand(
    int Year,
    decimal EfaRate,
    Guid UpdatedBy
) : ICommand<Guid>; // Implements ICommand and will return a Guid (ID of the created record)


