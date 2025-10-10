using MediatR;

namespace YouDovezu.Application.Common.Models;

public abstract class BaseCommand : IRequest
{
}

public abstract class BaseCommand<TResponse> : IRequest<TResponse>
{
}

public abstract class BaseQuery<TResponse> : IRequest<TResponse>
{
}
