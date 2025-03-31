using TaskCompletionSource.Models;

namespace TaskCompletionSource.Interfaces;

public interface IRequestProcessor
{
    // При отмене CancellationToken не обязательно гарантировать то,
    // что мы не отправим запрос на сервер, но клиент должен получить отмену задачи
    Task<Response> SendAsync(Request request, CancellationToken cancellationToken);

    // Вспомогательный метод. Гарантированно вызывается 1 раз при старте приложения
    Task RunAsync(CancellationToken cancellationToken);
}