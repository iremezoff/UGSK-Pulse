using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Dapper;
using Hangfire;
using Hangfire.SqlServer;
using LightInject;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Nancy;
using Owin;
using LightInject.Nancy;
using Nancy.Bootstrapper;
using Nancy.Owin;

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
            container.Register<AverageWeekStatisticDailyProcessorAdapter, AverageWeekStatisticDailyProcessorAdapter>(new PerRequestLifeTime());
            container.Register<PreviousDateStatCleanerAdapter, PreviousDateStatCleanerAdapter>(new PerRequestLifeTime());

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

            InitializeJobs();

            app.UseNancy(options =>
              options.PerformPassThrough = context =>
                  context.Response.StatusCode == HttpStatusCode.NotFound);
        }

        public static void InitializeJobs()
        {
            RecurringJob.AddOrUpdate<AverageWeekStatisticDailyProcessorAdapter>(AverageWeekStatisticDailyProcessor.Name, p => p.Process(DateTime.Now.AddDays(-1).Date), "0 1 * * *");

            RecurringJob.AddOrUpdate<PreviousDateStatCleanerAdapter>(PreviousDateStatCleaner.Name, p => p.Process(DateTime.Now.AddDays(-1).Date), "1 0 * * *");

            // perform to update statistic if it has not been updated (worker failed, app wasn't run early)
            RecurringJob.Trigger(AverageWeekStatisticDailyProcessor.Name);
        }
    }

    public class PreviousDateStatCleaner
    {
        private readonly IBroadcaster _broadcaster;
        private readonly IDataStorage _dataStorage;

        public PreviousDateStatCleaner(IBroadcaster broadcaster, IDataStorage dataStorage)
        {
            _broadcaster = broadcaster;
            _dataStorage = dataStorage;
        }

        public static string Name = "previous date counter";

        public async Task ProcessAsync(DateTime passedDate)
        {
            foreach (var product in await _dataStorage.GetProducts())
            {
                await
                    _broadcaster.SendCounter(new CounterMessage()
                    {
                        PeriodKind = PeriodKind.Daily,
                        PeriodStart = passedDate,
                        Product = product,
                        Kind = CounterKind.Total,
                        Value = 0
                    });
            }
        }
    }

    class PreviousDateStatCleanerAdapter : PreviousDateStatCleaner
    {
        public PreviousDateStatCleanerAdapter(IBroadcaster broadcaster, IDataStorage dataStorage)
            : base(broadcaster, dataStorage)
        {
        }

        public void Process(DateTime dateForHandle)
        {
            ProcessAsync(dateForHandle).Wait();
        }
    }

    public class AverageWeekStatisticDailyProcessor
    {
        public const string Name = "daily for week average";

        private const int DaysInWeek = 7;
        private readonly IDataStorage _dataStorage;
        private readonly IBroadcaster _broadcaster;

        public AverageWeekStatisticDailyProcessor(IDataStorage dataStorage, IBroadcaster broadcaster)
        {
            _dataStorage = dataStorage;
            _broadcaster = broadcaster;
        }



        public async Task ProcessAsync(DateTime dateForHandle)
        {
            var products = await _dataStorage.GetProducts();

            foreach (var product in products)
            {
                var calendar = CultureInfo.GetCultureInfo("ru-RU").Calendar;
                var dayOfWeek = calendar.GetDayOfWeek(dateForHandle);
                var dayOffset = DayOfWeek.Monday - dayOfWeek;

                // because Sunday is 7th day in a week but not 0th
                if (dayOffset > 0)
                {
                    dayOffset -= DaysInWeek;
                }

                var periodStart = calendar.AddDays(dateForHandle, dayOffset);

                var counter =
                    await _dataStorage.GetCounter(product, PeriodKind.Weekly, periodStart, CounterKind.Average) ??
                    new Counter
                    {
                        Kind = CounterKind.Average,
                        PeriodKind = PeriodKind.Weekly,
                        PeriodStart = periodStart,
                        Product = product,
                        Value = 0,
                    };
                if (counter.IsClosed)
                {
                    continue;
                }

                var dailyCounter =
                    await _dataStorage.GetCounter(product, PeriodKind.Daily, dateForHandle, CounterKind.Total) ??
                    new Counter
                    {
                        Kind = CounterKind.Total,
                        PeriodKind = PeriodKind.Daily,
                        PeriodStart = dateForHandle,
                        Product = product,
                        Value = 0
                    };

                if (dailyCounter.IsClosed)
                {
                    continue;
                }

                var totalDays = (-1 * dayOffset + 1);
                var average = (counter.Value * -1 * dayOffset + dailyCounter.Value) / totalDays;

                if (totalDays == DaysInWeek)
                {
                    counter.IsClosed = true;
                }

                await _dataStorage.UpdateCounter(counter, average - counter.Value);

                dailyCounter.IsClosed = true;

                await _dataStorage.UpdateCounter(dailyCounter, 0);

                await _broadcaster.SendCounter(new CounterMessage
                {
                    Product = product,
                    PeriodKind = PeriodKind.Weekly,
                    Value = average,
                    PeriodStart = periodStart,
                    Kind = CounterKind.Average
                });
            }
        }
    }

    class AverageWeekStatisticDailyProcessorAdapter : AverageWeekStatisticDailyProcessor
    {
        public AverageWeekStatisticDailyProcessorAdapter(IDataStorage dataStorage, IBroadcaster broadcaster)
            : base(dataStorage, broadcaster)
        {
        }

        public void Process(DateTime dateForHandle)
        {
            ProcessAsync(dateForHandle).Wait();
        }
    }

    public class ContainerJobActivator : JobActivator
    {
        private ServiceContainer _container;

        public ContainerJobActivator(ServiceContainer container)
        {
            _container = container;
        }

        public override object ActivateJob(Type type)
        {
            using (_container.BeginScope())
            {
                return _container.GetInstance(type);
            }
        }
    }

    // ReSharper disable once UnusedMember.Global
    public class Bootstrapper : LightInjectNancyBootstrapper
    {
        protected override IServiceContainer GetServiceContainer()
        {
            return base.GetServiceContainer();
        }

        protected override void ApplicationStartup(IServiceContainer container, IPipelines pipelines)
        {
            StaticConfiguration.DisableErrorTraces = false;
            base.ApplicationStartup(container, pipelines);
        }
    }

    public class ScriptResourceNancyModule : NancyModule
    {
        public ScriptResourceNancyModule()
        {
            Get["sales-statistic"] = parameters =>
            {
                var owinEnv = Context.GetOwinEnvironment();

                var requestHeaders = (IDictionary<string, string[]>)owinEnv["owin.RequestHeaders"];

                var uri = string.Format("{0}://{1}{2}", owinEnv["owin.RequestScheme"], requestHeaders["Host"].First(),
                    owinEnv["owin.RequestPathBase"]);

                var env = new StatisticGainerEnvironment { ServiceAddress = uri };
                return Negotiate
                    .WithModel(env)
                    .WithHeader("Content-Type", "text/javascript")
                    .WithMediaRangeModel("text/javascript", env)
                    .WithView("statistic");
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
        Task ProcessAsync(SaleSystemNotification notification);
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

        public DefaultLogger()
        {
            _conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Pulse"].ConnectionString);
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

    public interface IBroadcaster
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
            _hub.Clients.All.broadcastCounter(counter);
        }

        public async Task SendIndex(IndexMessage index)
        {
            _hub.Clients.All.broadcastIndex(index.Product, index.Value);
        }
    }

    public interface IDataStorage
    {
        Task<IEnumerable<string>> GetProducts();
        Task<IEnumerable<Index>> GetIndexes();
        Task<Counter> GetCounter(string product, PeriodKind periodKind, DateTimeOffset periodStart, CounterKind counterKind);
        Task<Index> GetIndex(string product);
        Task<Counter> UpdateCounter(Counter counter, int delta);
        Task<Index> UpdateIndex(Index index);
        Task<Index> CreateIndex(Index index);
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
        public DapperDataStorage()
        {
            _conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Pulse"].ConnectionString);
        }

        public async Task<IEnumerable<string>> GetProducts()
        {
            return (await _conn.QueryAsync<Index>("select distinct Product from [Index]")).Select(p => p.Product);
        }

        public async Task<Counter> GetCounter(string product, PeriodKind periodKind, DateTimeOffset periodStart, CounterKind counterKind)
        {
            var counter = (await
                    _conn.QueryAsync<Counter>(
                        "select top 1 * from Counter where Product=@product and PeriodStart=@periodStart and PeriodKind=@periodKind and CounterKind=@counterKind",
                        new { product, periodKind, periodStart = periodStart.Date, counterKind })).SingleOrDefault();

            return counter ?? new Counter
            {
                Product = product,
                Kind = counterKind,
                PeriodStart = periodStart,
                PeriodKind = periodKind
            };
        }

        public async Task<IEnumerable<Index>> GetIndexes()
        {
            return await _conn.QueryAsync<Index>("select * from [Index]");
        }


        public async Task<Index> GetIndex(string product)
        {
            var index = (await
                _conn.QueryAsync<Index>(
                    "select top 1 * from [Index] where Product=@product",
                    new { product })).SingleOrDefault();
            return index ?? new Index { Product = product, Value = 100 };
        }

        public async Task<Counter> UpdateCounter(Counter counter, int delta)
        {
            await _conn.ExecuteAsync(
                "begin tran update Counter set Value=Value+@delta, IsClosed=@isClosed where Product=@product and PeriodStart=@periodStart and PeriodKind=@periodKind and CounterKind=@counterKind " +
                "if @@rowcount = 0 " +
                "begin " +
                "insert Counter (Product, PeriodStart, PeriodKind, CounterKind, Value, IsClosed) values (@product, @periodStart, @periodKind, @counterKind, 0, 0) " +
                "end " +
                "commit tran", new { counter.Product, counter.PeriodKind, periodStart = counter.PeriodStart.Date, counterKind = counter.Kind, delta, isClosed = counter.IsClosed });

            return await GetCounter(counter.Product, counter.PeriodKind, counter.PeriodStart.Date, counter.Kind);
        }

        public async Task<Index> UpdateIndex(Index index)
        {
            await _conn.ExecuteAsync(
            "update [Index] set Value=@value where Product=@product",
            new { index.Product, index.Value });

            return await GetIndex(index.Product);

        }

        public async Task<Index> CreateIndex(Index index)
        {
            return await await _conn.ExecuteAsync("insert [Index] (Product, Value) VALUES (@product, @value)",
                new { index.Product, index.Value }).ContinueWith(t => GetIndex(index.Product));
        }
    }
}