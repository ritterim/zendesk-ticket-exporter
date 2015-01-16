using System.Threading.Tasks;
using ZendeskApi_v2.Models.Search;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core
{
    public interface IZendeskApi
    {
        Task<TicketExportResponse> IncrementalTicketExport(long? marker);
        Task<SearchResults> SearchFor(int page);
    }
}