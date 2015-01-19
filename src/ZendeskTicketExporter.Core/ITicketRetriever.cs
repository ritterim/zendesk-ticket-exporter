using System.Collections.Generic;
using System.Threading.Tasks;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core
{
    public interface ITicketRetriever
    {
        Task<IEnumerable<Ticket>> GetAsync(IEnumerable<long> ids);

        Task<TicketExportResponse> GetBatchAsync(long? marker);
    }
}