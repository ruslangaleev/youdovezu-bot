using YouDovezu.Application.Common.Models;

namespace YouDovezu.Application.Queries.Health;

public class GetHealthStatusQuery : BaseQuery<HealthStatusDto>
{
}

public class HealthStatusDto
{
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = "Healthy";
}
