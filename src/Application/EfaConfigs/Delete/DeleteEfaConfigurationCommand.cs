using Application.Abstractions.Messaging;

namespace Application.EfaConfigs.Delete;

public sealed record DeleteEfaConfigurationCommand(
    Guid Id,
    Guid DeletedBy
) : ICommand<DeleteEfaConfigurationResponse>;

public sealed record DeleteEfaConfigurationResponse(
    Guid Id,
    int Year,
    DateTime DeletedAt
);

