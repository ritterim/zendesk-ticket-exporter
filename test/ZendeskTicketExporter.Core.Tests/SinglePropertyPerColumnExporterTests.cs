using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using ZendeskApi_v2.Models.Search;

namespace ZendeskTicketExporter.Core.Tests
{
    public class SinglePropertyPerColumnExporterTests
    {
        private readonly InMemoryDatabase _database;
        private readonly SinglePropertyPerColumnExporter _sut;

        public SinglePropertyPerColumnExporterTests()
        {
            _database = new InMemoryDatabase();
            _sut = new SinglePropertyPerColumnExporter(_database);
        }

        [Fact]
        public async Task WriteAsync_creates_table_if_not_exists()
        {
            IEnumerable<Result> tickets = new List<Result>()
            {
                new Result() { Id = 1 }
            };

            await _sut.WriteAsync(tickets);

            await AssertRecordCountIs(1);
        }

        [Fact]
        public async Task WriteAsync_inserts_records_twice()
        {
            var tickets = new List<Result>()
            {
                new Result() { Id = 1 }
            };
            var tickets2 = new List<Result>()
            {
                new Result() { Id = 2 }
            };

            await _sut.WriteAsync(tickets);
            await _sut.WriteAsync(tickets2);

            await AssertRecordCountIs(2);
        }

        private async Task AssertRecordCountIs(long count)
        {
            var actual = await _database.QueryScalerAsync<long>(
                "select count(*) from " + _sut.TableName);

            Assert.Equal(count, actual);
        }
    }
}