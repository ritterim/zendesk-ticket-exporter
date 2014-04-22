using Common.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ZendeskTicketExporter.Core.Tests
{
    public class WaitTests
    {
        private readonly ILog _log;
        private readonly IWait _sut;

        public WaitTests()
        {
            _log = Mock.Of<ILog>();
            _sut = new Wait(_log);
        }

        [Fact]
        public async Task UntilAsync_does_not_wait_for_past_datetime()
        {
            var utcNow = DateTime.UtcNow;

            await VerifyWaitOccurredAsync(
                utcNow.Subtract(TimeSpan.FromMinutes(1)),
                utcNow,
                Times.Never());
        }

        [Fact]
        public async Task UntilAsync_waits_for_future_datetime()
        {
            var utcNow = DateTime.UtcNow;

            await VerifyWaitOccurredAsync(
                utcNow.Add(TimeSpan.FromMilliseconds(1)),
                utcNow,
                Times.Once());
        }

        [Fact]
        public async Task UntilAsync_handles_local_vs_utc_properly()
        {
            var utcNow = DateTime.UtcNow;
            var easternNow = GetEasternStandardTime(utcNow);

            await VerifyWaitOccurredAsync(
                easternNow.Add(TimeSpan.FromMilliseconds(1)),
                utcNow,
                Times.Once());
        }

        [Fact]
        public async Task UntilAsync_handles_utc_vs_local_properly()
        {
            var utcNow = DateTime.UtcNow;
            var easternNow = GetEasternStandardTime(utcNow);

            await VerifyWaitOccurredAsync(
                utcNow.Add(TimeSpan.FromMilliseconds(1)),
                easternNow,
                Times.Once());
        }

        private static DateTime GetEasternStandardTime(DateTime timeUtc)
        {
            // http://stackoverflow.com/a/5997619/941536
            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);

            return easternTime;
        }

        private async Task VerifyWaitOccurredAsync(DateTime waitUntil, DateTime now, Times expectedTimes)
        {
            await _sut.UntilAsync(waitUntil, now);

            Mock.Get(_log).Verify(
                x => x.InfoFormat(It.IsAny<string>(), It.IsAny<string>()),
                expectedTimes);
        }
    }
}