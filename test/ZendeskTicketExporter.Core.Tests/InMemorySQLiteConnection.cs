using System.Data;
using System.Data.SQLite;

namespace ZendeskTicketExporter.Core.Tests
{
    public class InMemorySQLiteConnection : IDbConnection
    {
        private static readonly string InMemoryConnectionString = "Data Source=:memory:;Version=3;New=True;";

        private readonly SQLiteConnection _conn;

        public InMemorySQLiteConnection()
        {
            _conn = new SQLiteConnection(InMemoryConnectionString);
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return _conn.BeginTransaction(il);
        }

        public IDbTransaction BeginTransaction()
        {
            return _conn.BeginTransaction();
        }

        public void ChangeDatabase(string databaseName)
        {
            ChangeDatabase(databaseName);
        }

        public void Close()
        {
        }

        public string ConnectionString
        {
            get { return InMemoryConnectionString; }
            set { }
        }

        public int ConnectionTimeout
        {
            get { return 60; }
        }

        public IDbCommand CreateCommand()
        {
            return _conn.CreateCommand();
        }

        public string Database
        {
            get { return ":memory:"; }
        }

        public void Open()
        {
            if (_conn.State != ConnectionState.Open)
                _conn.Open();
        }

        public ConnectionState State
        {
            get { return _conn.State; }
        }

        public void Dispose()
        {
        }
    }
}