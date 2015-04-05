using System;
using System.Globalization;
using System.Threading.Tasks;
using UGSK.K3.Pulse.Infrastructure;

namespace UGSK.K3.Pulse.Processors
{
    public class AverageWeekStatisticDailyProcessor : IStatisticProcessor
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

    public interface IStatisticProcessor
    {
        Task ProcessAsync(DateTime dateForHandle);
    }
}