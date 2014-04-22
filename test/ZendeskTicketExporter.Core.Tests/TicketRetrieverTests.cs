using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ZendeskTicketExporter.Core.Tests
{
    public class TicketRetrieverTests
    {
        private static readonly DateTime Jan1_2014 = new DateTime(2014, 1, 1);

        private readonly IWait _wait;
        private readonly IZendeskApi _zendeskApi;
        private readonly Func<DateTime> _utcNowProvider;
        private readonly ITicketRetriever _sut;

        public TicketRetrieverTests()
        {
            _wait = Mock.Of<IWait>();
            _zendeskApi = Mock.Of<IZendeskApi>();
            _utcNowProvider = Mock.Of<Func<DateTime>>();
            _sut = new TicketRetriever(_wait, _zendeskApi, _utcNowProvider);

            SetupDefaultMocks();
        }

        private void SetupDefaultMocks()
        {
            Mock.Get(_utcNowProvider).Setup(x => x()).Returns(Jan1_2014);
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
        public async Task GetBatchAsync_waits_correct_duration_for_second_call()
        {
            await _sut.GetBatchAsync(marker: 123);

            await _sut.GetBatchAsync(marker: 456);

            Mock.Get(_wait).Verify(
                x => x.UntilAsync(Jan1_2014.Add(Configuration.ZendeskRequiredCooloffBetweenIncrementalTicketExportResults), /* now */ null),
                Times.Once());
        }
    }
}