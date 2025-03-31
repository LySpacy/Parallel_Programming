using TaskCompletionSource.Models;

namespace TaskCompletionSource.Interfaces;

public interface INetworkAdapter
{
    // Отправляет запрос
    Task WriteAsync(Request request, CancellationToken cancellationToken);
    
    // Вычитывает ответ
    Task<Response> ReadAsync(CancellationToken cancellationToken);
}