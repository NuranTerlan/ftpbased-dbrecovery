using System;
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

            services.AddHangfire(config =>
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseDefaultTypeSerializer()
                    .UseMemoryStorage());
            services.AddHangfireServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IBackgroundJobClient backJobClient, IRecurringJobManager recurringJobManager,
            IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FTPBasedSystem.API v1"));

                var databaseChecker = serviceProvider.GetRequiredService<ICheckDatabaseMiddleware>();
                app.UseHangfireDashboard("/hang-dashboard");
                backJobClient.Enqueue(() => Console.WriteLine("Fired!"));
                recurringJobManager.AddOrUpdate("DbCheck12345", 
                    () => databaseChecker.FetchAndSendToFtpServer(), "*/3 * * * *");
            }

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
