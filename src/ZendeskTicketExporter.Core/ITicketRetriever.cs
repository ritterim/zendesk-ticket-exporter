using System.Threading.Tasks;
using ZendeskApi_v2.Models.Search;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core
{
    public interface ITicketRetriever
    {
        Task<TicketExportResponse> GetBatchAsync(long? marker);
        Task<SearchResults> SearchFor(int page);
    }
}