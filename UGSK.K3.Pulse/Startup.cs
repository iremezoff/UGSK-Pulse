using System;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Cors;
using Hangfire;
using Hangfire.SqlServer;
using LightInject;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Nancy;
using Owin;
using UGSK.K3.Pulse.AppServices;
using UGSK.K3.Pulse.AppServices.Impl;
using UGSK.K3.Pulse.Config;
using UGSK.K3.Pulse.Infrastructure;
using UGSK.K3.Pulse.Infrastructure.Impl;
using UGSK.K3.Pulse.Processors;
using UGSK.K3.Pulse.Processors.SyncAdapters;

[assembly: OwinStartup(typeof(UGSK.K3.Pulse.Startup))]

namespace UGSK.K3.Pulse
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var container = new ServiceContainer();
            container.Register<ICounterProcessor, DefaultCounterProcessorAdapter>(new PerScopeLifetime());
            container.Register<DefaultCounterProcessorAdapter, DefaultCounterProcessorAdapter>(new PerRequestLifeTime());
            container.Register<IIndexProcessor, DefaultIndexProcessor>(new PerScopeLifetime());
            container.Register<ILogger, DefaultLogger>(new PerScopeLifetime());
            container.Register<IBroadcaster, SignalRBroadcaster>(new PerRequestLifeTime());
            container.Register<IDataStorage, DapperDataStorage>(new PerScopeLifetime());
            container.Register<ICounterQuery, CounterQuery>(new PerScopeLifetime());
            container.Register<AverageWeekStatisticDailyProcessor, AverageWeekStatisticDailyProcessor>(new PerRequestLifeTime());
            container.Register<PreviousDateStatCleaner, PreviousDateStatCleaner>(new PerRequestLifeTime());
            container.Register(typeof(CommonProcessorAdapter<>), typeof(CommonProcessorAdapter<>), new PerRequestLifeTime());

            container.RegisterApiControllers();

            container.EnableSignalR();

            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute("default", "api/{Controller}");
            config.EnableCors(new EnableCorsAttribute("*", "*", "get"));

            container.EnableWebApi(config);
            container.ScopeManagerProvider = new PerLogicalCallContextScopeManagerProvider(); // for 4.5 async purposes

            app.UseWebApi(config);

            app.UseCors(CorsOptions.AllowAll);

            app.MapSignalR();

            app.UseHangfire(conf =>
            {
                conf.UseSqlServerStorage("Pulse", new SqlServerStorageOptions { PrepareSchemaIfNecessary = false });
                conf.UseServer();
                conf.UseActivator(new ContainerJobActivator(container));
            });

//            InitializeJobs();

            app.UseNancy(options =>
              options.PerformPassThrough = context =>
                  context.Response.StatusCode == HttpStatusCode.NotFound);
        }

        public static void InitializeJobs()
        {
            RecurringJob.AddOrUpdate<CommonProcessorAdapter<AverageWeekStatisticDailyProcessor>>(AverageWeekStatisticDailyProcessor.Name, p => p.Process(DateTime.Now.AddDays(-1).Date), "0 1 * * *");

            RecurringJob.AddOrUpdate<CommonProcessorAdapter<PreviousDateStatCleaner>>(PreviousDateStatCleaner.Name, p => p.Process(DateTime.Now.AddDays(-1).Date), "35 * * * *");

            // perform to update statistic if it has not been updated (worker failed, app wasn't run early)
            RecurringJob.Trigger(AverageWeekStatisticDailyProcessor.Name);
        }
    }


    public class StatisticGainerEnvironment
    {
        public string ServiceAddress { get; set; }
    }
}