using System;

namespace ZendeskTicketExporter.Core
{
    public static class Configuration
    {
        public static readonly string MarkersTableName = "markers";

        public static readonly string MarkersTableColumnName = "MarkerValue";

        public static readonly string TicketsTableName = "tickets";

        // "You are only allowed to make 1 API call to this API end point every minute and we will return up to 1000 tickets per request."
        // http://developer.zendesk.com/documentation/rest_api/ticket_export.html on 4/9/2014
        public static readonly TimeSpan ZendeskRequiredCooloffBetweenIncrementalTicketExportResults = TimeSpan.FromMinutes(1);

        public static readonly int ZendeskMaxItemsReturnedFromTicketExportApi = 1000;

        // "Requests with start_time less than 5 minutes old will be rejected."
        // http://developer.zendesk.com/documentation/rest_api/ticket_export.html on 4/9/2014
        public static readonly TimeSpan ZendeskMinimumRequiredHistoricalMinutes = TimeSpan.FromMinutes(5);

        public static Uri GetZendeskApiUri(string siteName)
        {
            var uri = new Uri(string.Format("https://{0}.zendesk.com/api/v2/", Uri.EscapeUriString(siteName)));
            return uri;
        }

        public static string GetZendeskIncrementalTicketExportUrl(long marker)
        {
            var url = "exports/tickets.json?start_time=" + marker;
            return url;
        }
    }
}