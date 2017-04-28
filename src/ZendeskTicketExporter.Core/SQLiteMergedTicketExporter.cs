using Dapper;
using LiteGuard;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core
{
    public class SQLiteMergedTicketExporter : IMergedTicketExporter
    {
        private static readonly PropertyInfo[] FlattenedTicketProperties = typeof(FlattenedTicket).GetProperties();
        private static readonly PropertyInfo[] TicketExportProperties = typeof(TicketExportResult).GetProperties();

        private readonly IDatabase _database;

        public SQLiteMergedTicketExporter(IDatabase database)
        {
            _database = database;
        }

        public async Task WriteAsync(IEnumerable<FlattenedTicket> tickets)
        {
            Guard.AgainstNullArgument("tickets", tickets);

            if (tickets.Any() == false)
                return;

            await CreateTableIfNotExists(Configuration.TicketsTableName, FlattenedTicketProperties);
            await InsertOrReplaceInto(Configuration.TicketsTableName, tickets, FlattenedTicketProperties);
        }

        public async Task WriteAsync(IEnumerable<TicketExportResult> tickets)
        {
            Guard.AgainstNullArgument("tickets", tickets);

            if (tickets.Any() == false)
                return;

            await CreateTableIfNotExists(Configuration.TicketsExportTableName, TicketExportProperties);
            await InsertOrReplaceInto(Configuration.TicketsExportTableName, tickets, TicketExportProperties);
        }

        private async Task CreateTableIfNotExists(string tableName, IEnumerable<PropertyInfo> properties)
        {
            await _database.ExecuteAsync(string.Format(
                "create table if not exists {0} ({1}, primary key (Id));",
                tableName,
                string.Join(", ", properties.Select(x => x.Name))));
        }

        private async Task InsertOrReplaceInto<T>(string tableName, IEnumerable<T> items, IEnumerable<PropertyInfo> properties)
        {
            foreach (var item in items)
            {
                var insertParams = new DynamicParameters();
                foreach (var property in properties)
                    insertParams.Add(property.Name, property.GetValue(item));

                await _database.ExecuteAsync(
                    string.Format("insert or replace into {0} ({1}) values ({2})",
                        tableName,
                        string.Join(", ", properties.Select(x => x.Name)),
                        string.Join(", ", properties.Select(x => "@" + x.Name))),
                    insertParams);
            }
        }
    }
}