using System;
using System.Globalization;
using System.Threading.Tasks;
using UGSK.K3.Pulse.Infrastructure;

namespace UGSK.K3.Pulse.Processors
{
    public class PerWeekDailyAverageStatisticProcessor : IStatisticProcessor
    {
        public const string Name = "per-week daily average";

        private const int DaysInWeek = 7;
        private readonly IDataStorage _dataStorage;
        private readonly IBroadcaster _broadcaster;

        public PerWeekDailyAverageStatisticProcessor(IDataStorage dataStorage, IBroadcaster broadcaster)
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

                var weeklyCounter =
                    await _dataStorage.GetCounterOrDefault(product, PeriodKind.Weekly, periodStart, CounterKind.Average);

                if (!weeklyCounter.IsClosed)
                {
                    var dailyCounter =
                        await
                            _dataStorage.GetCounterOrDefault(product, PeriodKind.Daily, dateForHandle, CounterKind.Total);
                    
                    // check if dailyCounter already was accepted
                    if (weeklyCounter.PeriodActualDate < dailyCounter.PeriodStart)
                    {
                        var totalDays = (-1 * dayOffset + 1);
                        var average = (weeklyCounter.Value * -1 * dayOffset + dailyCounter.Value) / totalDays;

                        if (totalDays == DaysInWeek)
                        {
                            weeklyCounter.IsClosed = true;
                        }

                        weeklyCounter.PeriodActualDate = dailyCounter.PeriodStart;

                        weeklyCounter = await _dataStorage.UpdateCounter(weeklyCounter, average - weeklyCounter.Value);
                    }
                }

                await _broadcaster.SendCounter(new CounterMessage
                {
                    Product = product,
                    PeriodKind = PeriodKind.Weekly,
                    Value = weeklyCounter.Value,
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