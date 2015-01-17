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
        public string TableName
        {
            get { return (typeof (TTicketResult).Name); }
        }

        protected MergedTicketExporterBase(IDatabase database)
        {
            _database = database;
        }

        public abstract Task WriteAsync(IEnumerable<TTicketResult> tickets);

        protected void DropTable()
        {
            _database.DropTable(typeof(TTicketResult).Name);
        }
    }
}

