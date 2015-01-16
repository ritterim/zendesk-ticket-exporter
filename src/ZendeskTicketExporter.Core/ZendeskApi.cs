using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZendeskApi_v2.Models.Search;
using ZendeskApi_v2.Models.Tickets;
using ZendeskTicketExporter.Core.Extensions;

namespace ZendeskTicketExporter.Core
{
    public class ZendeskApi : IZendeskApi
    {
        private readonly ZendeskApi_v2.ZendeskApi _api;

        public ZendeskApi(string sitename, string username, string apiToken, string password = "")
        {
            _api = new ZendeskApi_v2.ZendeskApi(
                Configuration.GetZendeskApiUri(sitename).AbsoluteUri,
                username,
                password: password,
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
                return new TicketExportResponse
                {
                    EndTime = marker.Value,
                    Results = new List<TicketExportResult>(),
                };
            }

            var response = await _api.Tickets.GetInrementalTicketExportAsync(marker.Value.FromUnixTime());
            return response;
        }

        public async Task<SearchResults> SearchFor(int page)
        {
            // Returns all tickets including those are closed. Allowing for a full export. This is the crucial bit as the current website portal doesn't allow for this.
            const string searchQuery = "type:ticket%20status>=closed";
            if (page == 0)
            {
                return await _api.Search.SearchForAsync(searchQuery);
            }
            return await _api.Search.SearchForAsync(searchQuery, null, null, page);
        }
    }
}