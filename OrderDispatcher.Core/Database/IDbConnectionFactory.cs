using System.Data;

namespace OrderDispatcher.Core.Database
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}