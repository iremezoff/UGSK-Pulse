using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace UGSK.K3.Pulse.Infrastructure.Impl
{
    class SignalRBroadcaster : IBroadcaster
    {
        private readonly IHubContext _hub;

        public SignalRBroadcaster()
        {
            _hub = GlobalHost.ConnectionManager.GetHubContext<StatisticHub>();
        }

        public async Task SendCounter(CounterMessage counter)
        {
            _hub.Clients.All.broadcastCounter(counter);
        }

        public async Task SendIndex(IndexMessage index)
        {
            _hub.Clients.All.broadcastIndex(index.Product, index.Value);
        }
    }
}