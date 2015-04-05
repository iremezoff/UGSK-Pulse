using System.Threading.Tasks;

namespace UGSK.K3.Pulse
{
    public interface ICounterProcessor
    {
        Task ProcessAsync(SaleSystemNotification notification);
    }
}