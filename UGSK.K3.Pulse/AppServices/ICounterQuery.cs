using System;
using System.Threading.Tasks;

namespace UGSK.K3.Pulse.AppServices
{
    public interface ICounterQuery
    {
        Task<Counter> GetCounter(string product, PeriodKind periodKind = PeriodKind.Daily,
            DateTimeOffset? periodStart = null,
            CounterKind kind = CounterKind.Total);
    }
}