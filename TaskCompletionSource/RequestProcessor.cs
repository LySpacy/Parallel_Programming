using TaskCompletionSource.Interfaces;
using TaskCompletionSource.Models;

namespace TaskCompletionSource;

public class RequestProcessor(INetworkAdapter networkAdapter) : IRequestProcessor
{
    private Dictionary<Guid, TaskCompletionSource<Response>> _tasksRequest = new ();
    private bool _runInProgress;


    public async Task<Response> SendAsync(Request request, CancellationToken cancellationToken)
    {
        if (_runInProgress)
        {
            var taskCompletionSource = new TaskCompletionSource<Response>();

            _tasksRequest[request.Id] = taskCompletionSource; 
            
            try
            {
                await networkAdapter.WriteAsync(request, cancellationToken);
                return await taskCompletionSource.Task.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                taskCompletionSource.TrySetCanceled();
                throw; 
            }
            catch (Exception)
            {
                taskCompletionSource.TrySetException(new TaskCanceledException());
                throw;
            }
            finally
            {
                _tasksRequest.Remove(request.Id); 
            }
        }
        else
        {
            throw new TaskCanceledException();
        }
        
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _runInProgress = true;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Response response;
                
                try
                {
                    response = await networkAdapter.ReadAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    continue;
                }

                if (_tasksRequest.TryGetValue(response.Id, out var taskCompletionSource))
                {
                    taskCompletionSource.TrySetResult(response);
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
        }
        finally
        {
            foreach (var task in _tasksRequest)
            {
                task.Value.TrySetCanceled();
                _tasksRequest.Remove(task.Key);
            }

            _runInProgress = false;
        }
    }

}