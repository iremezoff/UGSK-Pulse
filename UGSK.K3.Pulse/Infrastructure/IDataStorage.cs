using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UGSK.K3.Pulse.Infrastructure
{
    public interface IDataStorage
    {
        Task<IEnumerable<string>> GetProducts();
        Task<Counter> GetCounterOrDefault(string product, PeriodKind periodKind, DateTimeOffset periodStart, CounterKind counterKind);
        Task<Index> GetIndex(string product);
        Task<Counter> UpdateCounter(Counter counter, int delta);
        Task<Index> UpdateIndex(Index index);
    }
}