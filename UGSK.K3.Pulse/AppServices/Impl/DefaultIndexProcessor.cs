using System.Threading.Tasks;
using UGSK.K3.Pulse.Infrastructure;

namespace UGSK.K3.Pulse.AppServices.Impl
{
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
            var savedCounter = await _storage.UpdateIndex(index);

            await _broadcaster.SendIndex(new IndexMessage() { Product = index.Product, Value = savedCounter.Value });
        }
    }
}