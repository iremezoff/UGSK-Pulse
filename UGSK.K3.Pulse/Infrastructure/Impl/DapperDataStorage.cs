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

        public async Task<Counter> GetCounter(string product, PeriodKind periodKind, DateTimeOffset periodStart, CounterKind counterKind)
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

        public async Task<Counter> UpdateCounter(Counter counter, int delta)
        {
            await _conn.ExecuteAsync(
                "begin tran update Counter set Value=Value+@delta, IsClosed=@isClosed where Product=@product and PeriodStart=@periodStart and PeriodKind=@periodKind and CounterKind=@counterKind " +
                "if @@rowcount = 0 " +
                "begin " +
                "insert Counter (Product, PeriodStart, PeriodKind, CounterKind, Value, IsClosed) values (@Product, @periodStart, @periodKind, @counterKind, 1, 0) " +
                "end " +
                "commit tran", new { counter.Product, counter.PeriodKind, periodStart = counter.PeriodStart.Date, counterKind = counter.Kind, delta, isClosed = counter.IsClosed });

            return await GetCounter(counter.Product, counter.PeriodKind, counter.PeriodStart.Date, counter.Kind);
        }

        public async Task<IEnumerable<Index>> GetIndexes()
        {
            var result = await _conn.QueryAsync<Index>("select * from [Index]");
            return result;
        }

        public async Task<Index> GetIndex(int id)
        {
            var index = (await
                _conn.QueryAsync<Index>(
                    "select * from [Index] where Id=@Id",
                    new { id })).Single();
            return index;
        }

        public async Task<Index> GetIndex(string product)
        {
            var index = (await
                _conn.QueryAsync<Index>(
                    "select top 1 * from [Index] where Product=@product",
                    new { product })).SingleOrDefault();
            return index ?? new Index { Product = product, Value = 100 };
        }

        public async Task DeleteIndex(int id)
        {
            await _conn.ExecuteAsync("delete from [Index] where Id=@Id", new { Id = id });
        }

        public async Task<Index> CreateOrUpdateIndex(Index index)
        {
            await _conn.ExecuteAsync(
                "BEGIN TRAN " +
                "UPDATE [Index] SET Product=@Product, Value=@Value, ActiveStart=@ActiveStart, IndexKind=@IndexKind "+
                "WHERE (Product=@Product AND ActiveStart=@ActiveStart AND IndexKind=@IndexKind) OR Id = @Id " +
                "IF @@ROWCOUNT = 0 " +
                "BEGIN " +
                "INSERT [Index] (Product, ActiveStart, IndexKind, Value) VALUES (@Product, @ActiveStart, @IndexKind, @Value) " +
                "END " +
                "COMMIT TRAN", new
                {
                    Id = index.Id,
                    Product = index.Product,
                    ActiveStart = index.ActiveStart,
                    IndexKind = index.IndexKind,
                    Value = index.Value
                });

            return await GetIndex(index.Product);
        }
    }
}