using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace UGSK.K3.Pulse.Infrastructure.Impl
{
    class DapperDataStorage : IDataStorage
    {
        private readonly IDbConnection _conn;
        public DapperDataStorage()
        {
            _conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Pulse"].ConnectionString);
        }

        public async Task<IEnumerable<string>> GetProducts()
        {
            return (await _conn.QueryAsync<Index>("select distinct Product from [Index]")).Select(p => p.Product);
        }

        public async Task<Counter> GetCounterOrDefault(string product, PeriodKind periodKind, DateTimeOffset periodStart, CounterKind counterKind)
        {
            var counter = (await
                _conn.QueryAsync<Counter>(
                    "select top 1 * from Counter where Product=@product and PeriodStart=@periodStart and PeriodKind=@periodKind and CounterKind=@counterKind",
                    new { product, periodKind, periodStart = periodStart.Date, counterKind })).SingleOrDefault();

            return counter ?? new Counter
            {
                Product = product,
                Kind = counterKind,
                PeriodStart = periodStart,
                PeriodKind = periodKind
            };
        }

        public async Task<Index> GetIndex(string product)
        {
            var index = (await
                _conn.QueryAsync<Index>(
                    "select top 1 * from [Index] where Product=@Product",
                    new { product })).SingleOrDefault();
            return index ?? new Index { Product = product, Value = 0 };
        }

        public async Task<Counter> UpdateCounter(Counter counter, int delta)
        {
            await _conn.ExecuteAsync(
                "begin tran update Counter set Value=Value+@delta, IsClosed=@isClosed, PeriodActualDate=@periodActualDate where Product=@product and PeriodStart=@periodStart and PeriodKind=@periodKind and CounterKind=@counterKind " +
                "if @@rowcount = 0 " +
                "begin " +
                "insert Counter (Product, PeriodStart, PeriodActualDate, PeriodKind, CounterKind, Value, IsClosed) values (@Product, @periodStart, @periodStart, @periodKind, @counterKind, 1, 0) " +
                "end " +
                "commit tran",
                new
                {
                    counter.Product,
                    counter.PeriodKind,
                    periodStart = counter.PeriodStart.Date,
                    periodActualDate = counter.PeriodActualDate.HasValue ? counter.PeriodActualDate.Value.Date : (DateTimeOffset?)null,
                    counterKind = counter.Kind,
                    delta,
                    isClosed = counter.IsClosed
                });

            return await GetCounterOrDefault(counter.Product, counter.PeriodKind, counter.PeriodStart.Date, counter.Kind);
        }

        public async Task<Index> UpdateIndex(Index index)
        {
            await _conn.ExecuteAsync(
                "begin tran " +
                "update [Index] set Value=@Value where Product=@Product and ActiveStart=@ActiveStart and IndexKind=@IndexKind" +
                "if @@rowcount = 0 " +
                "begin " +
                "insert [Index] (Product, ActiveStart, IndexKind, Value) values (@Product, @ActiveStart, @IndexKind, @Value) " +
                "end " +
                "commit tran", new { index.Product, index.ActiveStart, index.IndexKind, index.Value });

            return await GetIndex(index.Product);
        }
    }
}