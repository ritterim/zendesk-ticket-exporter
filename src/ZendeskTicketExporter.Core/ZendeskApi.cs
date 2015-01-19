using Common.Logging;
using LiteGuard;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZendeskApi_v2.Models.Tickets;
using ZendeskTicketExporter.Core.Extensions;

namespace ZendeskTicketExporter.Core
{
    public class ZendeskApi : IZendeskApi
    {
        private readonly ZendeskApi_v2.ZendeskApi _api;
        private readonly ILog _log;

        public ZendeskApi(string sitename, string username, string apiToken, ILog log)
        {
            _api = new ZendeskApi_v2.ZendeskApi(
                Configuration.GetZendeskApiUri(sitename).AbsoluteUri,
                username,
                password: "",
                apiToken: apiToken);
            _log = log;
        }

        public async Task<IEnumerable<TicketField>> GetTicketFieldsAsync()
        {
            var ticketFields = await _api.Tickets.GetTicketFieldsAsync();
            return ticketFields.TicketFields;
        }

        public async Task<IEnumerable<Ticket>> GetTicketsAsync(IEnumerable<long> ids)
        {
            Guard.AgainstNullArgument("ids", ids);

            // TODO: TPL equivalent of yield return to avoid storing all objects in memory, if possible
            var results = new List<Ticket>();

            // GetMultipleTicketsAsync uses an HTTP GET which could be problematic if too many
            // tickets are requested in one roundtrip because of the url length.
            // To avoid this, request tickets in batches.
            var batchSize = 100;
            foreach (var batch in ids.Batch(batchSize))
            {
                _log.InfoFormat(
                    "Starting GetTickets batch including ticket ids {0}.",
                    string.Join(", ", batch));

                // TODO: Respect the api speed limits and wait/retry when necessary

                var batchResult = await _api.Tickets.GetMultipleTicketsAsync(batch.ToList());
                results.AddRange(batchResult.Tickets);
            }

            return results;
        }

        public async Task<TicketExportResponse> IncrementalTicketExport(long? marker)
        {
            if (marker == null)
            {
                marker = 0;
            }
            else if (marker.Value.FromUnixTime() >= DateTime.UtcNow.Subtract(Configuration.ZendeskMinimumRequiredHistoricalMinutes))
            {
                return new TicketExportResponse()
                {
                    EndTime = marker.Value,
                    Results = new List<TicketExportResult>(),
                };
            }

            var response = await _api.Tickets.GetInrementalTicketExportAsync(marker.Value.FromUnixTime());
            return response;
        }
    }
}