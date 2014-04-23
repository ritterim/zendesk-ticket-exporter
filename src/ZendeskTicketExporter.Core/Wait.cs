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
            var utcNow = now == null ? DateTime.UtcNow : now.Value.ToUniversalTime();

            var timespanToWait = waitUntil.ToUniversalTime().Subtract(utcNow);
            if (timespanToWait.Ticks > 0)
            {
                _log.InfoFormat(
                    "Waiting until {0}.",
                    waitUntil.ToLocalTime().ToString());

                await Task.Delay(timespanToWait);
            }
        }
    }
}