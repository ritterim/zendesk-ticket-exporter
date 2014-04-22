using System.Data;
using System.Threading.Tasks;

namespace ZendeskTicketExporter.Core.Tests
{
    public class InMemoryDatabase : Database
    {
        private readonly IDbConnection _conn;

        public InMemoryDatabase()
            : base(dbFile: null)
        {
            _conn = new InMemorySQLiteConnection();
        }

        protected override async Task<IDbConnection> GetOpenConnectionAsync()
        {
            if (_conn.State != ConnectionState.Open)
                _conn.Open();

            return await Task<IDbConnection>.Run(() => _conn);
        }
    }
}