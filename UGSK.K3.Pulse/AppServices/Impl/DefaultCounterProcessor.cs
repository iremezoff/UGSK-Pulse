using System;
using System.Threading.Tasks;
using UGSK.K3.Pulse.Infrastructure;

namespace UGSK.K3.Pulse.AppServices.Impl
{
    class DefaultCounterProcessor : ICounterProcessor
    {
        private readonly IBroadcaster _broadcaster;
        private readonly IDataStorage _storage;
        private readonly ILogger _logger;

        public DefaultCounterProcessor(IBroadcaster broadcaster, IDataStorage storage, ILogger logger)
        {
            _broadcaster = broadcaster;
            _storage = storage;
            _logger = logger;
        }

        public async Task ProcessAsync(SaleSystemNotification notification)
        {
            var counter = new Counter() { Product = notification.Product, PeriodKind = PeriodKind.Daily, PeriodStart = DateTimeOffset.Now.Date, Kind = CounterKind.Total };

            await _logger.Write(notification);

            var delta = notification.Increment ? 1 : -1;

            var savedCounter = await _storage.UpdateCounter(counter, delta);

            await _broadcaster.SendCounter(new CounterMessage
            {
                Product = notification.Product,
                Value = savedCounter.Value,
                PeriodStart = counter.PeriodStart.Date,
                Kind = CounterKind.Total,
                PeriodKind = PeriodKind.Daily
            });
        }
    }
}