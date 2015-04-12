using System.Threading.Tasks;

namespace UGSK.K3.Pulse.AppServices
{
    public interface ICounterProcessor
    {
        Task ProcessAsync(SaleSystemNotification notification);
    }
}