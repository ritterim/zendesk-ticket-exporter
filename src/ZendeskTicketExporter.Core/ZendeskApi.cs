using Common.Logging;
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
    public class ZendeskApi : IZendeskApi
    {
        private readonly string _sitename;
        private readonly string _username;
        private readonly string _apiToken;
        private readonly ILog _log;

        public ZendeskApi(string sitename, string username, string apiToken, ILog log)
        {
            _sitename = sitename;
            _username = username;
            _apiToken = apiToken;
            _log = log;
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

            var handler = new HttpClientHandler()
            {
                Credentials = new NetworkCredential(_username + "/token", _apiToken)
            };

            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = Configuration.GetZendeskApiUri(_sitename);
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
    }
}