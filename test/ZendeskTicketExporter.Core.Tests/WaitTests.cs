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
            var now = DateTime.UtcNow;

            await _sut.UntilAsync(now.Subtract(TimeSpan.FromMinutes(1)), now);

            Mock.Get(_log).Verify(
                x => x.InfoFormat(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [Fact]
        public async Task UntilAsync_waits_for_future_datetime()
        {
            var now = DateTime.UtcNow;

            await _sut.UntilAsync(now.Add(TimeSpan.FromMilliseconds(1)), now);

            Mock.Get(_log).Verify(
                x => x.InfoFormat(It.IsAny<string>(), It.IsAny<string>()),
                Times.Once());
        }
    }
}