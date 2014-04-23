using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZendeskApi_v2.Models.Tickets;
using ZendeskTicketExporter.Core.Extensions;

namespace ZendeskTicketExporter.Core
{
    public class ZendeskApi : IZendeskApi
    {
        private readonly ZendeskApi_v2.ZendeskApi _api;

        public ZendeskApi(string sitename, string username, string apiToken)
        {
            _api = new ZendeskApi_v2.ZendeskApi(
                Configuration.GetZendeskApiUri(sitename).AbsoluteUri,
                username,
                password: "",
                apiToken: apiToken);
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