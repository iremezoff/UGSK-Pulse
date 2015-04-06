﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Hangfire;

namespace UGSK.K3.Pulse
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

    class DefaultCounterProcessorAdapter : DefaultCounterProcessor
    {
        public DefaultCounterProcessorAdapter(IBroadcaster broadcaster, IDataStorage storage, ILogger logger)
            : base(broadcaster, storage, logger)
        { }

        public void Process(SaleSystemNotification notification)
        {
            ProcessAsync(notification).Wait();
        }
    }

    class DefaultIndexProcessor : IIndexProcessor
    {
        private readonly IBroadcaster _broadcaster;
        private readonly IDataStorage _storage;

        public DefaultIndexProcessor(IBroadcaster broadcaster, IDataStorage storage)
        {
            _broadcaster = broadcaster;
            _storage = storage;

        }

        public async Task Process(Index index)
        {
            var savedCounter = await _storage.CreateIndex(index);

            await _broadcaster.SendIndex(new IndexMessage() { Product = index.Product, Value = savedCounter.Value });

            index.Id = savedCounter.Id;
        }
    }
}