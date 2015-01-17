using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZendeskTicketExporter.Core
{
    public interface IMergedTicketExporter<T> where T : new()
    {
        Task WriteAsync(IEnumerable<T> tickets);
          string TableName { get; }
    }
}