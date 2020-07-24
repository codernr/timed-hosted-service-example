using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TimedHostedServiceExample.Model;

namespace TimedHostedServiceExample.Services
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<TimedHostedService> logger;

        private readonly IServiceProvider services;

        private Timer _timer;

        private Task _executingTask;

        private CancellationTokenSource _stoppingCts;

        public TimedHostedService(IServiceProvider services, ILogger<TimedHostedService> logger) =>
            (this.services, this.logger) = (services, logger);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            this._timer = new Timer(this.FireTask, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30));

            return Task.CompletedTask;
        }

        private void FireTask(object state)
        {
            if (this._executingTask == null || this._executingTask.IsCompleted)
            {
                this._executingTask = this.ExecuteNextJobAsync(this._stoppingCts.Token);
            }
        }

        private async Task ExecuteNextJobAsync(CancellationToken cancellationToken)
        {
            using var scope = this.services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<JobDbContext>();

            // whatever logic to retrieve the next job
            var nextJobData = await context.JobDatas.FirstOrDefaultAsync();

            if (nextJobData == null)
            {
                // no next job
                return;
            }

            // simulate long running job
            await Task.Delay(TimeSpan.FromSeconds(nextJobData.Delay));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this._timer.Change(Timeout.Infinite, 0);

            if (this._executingTask == null || this._executingTask.IsCompleted)
            {
                return;
            }

            try
            {
                this._stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(this._executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        public void Dispose()
        {
            this._timer.Dispose();
            this._stoppingCts?.Cancel();
        }
    }
}