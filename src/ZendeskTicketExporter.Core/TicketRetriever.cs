using System;
using System.Threading.Tasks;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core
{
    public class TicketRetriever : ITicketRetriever
    {
        private readonly IWait _wait;
        private readonly IZendeskApi _zendeskApi;

        private DateTime? _lastBatchRetrivedAt;

        public TicketRetriever(IWait wait, IZendeskApi zendeskApi)
        {
            _wait = wait;
            _zendeskApi = zendeskApi;
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

            _lastBatchRetrivedAt = DateTime.UtcNow;

            return ticketsBatch;
        }
    }
}