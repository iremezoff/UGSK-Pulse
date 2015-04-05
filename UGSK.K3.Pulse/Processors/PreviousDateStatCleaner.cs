using System;
using System.Threading.Tasks;
using UGSK.K3.Pulse.Infrastructure;

namespace UGSK.K3.Pulse.Processors
{
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
}