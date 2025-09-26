using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;

namespace Application.EfaConfigs.Create;
public sealed record CreateEfaConfigurationCommand(
    int Year,
    decimal EfaRate,
    Guid UpdatedBy
) : ICommand<Guid>;
