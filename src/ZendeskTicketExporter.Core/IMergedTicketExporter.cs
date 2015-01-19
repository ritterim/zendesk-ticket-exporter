using System.Collections.Generic;
using System.Threading.Tasks;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core
{
    public interface IMergedTicketExporter
    {
        Task WriteAsync(IEnumerable<FlattenedTicket> tickets);

        Task WriteAsync(IEnumerable<TicketExportResult> tickets);
    }
}