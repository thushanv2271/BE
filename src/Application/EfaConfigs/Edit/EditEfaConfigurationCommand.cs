using Application.Abstractions.Messaging;

namespace Application.EfaConfigs.Edit;

public sealed record EditEfaConfigurationCommand(
    Guid Id,
    int Year,
    decimal EfaRate,
    Guid UpdatedBy
) : ICommand<EditEfaConfigurationResponse>;

public sealed record EditEfaConfigurationResponse(
    Guid Id,
    int Year,
    decimal EfaRate,
    DateTime UpdatedAt,
    Guid UpdatedBy
)
{
}
