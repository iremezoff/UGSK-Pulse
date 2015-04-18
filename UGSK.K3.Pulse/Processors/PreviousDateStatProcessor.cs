using System;
using System.Threading.Tasks;
using UGSK.K3.Pulse.Infrastructure;

namespace UGSK.K3.Pulse.Processors
{
    public class PreviousDateStatProcessor : IStatisticProcessor
    {
        private readonly IBroadcaster _broadcaster;
        private readonly IDataStorage _dataStorage;

        public PreviousDateStatProcessor(IBroadcaster broadcaster, IDataStorage dataStorage)
        {
            _broadcaster = broadcaster;
            _dataStorage = dataStorage;
        }

        public static string Name = "previous date counter";

        public async Task ProcessAsync(DateTime passedDate)
        {
            foreach (var product in await _dataStorage.GetProducts())
            {
                var dailyCounter =
                    await _dataStorage.GetCounterOrDefault(product, PeriodKind.Daily, passedDate, CounterKind.Total);

                if (!dailyCounter.IsClosed)
                {
                    dailyCounter.IsClosed = true;
                    await _dataStorage.UpdateCounter(dailyCounter, 0);
                }

                var currentDate = DateTime.Now.Date;
                var currentDailyCounter =
                    await
                        _dataStorage.GetCounterOrDefault(product, PeriodKind.Daily, currentDate, CounterKind.Total);

                await _broadcaster.SendCounter(new CounterMessage()
                {
                    PeriodKind = PeriodKind.Daily,
                    PeriodStart = currentDate,
                    Product = product,
                    Kind = CounterKind.Total,
                    Value = currentDailyCounter.Value
                });
            }
        }
    }
}