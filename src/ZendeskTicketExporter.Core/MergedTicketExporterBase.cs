using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ZendeskTicketExporter.Core
{
    /// <summary>
    /// Base class for ticket exporter which writes tickets to database
    /// </summary>
    /// <typeparam name="TTicketResult">Type of Ticket, currently can only by 'Ticket' or 'SearchResult'</typeparam>
    public abstract class MergedTicketExporterBase<TTicketResult> : IMergedTicketExporter<TTicketResult> where TTicketResult : new()
    {
        protected static readonly PropertyInfo[] TicketProperties = typeof(TTicketResult).GetProperties();
        protected readonly IDatabase _database;
        protected readonly string _tableName;

        protected MergedTicketExporterBase(IDatabase database, string tableName)
        {
            _database = database;
            _tableName = tableName;
        }

        public abstract Task WriteAsync(IList<TTicketResult> tickets);

        protected void DropTable()
        {
            _database.DropTable(_tableName);
        }
    }
}

