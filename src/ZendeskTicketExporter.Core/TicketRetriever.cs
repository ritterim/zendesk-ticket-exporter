using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core
{
    public class TicketRetriever : ITicketRetriever
    {
        private readonly IWait _wait;
        private readonly Func<DateTime> _utcNowProvider;
        private readonly IZendeskApi _zendeskApi;

        private DateTime? _lastBatchRetrivedAt;

        public TicketRetriever(IWait wait, IZendeskApi zendeskApi, Func<DateTime> utcNowProvider = null)
        {
            _wait = wait;
            _zendeskApi = zendeskApi;
            _utcNowProvider = utcNowProvider ?? (() => DateTime.UtcNow);
        }

        public async Task<IEnumerable<Ticket>> GetAsync(IEnumerable<long> ids)
        {
            var tickets = await _zendeskApi.GetTicketsAsync(ids);
            return tickets;
        }

        public async Task<TicketExportResponse> GetBatchAsync(long? marker)
        {
            if (_lastBatchRetrivedAt.HasValue)
            {
                var nextAllowedRequest = _lastBatchRetrivedAt.Value.Add(
                    Configuration.ZendeskRequiredCooloffBetweenIncrementalTicketExportResults);

                await _wait.UntilAsync(nextAllowedRequest);
            }

            var ticketsBatch = await _zendeskApi.IncrementalTicketExport(marker);

            _lastBatchRetrivedAt = _utcNowProvider();

            return ticketsBatch;
        }
    }
}