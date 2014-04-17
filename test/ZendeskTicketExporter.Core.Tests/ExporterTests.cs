using AssertExLib;
using Common.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core.Tests
{
    public class ExporterTests
    {
        private readonly ILog _log;
        private readonly IDatabase _database;
        private readonly IMarkerStorage _markerStorage;
        private readonly ITicketRetriever _ticketRetriever;
        private readonly IMergedTicketExporter _mergeExporter;
        private readonly ICsvFileWriter _csvFileWriter;
        private readonly Exporter _sut;

        public ExporterTests()
        {
            _log = Mock.Of<ILog>();
            _database = Mock.Of<IDatabase>();
            _markerStorage = Mock.Of<IMarkerStorage>();
            _ticketRetriever = Mock.Of<ITicketRetriever>();
            _mergeExporter = Mock.Of<IMergedTicketExporter>();
            _csvFileWriter = Mock.Of<ICsvFileWriter>();

            _sut = new Exporter(_log, _database, _markerStorage, _ticketRetriever, _mergeExporter, _csvFileWriter);

            SetupDefaultMocks();
        }

        /// <remarks>
        /// First pass is 1 ticket with EndTime of 123,
        /// second request should be for marker 123 which results no results
        /// </remarks>
        private void SetupDefaultMocks()
        {
            Mock.Get(_markerStorage).Setup(x => x.GetCurrentMarker()).ReturnsAsync(null);

            Mock.Get(_ticketRetriever).Setup(x => x.GetBatch(null)).ReturnsAsync(new TicketExportResponse()
            {
                EndTime = 123,
                Results = new List<TicketExportResult>()
                {
                    new TicketExportResult()
                },
            });

            Mock.Get(_ticketRetriever).Setup(x => x.GetBatch(123)).ReturnsAsync(new TicketExportResponse()
            {
                EndTime = 456,
                Results = new List<TicketExportResult>(),
            });
        }

        [Fact]
        public void RefreshLocalCopyFromServer_should_not_allow_newDatabase_true_with_existing_marker()
        {
            Mock.Get(_markerStorage).Setup(x => x.GetCurrentMarker()).ReturnsAsync(123);

            var exception = AssertEx.TaskThrows<InvalidOperationException>(
                () => _sut.RefreshLocalCopyFromServer(newDatabase: true));

            Assert.Equal("Cannot specify newDatabase 'true' when updating from an existing marker.", exception.Message);
        }

        [Fact]
        public void RefreshLocalCopyFromServer_should_not_allow_newDatabase_false_with_no_existing_marker()
        {
            var exception = AssertEx.TaskThrows<InvalidOperationException>(
                () => _sut.RefreshLocalCopyFromServer(newDatabase: false));

            Assert.Equal("marker must have a value when not creating a new database.", exception.Message);
        }

        [Fact]
        public async Task RefreshLocalCopyFromServer_should_use_marker_when_calling_GetBatch()
        {
            Mock.Get(_markerStorage).Setup(x => x.GetCurrentMarker()).ReturnsAsync(123);

            await _sut.RefreshLocalCopyFromServer(newDatabase: false);

            Mock.Get(_ticketRetriever).Verify(
                x => x.GetBatch(123),
                Times.Once());
        }

        [Fact]
        public async Task RefreshLocalCopyFromServer_should_update_marker_from_GetBatch_result()
        {
            await _sut.RefreshLocalCopyFromServer(newDatabase: true);

            Mock.Get(_markerStorage).Verify(
                x => x.UpdateCurrentMarker(123),
                Times.Once());
        }

        [Fact]
        public async Task RefreshLocalCopyFromServer_should_write_results_to_exporter()
        {
            await _sut.RefreshLocalCopyFromServer(newDatabase: true);

            Mock.Get(_mergeExporter).Verify(
                x => x.WriteAsync(It.IsAny<IEnumerable<TicketExportResult>>()),
                Times.Once());
        }

        [Fact]
        public async Task RefreshLocalCopyFromServer_should_not_write_results_to_exporter_if_no_results()
        {
            Mock.Get(_ticketRetriever).Setup(x => x.GetBatch(null)).ReturnsAsync(new TicketExportResponse()
            {
                EndTime = 123,
                Results = new List<TicketExportResult>(),
            });

            await _sut.RefreshLocalCopyFromServer(newDatabase: true);

            Mock.Get(_mergeExporter).Verify(
                x => x.WriteAsync(It.IsAny<IEnumerable<TicketExportResult>>()),
                Times.Never());
        }

        [Fact]
        public void RefreshLocalCopyFromServer_should_terminate_loop_when_no_results()
        {
            Mock.Get(_ticketRetriever).Setup(x => x.GetBatch(null)).ReturnsAsync(new TicketExportResponse()
            {
                EndTime = 123,
                Results = new List<TicketExportResult>(),
            });

            _sut.RefreshLocalCopyFromServer(newDatabase: true).Wait(TimeSpan.FromSeconds(5));

            // Test passes if loop terminates in a timely manner.
        }

        [Fact]
        public void RefreshLocalCopyFromServer_should_terminate_loop_when_less_than_max_possible_results()
        {
            var oneLessThanMaxResultsCollection = new List<TicketExportResult>();
            foreach (var i in Enumerable.Range(0, Configuration.ZendeskMaxItemsReturnedFromTicketExportApi - 1))
            {
                oneLessThanMaxResultsCollection.Add(new TicketExportResult());
            }

            Mock.Get(_ticketRetriever).Setup(x => x.GetBatch(null)).ReturnsAsync(new TicketExportResponse()
            {
                EndTime = 123,
                Results = oneLessThanMaxResultsCollection,
            });

            _sut.RefreshLocalCopyFromServer(newDatabase: true).Wait(TimeSpan.FromSeconds(5));

            // Test passes if loop terminates in a timely manner.
        }

        [Fact]
        public async Task RefreshLocalCopyFromServer_should_use_marker_properly_in_a_loop()
        {
            var maxPossibleResults = new List<TicketExportResult>();
            foreach (var i in Enumerable.Range(0, Configuration.ZendeskMaxItemsReturnedFromTicketExportApi))
            {
                maxPossibleResults.Add(new TicketExportResult());
            }

            Mock.Get(_ticketRetriever).Setup(x => x.GetBatch(null)).ReturnsAsync(new TicketExportResponse()
            {
                EndTime = 123,
                Results = maxPossibleResults
            });

            await _sut.RefreshLocalCopyFromServer(newDatabase: true);

            Mock.Get(_ticketRetriever).Verify(x => x.GetBatch(null), Times.Once());
            Mock.Get(_ticketRetriever).Verify(x => x.GetBatch(123), Times.Once());
        }

        [Fact]
        public async Task ExportLocalCopyToCsv_queries_database()
        {
            await _sut.ExportLocalCopyToCsv("theFile.csv");

            Mock.Get(_database).Verify(
                x => x.QueryAsync<TicketExportResult>(
                    "select * from " + Configuration.TicketsTableName,
                    /* param */ null),
                Times.Once());
        }

        [Fact]
        public async Task ExportLocalCopyToCsv_calls_CsvFileWriter_with_results_from_database()
        {
            var records = new List<TicketExportResult>()
            {
                new TicketExportResult()
            };

            Mock.Get(_database)
                .Setup(x => x.QueryAsync<TicketExportResult>(
                    "select * from " + Configuration.TicketsTableName,
                    /* param */ null))
                .ReturnsAsync(records);

            await _sut.ExportLocalCopyToCsv("theFile.csv", allowOverwrite: false);

            Mock.Get(_csvFileWriter).Verify(
                x => x.WriteFile(records, "theFile.csv", /* allowOverwrite */ false),
                Times.Once());
        }
    }
}