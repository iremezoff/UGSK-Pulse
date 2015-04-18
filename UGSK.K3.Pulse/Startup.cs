using System;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Hosting;
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
            container.Register<PerWeekDailyAverageStatisticProcessor, PerWeekDailyAverageStatisticProcessor>(new PerRequestLifeTime());
            container.Register<PreviousDateStatProcessor, PreviousDateStatProcessor>(new PerRequestLifeTime());
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
                conf.UseSqlServerStorage(GlobalOptions.HangfireSqlServer.ConnectionsStringName, new SqlServerStorageOptions { PrepareSchemaIfNecessary = GlobalOptions.HangfireSqlServer.PrepareSchemaIfNecessary });
                conf.UseServer();
                conf.UseActivator(new ContainerJobActivator(container));
            });
            app.UseHangfireDashboard();
           

            InitializeJobs();

            app.UseNancy(options =>
              options.PerformPassThrough = context =>
                  context.Response.StatusCode == HttpStatusCode.NotFound);
        }

        public static void InitializeJobs()
        {
            RecurringJob.AddOrUpdate<CommonProcessorAdapter<PerWeekDailyAverageStatisticProcessor>>(PerWeekDailyAverageStatisticProcessor.Name, p => p.Process(DateTime.Now.AddDays(-1).Date), "0 1 * * *");

            RecurringJob.AddOrUpdate<CommonProcessorAdapter<PreviousDateStatProcessor>>(PreviousDateStatProcessor.Name, p => p.Process(DateTime.Now.AddDays(-1).Date), "35 * * * *");

            RecurringJob.AddOrUpdate("test of unslept state", () => WriteLog("start app"), "1 * * * *");

            // perform to update statistic if it has not been updated (worker failed, app wasn't run early)
            RecurringJob.Trigger(PerWeekDailyAverageStatisticProcessor.Name);
            RecurringJob.Trigger(PreviousDateStatProcessor.Name);
            RecurringJob.Trigger("test of unslept state");
        }

        public static void WriteLog(string message = null)
        {
            using (var fs = new StreamWriter(@"c:\Logs\UGSK.K3.Pulse\log.txt", true))
            {
                fs.WriteLine(string.Join("|", new[] { DateTime.Now.ToString(), message }));
            }
        }
    }


    public class StatisticGainerEnvironment
    {
        public string ServiceAddress { get; set; }
    }

    public class ApplicationPreload : System.Web.Hosting.IProcessHostPreloadClient
    {
        public void Preload(string[] parameters)
        {
            HangfireBootstrapper.Instance.Start();
        }
    }

    // for keep working of application and ensure that workers will be performed
    // for more information, see http://docs.hangfire.io/en/latest/deployment-to-production/making-aspnet-app-always-running.html

    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            HangfireBootstrapper.Instance.Start();
        }

        protected void Application_End(object sender, EventArgs e)
        {
            HangfireBootstrapper.Instance.Stop();
        }
    }

    internal static class GlobalOptions
    {
        public static class HangfireSqlServer
        {
            public const string ConnectionsStringName = "Pulse";
            public const bool PrepareSchemaIfNecessary = false;
        }
    }

    public class HangfireBootstrapper : IRegisteredObject
    {
        public static readonly HangfireBootstrapper Instance = new HangfireBootstrapper();

        private readonly object _lockObject = new object();
        private bool _started;

        private BackgroundJobServer _backgroundJobServer;

        public void Start()
        {
            lock (_lockObject)
            {
                if (_started) return;
                _started = true;

                HostingEnvironment.RegisterObject(this);

                JobStorage.Current = new SqlServerStorage(GlobalOptions.HangfireSqlServer.ConnectionsStringName,
                    new SqlServerStorageOptions()
                    {
                        PrepareSchemaIfNecessary = GlobalOptions.HangfireSqlServer.PrepareSchemaIfNecessary
                    });

                _backgroundJobServer = new BackgroundJobServer();
                _backgroundJobServer.Start();

                Startup.WriteLog("server start");
            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                if (_backgroundJobServer != null)
                {
                    _backgroundJobServer.Dispose();
                    Startup.WriteLog("server stop");
                }

                HostingEnvironment.UnregisterObject(this);
            }
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            Stop();
        }
    }

    // don't forget include below to applicationHost.config (%WINDIR%\System32\inetsrv\config\applicationHost.config)
    // see parameters in {}

    //<applicationPools>
    //    <add name="{PoolName}" managedRuntimeVersion="v4.0" startMode="AlwaysRunning" />
    //</applicationPools>

    //<!-- ... -->

    //<sites>
    //    <site name="{SiteName}" id="1">
    //        <application path="/" serviceAutoStartEnabled="true"
    //                              serviceAutoStartProvider="ApplicationPreload" />
    //    </site>
    //</sites>

    //<!-- Just AFTER closing the `sites` element AND AFTER `webLimits` tag -->
    //<serviceAutoStartProviders>
    //    <add name="ApplicationPreload" type="UGSK.K3.Pulse.ApplicationPreload, UGSK.K3.Pulse" />
    //</serviceAutoStartProviders>
}