using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZendeskApi_v2.Models.Search;
using ZendeskApi_v2.Models.Tickets;
using ZendeskTicketExporter.Core.Extensions;

namespace ZendeskTicketExporter.Core
{
    public class ZendeskApi2 : IZendeskApi
    {
        private readonly ZendeskApi_v2.ZendeskApi _api;

        public ZendeskApi2(string sitename, string username, string apiToken)
        {
            _api = new ZendeskApi_v2.ZendeskApi(
                Configuration.GetZendeskApiUri(sitename).AbsoluteUri,
                username,
                //password: password,
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

        public async Task<SearchResults> SearchFor(int? page)
        {
            if (page == null)
            {
                var response = await _api.Search.SearchForAsync("type:ticket%20status>=closed");
                return response;
            }
            else
            {
                var response =await _api.Search.SearchForAsync("type:ticket%20status>=closed", null, null, (int)page);
                return response;
            }
        }
    }
}