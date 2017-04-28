using System.Collections.Generic;
using System.Threading.Tasks;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core
{
    public interface IZendeskApi
    {
        Task<IEnumerable<TicketField>> GetTicketFieldsAsync();

        Task<IEnumerable<Ticket>> GetTicketsAsync(IEnumerable<long> ids);

        Task<TicketExportResponse> IncrementalTicketExport(long? marker);
    }
}