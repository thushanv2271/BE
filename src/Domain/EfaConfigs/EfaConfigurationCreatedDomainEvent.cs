using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedKernel;

namespace Domain.EfaConfigs;
public sealed record EfaConfigurationCreatedDomainEvent(Guid EfaConfigurationId) : IDomainEvent;
