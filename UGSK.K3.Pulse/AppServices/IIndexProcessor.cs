using System.Threading.Tasks;

namespace UGSK.K3.Pulse
{
    public interface IIndexProcessor
    {
        Task Process(Index index);
    }
}