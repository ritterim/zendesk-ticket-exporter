using Common.Logging;
using System;
using System.Threading.Tasks;

namespace ZendeskTicketExporter.Core
{
    public class Wait : IWait
    {
        private readonly ILog _log;

        public Wait(ILog log)
        {
            _log = log;
        }

        public async Task UntilAsync(DateTime waitUntil, DateTime? now = null)
        {
            var timespanToWait = waitUntil.Subtract(now ?? DateTime.UtcNow);
            if (timespanToWait.Ticks > 0)
            {
                _log.InfoFormat(
                    "Waiting until {0}",
                    waitUntil.ToLocalTime().ToString());

                await Task.Delay(timespanToWait);
            }
        }
    }
}