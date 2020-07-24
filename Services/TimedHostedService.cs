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

            this.logger.LogInformation("Started timer");

            return Task.CompletedTask;
        }

        private void FireTask(object state)
        {
            if (this._executingTask == null || this._executingTask.IsCompleted)
            {
                this.logger.LogInformation("No task is running, check for new job");
                this._executingTask = this.ExecuteNextJobAsync(this._stoppingCts.Token);
            }
            else
            {
                this.logger.LogInformation("There is a task still running, wait for next cycle");
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
                this.logger.LogInformation("No new job found, wait for next cycle");
                return;
            }

            // simulate long running job
            this.logger.LogInformation("Execute job with Id: {0} Delay: {1}", nextJobData.Id, nextJobData.Delay);

            await Task.Delay(TimeSpan.FromSeconds(nextJobData.Delay));

            this.logger.LogInformation("Job execution finished (Id: {0})", nextJobData.Id);

            // remove executed job from queue
            context.Remove(nextJobData);
            await context.SaveChangesAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Initiate graceful shutdown");

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