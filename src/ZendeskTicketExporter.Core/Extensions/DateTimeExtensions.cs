using System;

namespace ZendeskTicketExporter.Core.Extensions
{
    public static class DateTimeExtensions
    {
        // http://stackoverflow.com/a/7844741/941536
        public static DateTime FromUnixTime(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }
    }
}