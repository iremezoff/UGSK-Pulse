using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

namespace UGSK.K3.Pulse.Infrastructure.Impl
{
    class DefaultLogger : ILogger
    {
        private readonly IDbConnection _conn;

        public DefaultLogger()
        {
            _conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Pulse"].ConnectionString);
        }

        public async Task Write(SaleSystemNotification notification)
        {
            await
                _conn.ExecuteAsync(
                    "insert into Hit (Product, Filial, ContractSigningDate, Increment) values (@product, @filial, @signingdate, @increment)",
                    new
                    {
                        product = notification.Product,
                        filial = notification.Filial,
                        signingdate = notification.ContractSigningDateTime,
                        increment = notification.Increment
                    });
        }
    }
}