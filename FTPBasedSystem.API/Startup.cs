using System;
using FTPBasedSystem.API.Configs;
using FTPBasedSystem.API.Helpers;
using FTPBasedSystem.API.ScheduledTasks;
using FTPBasedSystem.API.ScheduledTasks.Abstract;
using FTPBasedSystem.DATAACCESS.Data;
using FTPBasedSystem.DATAACCESS.Data.Abstraction;
using FTPBasedSystem.SERVICES.Abstraction;
using FTPBasedSystem.SERVICES.Concretion;
using FTPBasedSystem.SERVICES.FtpServices.Abstraction;
using FTPBasedSystem.SERVICES.FtpServices.Concretion;
using FTPBasedSystem.SERVICES.MappingProfiles;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace FTPBasedSystem.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FTPBasedSystem.API", Version = "v1" });
            });

            // ADDING DB and CONFIGURING MIGRATION PATHS
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                    optionsBuilder => 
                        optionsBuilder.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));


            services.AddScoped<IAppDbContext>(provider => provider.GetService<AppDbContext>());
            services.AddScoped<ICustomLoggingService, CustomLoggingService>();
            services.AddScoped<INumericService, NumericService>();
            services.AddScoped<ITextService, TextService>();
            services.AddScoped<IDateService, DateService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IFtpHelpers, FtpHelpers>();
            services.AddTransient<ICheckDatabaseMiddleware, CheckDatabaseMiddleware>();

            services.AddAutoMapper(typeof(EntitiesMappingProfile));

            services.AddSingleton<AutomaticRetryAttribute>();
            services.AddHangfire((provider, config) =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseDefaultTypeSerializer()
                    .UseMemoryStorage();
                config.UseFilter(provider.GetRequiredService<AutomaticRetryAttribute>());
            });
            services.AddHangfireServer();

            // add functionality to inject IOptions<T> Generic Interface to use configurations from appsettings.json
            services.AddOptions();
            services.Configure<CronOptions>(Configuration.GetSection(nameof(CronOptions)));
            services.Configure<FilePathOptions>(Configuration.GetSection(nameof(FilePathOptions)));
            services.Configure<FtpRequestOptions>(Configuration.GetSection(nameof(FtpRequestOptions)));
            services.Configure<HangfireOptions>(Configuration.GetSection(nameof(HangfireOptions)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider, 
            IOptions<CronOptions> cronOptions, IOptions<HangfireOptions> hangfireOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FTPBasedSystem.API v1"));
            }

            var hangConfig = hangfireOptions.Value;
            var databaseChecker = serviceProvider.GetRequiredService<ICheckDatabaseMiddleware>();
            app.UseHangfireDashboard(hangConfig.DashboardUiEndpoint);
            var cron = Generators.CronGenerator(cronOptions.Value);
            recurringJobManager.AddOrUpdate(hangConfig.RecurringJobId,
                () => databaseChecker.FetchAndSendToFtpServer(), cron);

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
