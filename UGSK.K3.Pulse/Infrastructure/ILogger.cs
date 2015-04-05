using System.Threading.Tasks;

namespace UGSK.K3.Pulse.Infrastructure
{
    interface ILogger
    {
        Task Write(SaleSystemNotification notification);
    }
}