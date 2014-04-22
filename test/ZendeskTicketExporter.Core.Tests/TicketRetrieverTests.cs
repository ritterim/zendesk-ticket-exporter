using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ZendeskTicketExporter.Core.Tests
{
    public class TicketRetrieverTests
    {
        private readonly IWait _wait;
        private readonly IZendeskApi _zendeskApi;
        private readonly ITicketRetriever _sut;

        public TicketRetrieverTests()
        {
            _wait = Mock.Of<IWait>();
            _zendeskApi = Mock.Of<IZendeskApi>();
            _sut = new TicketRetriever(_wait, _zendeskApi);
        }

        [Fact]
        public async Task GetBatchAsync_calls_ZendeskApi_IncrementalTicketExport_using_marker()
        {
            await _sut.GetBatchAsync(marker: 123);

            Mock.Get(_zendeskApi).Verify(
                x => x.IncrementalTicketExport(123),
                Times.Once());
        }

        [Fact]
        public async Task GetBatchAsync_does_not_wait_for_first_call()
        {
            await _sut.GetBatchAsync(marker: 123);

            Mock.Get(_wait).Verify(
                x => x.UntilAsync(It.IsAny<DateTime>(), /* now */ null),
                Times.Never());
        }

        [Fact]
        public async Task GetBatchAsync_waits_for_second_call()
        {
            await _sut.GetBatchAsync(marker: 123);

            await _sut.GetBatchAsync(marker: 456);

            Mock.Get(_wait).Verify(
                x => x.UntilAsync(IsWithinMinutesTolerance(DateTime.UtcNow, 5), /* now */ null),
                Times.Once());
        }

        private static DateTime IsWithinMinutesTolerance(DateTime dateTime, int minutes)
        {
            return It.IsInRange<DateTime>(
                dateTime.AddMinutes(-minutes),
                dateTime.AddMinutes(minutes),
                Range.Inclusive);
        }
    }
}