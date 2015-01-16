using System;
using System.Threading.Tasks;
using ZendeskApi_v2.Models.Search;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core
{
    public class TicketRetriever : ITicketRetriever
    {
        private readonly IWait _wait;
        private readonly Func<DateTime> _utcNowProvider;
        private readonly IZendeskApi _zendeskApi;

        private DateTime? _lastBatchRetrievedAt;

        public TicketRetriever(IWait wait, IZendeskApi zendeskApi, Func<DateTime> utcNowProvider = null)
        {
            _wait = wait;
            _zendeskApi = zendeskApi;
            _utcNowProvider = utcNowProvider ?? (() => DateTime.UtcNow);
        }

        public async Task<TicketExportResponse> GetBatchAsync(long? marker)
        {
            if (_lastBatchRetrievedAt.HasValue)
            {
                var nextAllowedRequest = _lastBatchRetrievedAt.Value.Add(
                    Configuration.ZendeskRequiredCooloffBetweenIncrementalTicketExportResults);

                await _wait.UntilAsync(nextAllowedRequest);
            }

            var ticketsBatch = await _zendeskApi.IncrementalTicketExport(marker);

            _lastBatchRetrievedAt = _utcNowProvider();

            return ticketsBatch;
        }

        public async Task<SearchResults> SearchFor(int page)
        {
            var ticketsBatch = await _zendeskApi.SearchFor(page);
            return ticketsBatch;
        }
    }
}