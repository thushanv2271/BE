using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;

namespace Application.EfaConfigs.GetAll;

/// <summary>
/// Represents a request to retrieve all EFA configurations.
/// Returns a list of <see cref="EfaConfigurationResponse"/> objects.
/// </summary>
public sealed record GetAllEfaConfigurationsQuery : IQuery<List<EfaConfigurationResponse>>;
