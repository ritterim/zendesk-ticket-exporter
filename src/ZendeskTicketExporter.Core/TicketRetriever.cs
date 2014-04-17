using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ZendeskApi_v2.Models.Tickets;
using ZendeskTicketExporter.Core.Extensions;

namespace ZendeskTicketExporter.Core
{
    public class TicketRetriever : ITicketRetriever
    {
        private readonly string _siteName;
        private readonly string _username;
        private readonly string _apiToken;

        public DateTime? _lastBatchRetrivedAt;

        public TicketRetriever(string siteName, string username, string apiToken)
        {
            _siteName = siteName;
            _username = username;
            _apiToken = apiToken;
        }

        public async Task<TicketExportResponse> GetBatch(long? marker)
        {
            if (_lastBatchRetrivedAt.HasValue)
            {
                var nextAllowedRequest = _lastBatchRetrivedAt.Value.Add(
                    Configuration.ZendeskRequiredCooloffBetweenIncrementalTicketExportResults);

                await WaitUntil(nextAllowedRequest);
            }

            var ticketsBatch = await GetTicketsBatch(marker);

            _lastBatchRetrivedAt = DateTime.UtcNow;

            return ticketsBatch;
        }

        private async Task<TicketExportResponse> GetTicketsBatch(long? marker)
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

            var handler = new HttpClientHandler()
            {
                Credentials = new NetworkCredential(_username + "/token", _apiToken)
            };

            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = Configuration.GetZendeskApiUri(_siteName);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var url = Configuration.GetZendeskIncrementalTicketExportUrl(marker.Value);
                var response = await client.GetAsync(url);

                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(
                        "A non-success status code was returned by Zendesk: " + ex.Message,
                        ex);
                }
#if DEBUG
                var stringResponse = await response.Content.ReadAsStringAsync();
#endif
                var ticketsBatch = await response.Content.ReadAsAsync<TicketExportResponse>();
                return ticketsBatch;
            }
        }

        private static async Task WaitUntil(DateTime waitUntil)
        {
            var timespanToWait = waitUntil.Subtract(DateTime.UtcNow);
            if (timespanToWait.Ticks > 0)
                await Task.Delay(timespanToWait);
        }
    }
}