using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core.Tests
{
    public class SQLiteMergedTicketExporterTests
    {
        private readonly InMemoryDatabase _database;
        private readonly SQLiteMergedTicketExporter _sut;

        public SQLiteMergedTicketExporterTests()
        {
            _database = new InMemoryDatabase();
            _sut = new SQLiteMergedTicketExporter(_database);
        }

        [Fact]
        public async Task WriteAsync_FlattenedTickets_creates_table_if_not_exists()
        {
            var flattenedTickets = new List<FlattenedTicket>()
            {
                new FlattenedTicket() { Id = 1 }
            };

            await _sut.WriteAsync(flattenedTickets);

            await AssertRecordCountIs(Configuration.TicketsTableName, 1);
        }

        [Fact]
        public async Task WriteAsync_FlattenedTickets_inserts_records_twice()
        {
            var flattenedTickets = new List<FlattenedTicket>()
            {
                new FlattenedTicket() { Id = 1 }
            };
            var flattenedTickets2 = new List<FlattenedTicket>()
            {
                new FlattenedTicket() { Id = 2 }
            };

            await _sut.WriteAsync(flattenedTickets);
            await _sut.WriteAsync(flattenedTickets2);

            await AssertRecordCountIs(Configuration.TicketsTableName, 2);
        }

        [Fact]
        public async Task WriteAsync_TicketExportResult_creates_table_if_not_exists()
        {
            var tickets = new List<TicketExportResult>()
            {
                new TicketExportResult() { Id = 1 }
            };

            await _sut.WriteAsync(tickets);

            await AssertRecordCountIs(Configuration.TicketsExportTableName, 1);
        }

        [Fact]
        public async Task WriteAsync_TicketExportResult_inserts_records_twice()
        {
            var tickets = new List<TicketExportResult>()
            {
                new TicketExportResult() { Id = 1 }
            };
            var tickets2 = new List<TicketExportResult>()
            {
                new TicketExportResult() { Id = 2 }
            };

            await _sut.WriteAsync(tickets);
            await _sut.WriteAsync(tickets2);

            await AssertRecordCountIs(Configuration.TicketsExportTableName, 2);
        }

        private async Task AssertRecordCountIs(string tableName, long count)
        {
            var actual = await _database.QueryScalerAsync<long>(
                "select count(*) from " + tableName);

            Assert.Equal(count, actual);
        }
    }
}