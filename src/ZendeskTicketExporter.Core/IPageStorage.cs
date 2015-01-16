using System.Threading.Tasks;

namespace ZendeskTicketExporter.Core
{
    public interface IPageStorage
    {
        Task<string> GetCurrentPage();

        Task UpdateCurrentPage(string page);
    }
}