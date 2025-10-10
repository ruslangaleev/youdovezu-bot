using MediatR;
using YouDovezu.Application.Queries.Health;

namespace YouDovezu.Application.Handlers.Health;

public class GetHealthStatusQueryHandler : IRequestHandler<GetHealthStatusQuery, HealthStatusDto>
{
    public Task<HealthStatusDto> Handle(GetHealthStatusQuery request, CancellationToken cancellationToken)
    {
        var result = new HealthStatusDto
        {
            Message = "YouDovezu API is running",
            Timestamp = DateTime.UtcNow,
            Status = "Healthy"
        };

        return Task.FromResult(result);
    }
}
