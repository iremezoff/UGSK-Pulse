using System;
using UGSK.K3.Pulse.Infrastructure;

namespace UGSK.K3.Pulse.Processors.SyncAdapters
{
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
}