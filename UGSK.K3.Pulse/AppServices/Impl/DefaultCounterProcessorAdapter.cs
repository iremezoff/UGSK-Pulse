using UGSK.K3.Pulse.Infrastructure;

namespace UGSK.K3.Pulse.AppServices.Impl
{
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
}