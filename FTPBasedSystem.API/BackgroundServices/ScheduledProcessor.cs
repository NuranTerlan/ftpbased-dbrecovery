using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NCrontab;

namespace FTPBasedSystem.API.BackgroundServices
{
    public abstract class ScheduledProcessor : ScopedProcessor
    {
        private readonly CrontabSchedule _schedule;
        private DateTime _nextRun;

        protected abstract string Schedule { get; }

        protected ScheduledProcessor(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
            _schedule = CrontabSchedule.Parse(Schedule);
            _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
        }

        protected override async Task ExecuteAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            do
            {
                var now = DateTime.Now;

                if (now > _nextRun)
                {
                    await Process();

                    _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken); // 5 seconds delay

            } while (!cancellationToken.IsCancellationRequested);
        }

    }
}