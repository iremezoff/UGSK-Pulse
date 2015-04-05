using System.Threading.Tasks;

namespace UGSK.K3.Pulse.Infrastructure
{
    public interface IBroadcaster
    {
        Task SendCounter(CounterMessage counter);
        Task SendIndex(IndexMessage index);
    }
}