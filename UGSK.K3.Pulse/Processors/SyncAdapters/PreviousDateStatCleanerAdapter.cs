using System;
using UGSK.K3.Pulse.Infrastructure;

namespace UGSK.K3.Pulse.Processors.SyncAdapters
{
    class PreviousDateStatCleanerAdapter : PreviousDateStatCleaner
    {
        public PreviousDateStatCleanerAdapter(IBroadcaster broadcaster, IDataStorage dataStorage)
            : base(broadcaster, dataStorage)
        {
        }

        public void Process(DateTime dateForHandle)
        {
            ProcessAsync(dateForHandle).Wait();
        }
    }
}