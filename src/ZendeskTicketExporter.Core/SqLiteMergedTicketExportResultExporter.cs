using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using LiteGuard;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core
{
    /// <summary>
    /// TicketExportResult Does not contain complex object properties or IList properties so this original implementation
    /// works
    /// </summary>
    public class SqLiteMergedTicketExportResultExporter : MergedTicketExporterBase<TicketExportResult>
    {
        public SqLiteMergedTicketExportResultExporter(IDatabase database, string tableName)
            : base(database, tableName)
        {
        }

        public override async Task WriteAsync(IList<TicketExportResult> tickets)
        {
            Guard.AgainstNullArgument("tickets", tickets);

            await _database.ExecuteAsync(string.Format(
                "create table if not exists {0} ({1}, primary key (Id));",
                _tableName,
                string.Join(", ", TicketProperties.Select(x => x.Name))));

            foreach (var ticket in tickets)
            {
                var insertParams = new DynamicParameters();
                foreach (var property in TicketProperties)
                    insertParams.Add(property.Name, property.GetValue(ticket));

                await _database.ExecuteAsync(
                    string.Format("insert or replace into {0} ({1}) values ({2})",
                        _tableName,
                        string.Join(", ", TicketProperties.Select(x => x.Name)),
                        string.Join(", ", TicketProperties.Select(x => "@" + x.Name))),
                    insertParams);
            }
        }
    }
}