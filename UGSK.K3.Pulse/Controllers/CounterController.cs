using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Hangfire;
using UGSK.K3.Pulse.AppServices;
using UGSK.K3.Pulse.AppServices.Impl;

namespace UGSK.K3.Pulse.Controllers
{
    public class CounterController : ApiController
    {
        private readonly ICounterQuery _counterQuery;

        public CounterController(ICounterQuery counterQuery)
        {
            _counterQuery = counterQuery;
            
        }

        public async Task<Counter> Get(string product, PeriodKind periodKind = PeriodKind.Daily, DateTimeOffset? periodStart = null, CounterKind counterKind=CounterKind.Total)
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
}