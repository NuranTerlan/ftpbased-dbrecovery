using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace FTPBasedSystem.API.BackgroundServices
{
    public abstract class BackgroundService : IHostedService
    {
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCTS = new CancellationTokenSource();

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteAsync(TimeSpan.FromSeconds(1), _stoppingCTS.Token);

            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            return Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask is null)
            {
                return;
            }

            try
            {
                _stoppingCTS.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        protected virtual async Task ExecuteAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            do
            {
                await Process();
                await Task.Delay(interval, cancellationToken);
            } while (!_stoppingCTS.IsCancellationRequested);
        }

        protected abstract Task Process();
    }
}