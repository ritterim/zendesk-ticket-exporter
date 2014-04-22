using System;
using System.Threading.Tasks;

namespace ZendeskTicketExporter.Core
{
    public interface IWait
    {
        Task UntilAsync(DateTime waitUntil, DateTime? now = null);
    }
}