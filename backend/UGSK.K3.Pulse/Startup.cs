using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Dapper;
using LightInject;
using LightInject.Nancy;
using LightInject.WebApi;
using LightInject.WebApi;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Owin;
using Owin;

[assembly: OwinStartup(typeof(UGSK.K3.Pulse.Startup))]

namespace UGSK.K3.Pulse
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var container = new ServiceContainer();
            container.Register<ICounterProcessor, DefaultCounterProcessor>(new PerScopeLifetime());
            container.Register<IIndexProcessor, DefaultIndexProcessor>(new PerScopeLifetime());
            container.Register<ILogger, DefaultLogger>(new PerScopeLifetime());
            container.Register<IBroadcaster, SignalRBroadcaster>(new PerRequestLifeTime());
            container.Register<IDataStorage, DapperDataStorage>(new PerScopeLifetime());
            container.Register<IDbConnection, SqlConnection>(new PerScopeLifetime());
            container.Register<ICounterQuery, CounterQuery>(new PerScopeLifetime());

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

            app.UseNancy(options =>
              options.PerformPassThrough = context =>
                  context.Response.StatusCode == HttpStatusCode.NotFound);
        }
    }

    // ReSharper disable once UnusedMember.Global
    public class Bootstrapper : LightInjectNancyBootstrapper
    {
        protected override IServiceContainer GetServiceContainer()
        {
            return base.GetServiceContainer();
        }
    }

    public class ScriptResourceNancyModule : NancyModule
    {
        public ScriptResourceNancyModule()
        {
            Get["/sales-statistic"] = parameters =>
            {
                var owinEnv = Context.GetOwinEnvironment();

                var requestHeaders = (IDictionary<string, string[]>)owinEnv["owin.RequestHeaders"];

                var uri = string.Format("{0}://{1}", owinEnv["owin.RequestScheme"], requestHeaders["Host"].First());

                var env = new StatisticGainerEnvironment() { ServiceAddress = uri };
                return Negotiate
                    .WithModel(env)
                    .WithHeader("Content-Type", "text/javascript")
                    .WithMediaRangeModel("text/javascript", env)
                    .WithView("statistic.cshtml");
            };
        }
    }

    public class StatisticGainerEnvironment
    {
        public string ServiceAddress { get; set; }
    }

    public class StatisticHub : Hub
    {
    }

    public interface ICounterProcessor
    {
        Task Process(SaleSystemNotification notification);
    }

    public interface IIndexProcessor
    {
        Task Process(Index index);
    }

    interface ILogger
    {
        Task Write(SaleSystemNotification notification);
    }

    class DefaultLogger : ILogger
    {
        private readonly IDbConnection _conn;

        public DefaultLogger(IDbConnection conn)
        {
            _conn = conn;
            _conn.ConnectionString = ConfigurationManager.ConnectionStrings["Pulse"].ConnectionString;
        }

        public async Task Write(SaleSystemNotification notification)
        {
            await
                _conn.ExecuteAsync(
                    "insert into Hit (Product, Filial, ContractSigningDate, Increment) values (@product, @filial, @signingdate, @increment)",
                    new
                    {
                        product = notification.Product,
                        filial = notification.Filial,
                        signingdate = notification.ContractSigningDateTime,
                        increment = notification.Increment
                    });
        }
    }

    interface IBroadcaster
    {
        Task SendCounter(CounterMessage counter);
        Task SendIndex(IndexMessage index);
    }

    class SignalRBroadcaster : IBroadcaster
    {
        private readonly IHubContext _hub;

        public SignalRBroadcaster()
        {
            _hub = GlobalHost.ConnectionManager.GetHubContext<StatisticHub>();
        }

        public async Task SendCounter(CounterMessage counter)
        {
            _hub.Clients.All.broadcastCounter(counter.Product, counter.Value);
        }

        public async Task SendIndex(IndexMessage index)
        {
            _hub.Clients.All.broadcastIndex(index.Product, index.Value);
        }
    }

    public interface IDataStorage
    {
        Task<Counter> GetCounter(string product, PeriodKind periodKind, DateTimeOffset periodStart, CounterKind counterKind);
        Task<Index> GetIndex(string product, PeriodKind periodKind);
        Task<Counter> UpdateCounter(Counter counter, int delta);
        Task<Index> UpdateIndex(Index index);
    }

    public interface ICounterQuery
    {
        Task<Counter> GetCounter(string product, PeriodKind periodKind = PeriodKind.Daily,
            DateTimeOffset? periodStart = null,
            CounterKind kind = CounterKind.Total);
    }

    class DapperDataStorage : IDataStorage
    {
        private readonly IDbConnection _conn;
        public DapperDataStorage(IDbConnection conn)
        {
            _conn = conn;
            _conn.ConnectionString = ConfigurationManager.ConnectionStrings["Pulse"].ConnectionString;
        }

        private Index _globalIndex = new Index() { Value = new Random().Next(100, 120) };

        public async Task<Counter> GetCounter(string product, PeriodKind periodKind, DateTimeOffset periodStart, CounterKind counterKind)
        {
            var counter = (await
                    _conn.QueryAsync<Counter>(
                        "select top 1 * from Counter where Product=@product and PeriodStart=@periodStart and PeriodKind=@periodKind and CounterKind=@counterKind",
                        new { product, periodKind, periodStart = periodStart.Date, counterKind })).SingleOrDefault();

            return counter ?? new Counter()
                {
                    Product = product,
                    Kind = counterKind,
                    PeriodStart = periodStart,
                    PeriodKind = periodKind
                };
        }

        public async Task<Index> GetIndex(string product, PeriodKind periodKind)
        {
            var index = (await
                _conn.QueryAsync<Index>(
                    "select top 1 * from [Index] where Product=@product",
                    new { product, periodKind })).SingleOrDefault();
            return index ?? new Index() { Product = product, PeriodKind = periodKind, Value = 100 };
        }

        public async Task<Counter> UpdateCounter(Counter counter, int delta)
        {
            await _conn.ExecuteAsync(
                "begin tran update Counter set Value=Value+@delta where Product=@product and PeriodStart=@periodStart and PeriodKind=@periodKind and CounterKind=@counterKind " +
                "if @@rowcount = 0 " +
                "begin " +
                "insert Counter (Product, PeriodStart, PeriodKind, CounterKind, Value) values (@product, @periodStart, @periodKind, @counterKind, 0) " +
                "end " +
                "commit tran", new { counter.Product, counter.PeriodKind, periodStart = counter.PeriodStart.Date, counterKind = counter.Kind, delta });

            return await GetCounter(counter.Product, counter.PeriodKind, counter.PeriodStart.Date, counter.Kind);
        }

        public async Task<Index> UpdateIndex(Index index)
        {
            await _conn.ExecuteAsync(
            "update [Index] set Value=@value where Product=@product",
            new { index.Product, index.Value });

            return await GetIndex(index.Product, index.PeriodKind);

        }
    }
}