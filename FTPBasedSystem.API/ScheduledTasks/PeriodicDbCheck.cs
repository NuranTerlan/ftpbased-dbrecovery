using System;
using System.Threading.Tasks;
using FTPBasedSystem.API.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FTPBasedSystem.API.ScheduledTasks
{
    public class PeriodicDbCheck : ScheduledProcessor
    {
        protected override string Schedule => "1 * * * *";

        private readonly ILogger<PeriodicDbCheck> _logger;

        public PeriodicDbCheck(IServiceScopeFactory serviceScopeFactory, ILogger<PeriodicDbCheck> logger) : base(serviceScopeFactory)
        {
            _logger = logger;
        }

        public override async Task ProcessInScope(IServiceProvider scopeServiceProvider)
        {
            _logger.LogInformation($"{nameof(PeriodicDbCheck)} called: at {DateTime.UtcNow.ToShortTimeString()}" +
                                   $" | {DateTime.UtcNow.ToLongDateString()}");
            await Task.Run(() => Task.CompletedTask);
        }
    }
}