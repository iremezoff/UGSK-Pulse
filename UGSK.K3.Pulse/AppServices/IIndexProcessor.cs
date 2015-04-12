using System.Threading.Tasks;

namespace UGSK.K3.Pulse.AppServices
{
    public interface IIndexProcessor
    {
        Task Process(Index index);
    }
}