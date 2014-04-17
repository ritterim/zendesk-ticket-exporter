using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZendeskTicketExporter.Core
{
    public interface IDatabase
    {
        Task ExecuteAsync(string sql, object param = null);

        Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null);

        Task<T> QueryScalerAsync<T>(string sql, object param = null);

        Task<bool> TableExistsAsync(string tableName);
    }
}