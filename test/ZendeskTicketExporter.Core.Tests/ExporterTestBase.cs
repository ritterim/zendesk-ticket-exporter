using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ZendeskTicketExporter.Core.Tests
{
    public class ExporterTestBase<TExporter, TResult> 
        where TExporter : MergedTicketExporterBase<TResult> 
        where TResult : new()
    {
        private readonly InMemoryDatabase _database;
        private readonly TExporter _sut;

        public ExporterTestBase()
        {
            _database = new InMemoryDatabase();
            _sut = (TExporter)Activator.CreateInstance(typeof(TExporter), _database);
        }

        [Fact]
        public async Task WriteAsync_creates_table_if_not_exists()
        {
            IEnumerable<TResult> tickets = new List<TResult>()
            {
                new TResult() {  }
            };

            await _sut.WriteAsync(tickets);

            await AssertRecordCountIs(1);
        }
        // This fails returnign a count of 1. Perhaps its a sqllite issue where it would not allow duplicate records?
        [Fact]
        public async Task WriteAsync_inserts_records_twice()
        {
            var tickets = new List<TResult>()
            {
                new TResult() {  }
            };
            var tickets2 = new List<TResult>()
            {
                new TResult() { }
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
    //public class SQLiteMergedTicketExporterTests : ExporterTestBase<SqLiteMergedTicketExporter, TicketExportResult>{ }
    //public class SinglePropertyPerColumnExporterTests : ExporterTestBase< SinglePropertyPerColumnExporter, Result> {}
}