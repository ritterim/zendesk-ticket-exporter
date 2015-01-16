using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using LiteGuard;
using ZendeskApi_v2.Models.Shared;
using ZendeskApi_v2.Models.Tickets;
using Result = ZendeskApi_v2.Models.Search.Result;

namespace ZendeskTicketExporter.Core
{
    /// <summary>
    /// This strategy avoids the problem over encountering a situtaion where one row may have more columns than the rest,
    /// e.g. if CollaboratorIds is empty in the first row, and then has values in the secnod, we would ned to dynamically generate columns
    /// Arrays in columns are packed into a single column and pip delimited
    /// </summary>
    public class SinglePropertyPerColumnExporter : MergedTicketExporterBase<Result>
    {
        private const string Delimiter = "|";

        public SinglePropertyPerColumnExporter(IDatabase database, string tableName)
            : base(database, tableName)
        {
            // since potentially different schemas could be used for the same table name, we nuke the table each time we use it
            DropTable();
        }

        public override async Task WriteAsync(IList<Result> tickets)
        {
            Guard.AgainstNullArgument("tickets", tickets);
            foreach (var ticket in tickets)
            {
                var insertParams = new DynamicParameters();
                foreach (var property in TicketProperties)
                {
                    var value = property.GetValue(ticket);

                    //Handle 'CustomFields'
                    var customFields = value as IList<CustomField>;
                    if (customFields != null)
                    {
                        insertParams.Add(property.Name, string.Join(Delimiter, customFields.Select(x => x.Value).ToArray()));
                        continue;
                    }

                    //Handle 'Attachments'
                    var attachments = value as IList<Attachment>;
                    if (attachments != null)
                    {
                        var s = attachments.Aggregate("", (current, a) => current + GetAttachmentValue(a));
                        insertParams.Add("Attachments", s);
                        continue;
                    }

                    //Handle 'Tags' property
                    var list = value as IList<string>;
                    if (list != null)
                    {
                        insertParams.Add(property.Name, string.Join(Delimiter, list));
                        continue;
                    }

                    //Handle 'Via' property
                    var via = value as Via;
                    if (via != null)
                    {
                        insertParams.Add("Via", string.Format("Channel:{0}{1}SourceFromName:{2}{1}SourceFromAddress:{3}{1}SourceToAddress:{4}{1}SourceToName:{5}", via.Channel, Delimiter, via.Source.From.Name, via.Source.From.Address, via.Source.To.Address, via.Source.To.Name));
                        continue;
                    }

                    //handle remaining properties
                    insertParams.Add(property.Name, value);
                }

                await
                    _database.ExecuteAsync(string.Format("create table if not exists {0} ({1}, primary key (Id));",
                        _tableName, string.Join(", ", insertParams.ParameterNames)));

                await _database.ExecuteAsync(
                    string.Format("insert or replace into {0} ({1}) values ({2})",
                        _tableName,
                        string.Join(", ", insertParams.ParameterNames),
                        string.Join(", ", insertParams.ParameterNames.Select(x => "@" + x))),
                    insertParams);
            }
        }

        private static string GetAttachmentValue(Attachment attachment)
        {
            return string.Format("Id:{0}{1}ContentType:{2}{1}ContentUrl:{3}{1}FileName:{4}{1}Size:{5}{1}", attachment.Id, Delimiter, attachment.ContentType, attachment.ContentUrl, attachment.FileName, attachment.Size) ;// disregard thumbnails
        }
    }
}