using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OrderDispatcher.Core.Constants;
using System.Data;

namespace OrderDispatcher.Core.Database
{
    public class WarehouseDbConnectionFactory : IWarehouseDbConnectionFactory
    {
        private readonly string _connectionString;

        public WarehouseDbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString(ConfigConstants.WarehouseDb)
                ?? throw new InvalidOperationException("WarehouseDb connection string is missing.");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}