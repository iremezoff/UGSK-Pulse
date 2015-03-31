using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Hangfire;
using System.Collections.Generic;

namespace UGSK.K3.Pulse
{
    public class CounterController : ApiController
    {
        private readonly ICounterQuery _counterQuery;

        public CounterController(ICounterQuery counterQuery)
        {
            _counterQuery = counterQuery;

        }

        public async Task<Counter> Get(string product, PeriodKind periodKind = PeriodKind.Daily, DateTimeOffset? periodStart = null, CounterKind counterKind = CounterKind.Total)
        {
            return await _counterQuery.GetCounter(product, periodKind, periodStart, counterKind);
        }

        public IHttpActionResult Post(SaleSystemNotification notification)
        {
            // run asynchronously to leave web request thread
            BackgroundJob.Enqueue<DefaultCounterProcessorAdapter>(p => p.Process(notification));

            return StatusCode(HttpStatusCode.Accepted);
        }
    }

    public class IndexController : ApiController
    {
        private readonly IDataStorage _dataStorage;
        private readonly IIndexProcessor _processor;

        public IndexController(IDataStorage dataStorage, IIndexProcessor processor)
        {
            _dataStorage = dataStorage;
            _processor = processor;
        }

        public async Task<IEnumerable<Index>> Get()
        {
            return await _dataStorage.GetIndexes();
        }

        public async Task<Index> Get(string product, PeriodKind periodKind = PeriodKind.Daily)
        {
            return await _dataStorage.GetIndex(product);
        }

        public async Task<IHttpActionResult> Post(Index index)
        {
            try
            {
                await _processor.Process(index);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                return StatusCode(HttpStatusCode.Conflict);
            }
            

            return StatusCode(HttpStatusCode.Created);
        }

        public async Task<IHttpActionResult> Put(Index index)
        {
            await _dataStorage.UpdateIndex(index);
            return StatusCode(HttpStatusCode.OK);

        }
    }

    public class ProductsController : ApiController
    {
        private readonly IDataStorage _dataStorage;
        public ProductsController(IDataStorage dataStorage)
        {
            _dataStorage = dataStorage;
        }

        public async Task<IEnumerable<string>> Get()
        {
            var products = await _dataStorage.GetProducts();
            return products;
        }
    }
}