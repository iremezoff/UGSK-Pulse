using System;
using System.Globalization;
using System.Threading.Tasks;
using UGSK.K3.Pulse.Infrastructure;

namespace UGSK.K3.Pulse.AppServices.Impl
{
    class CounterQuery : ICounterQuery
    {
        private readonly IDataStorage _dataStorage;

        public CounterQuery(IDataStorage dataStorage)
        {
            _dataStorage = dataStorage;
        }

        public Task<Counter> GetCounter(string product, PeriodKind periodKind = PeriodKind.Daily, DateTimeOffset? periodStart = null,
            CounterKind kind = CounterKind.Total)
        {
            periodStart = periodStart == null
                ? DateTimeOffset.Now.Date
                : periodStart.Value.Date;

            int dayOffset = 0;

            var calendar = CultureInfo.GetCultureInfo("ru-RU").Calendar;

            switch (periodKind)
            {
                case PeriodKind.Daily:
                    dayOffset = 0;
                    break;
                case PeriodKind.Weekly:
                    var dayOfWeek = calendar.GetDayOfWeek(periodStart.Value.DateTime);
                    dayOffset = DayOfWeek.Monday - dayOfWeek;

                    if (dayOffset > 0)
                    {
                        dayOffset -= 7;
                    }
                    break;
                case PeriodKind.Monthly:
                    dayOffset = periodStart.Value.Day == 1 ? 0 : 1 - periodStart.Value.Day;
                    break;
            }

            return _dataStorage.GetCounter(product, periodKind, calendar.AddDays(periodStart.Value.DateTime, dayOffset), kind);
        }

    }
}