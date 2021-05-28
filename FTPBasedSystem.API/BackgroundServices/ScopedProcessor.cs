using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace FTPBasedSystem.API.BackgroundServices
{
    public abstract class ScopedProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        protected ScopedProcessor(IServiceScopeFactory serviceScopeFactory) 
            : base()
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        protected override async Task Process()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            await ProcessInScope(scope.ServiceProvider);
        }

        public abstract Task ProcessInScope(IServiceProvider scopeServiceProvider);
    }
}