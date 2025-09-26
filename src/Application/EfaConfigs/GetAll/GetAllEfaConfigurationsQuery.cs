using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;

namespace Application.EfaConfigs.GetAll;
public sealed record GetAllEfaConfigurationsQuery : IQuery<List<EfaConfigurationResponse>>;
