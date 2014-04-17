using System.Threading.Tasks;

namespace ZendeskTicketExporter.Core
{
    public interface IMarkerStorage
    {
        Task<long?> GetCurrentMarker();

        Task UpdateCurrentMarker(long marker);
    }
}