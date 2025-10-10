using MediatR;
using Microsoft.AspNetCore.Mvc;
using YouDovezu.Application.Queries.Health;

namespace YouDovezu.API.Controllers;

/// <summary>
/// Контроллер для проверки состояния API
/// </summary>
/// <remarks>
/// Предоставляет endpoint для мониторинга работоспособности приложения.
/// Использует MediatR для получения статуса через Application слой.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Инициализирует новый экземпляр HealthController
    /// </summary>
    /// <param name="mediator">MediatR для отправки запросов</param>
    public HealthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Проверяет состояние API
    /// </summary>
    /// <returns>Статус работоспособности</returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var query = new GetHealthStatusQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
