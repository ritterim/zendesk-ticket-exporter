using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core.Tests
{
    public class SQLiteMergedTicketExporterTests
    {
        private readonly InMemoryDatabase _database;
        private readonly SqLiteMergedTicketExportResultExporter _sut;

        public SQLiteMergedTicketExporterTests()
        {
            _database = new InMemoryDatabase();
            _sut = new SqLiteMergedTicketExportResultExporter(_database);
        }

        [Fact]
        public async Task WriteAsync_creates_table_if_not_exists()
        {
            var tickets = new List<TicketExportResult>()
            {
                new TicketExportResult() { Id = 1 }
            };

            await _sut.WriteAsync(tickets);

            await AssertRecordCountIs(1);
        }

        [Fact]
        public async Task WriteAsync_inserts_records_twice()
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

            await AssertRecordCountIs(2);
        }

        private async Task AssertRecordCountIs(long count)
        {
            var actual = await _database.QueryScalerAsync<long>(
                "select count(*) from " + TicketExportResultExporter.TicketsTableName);

            Assert.Equal(count, actual);
        }
    }
}