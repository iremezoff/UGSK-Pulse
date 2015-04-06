using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UGSK.K3.Pulse.Infrastructure
{
    public interface IDataStorage
    {
        Task<IEnumerable<string>> GetProducts();
        Task<Counter> GetCounter(string product, PeriodKind periodKind, DateTimeOffset periodStart, CounterKind counterKind);

        Task<IEnumerable<Index>> GetIndexes();
        Task<Index> GetIndex(int id);
        Task<Index> GetIndex(string product);
        Task<Counter> UpdateCounter(Counter counter, int delta);
        Task<Index> CreateOrUpdateIndex(Index index);
        Task<Index> UpdateIndex(Index index);
        Task DeleteIndex(int id);
    }
}