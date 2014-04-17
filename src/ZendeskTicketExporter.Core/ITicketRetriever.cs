using System.Threading.Tasks;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core
{
    public interface ITicketRetriever
    {
        Task<TicketExportResponse> GetBatch(long? marker);
    }
}