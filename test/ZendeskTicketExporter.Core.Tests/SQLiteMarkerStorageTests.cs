using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ZendeskTicketExporter.Core.Tests
{
    public class SQLiteMarkerStorageTests
    {
        private readonly InMemoryDatabase _database;
        private readonly SQLiteMarkerStorage _sut;

        public SQLiteMarkerStorageTests()
        {
            _database = new InMemoryDatabase();
            _sut = new SQLiteMarkerStorage(_database);
        }

        [Fact]
        public async Task GetCurrentMarker_returns_null_when_no_table()
        {
            var marker = await _sut.GetCurrentMarker();

            Assert.Null(marker);
        }

        [Fact]
        public async Task GetCurrentMarker_returns_existing_marker()
        {
            await _database.ExecuteAsync(string.Format(
                "create table {0} ({1} integer)",
                Configuration.MarkersTableName,
                Configuration.MarkersTableColumnName));

            await _database.ExecuteAsync(string.Format(
                "insert into {0} values (123)",
                Configuration.MarkersTableName));

            var marker = await _sut.GetCurrentMarker();

            Assert.Equal(123, marker);
        }

        [Fact]
        public async Task UpdateCurrentMarker_writes_marker_when_no_table()
        {
            await _sut.UpdateCurrentMarker(123);

            var markers = await GetAllMarkers();

            Assert.Equal(1, markers.Count());
            Assert.Equal(123, markers.Single());
        }

        [Fact]
        public async Task UpdateCurrentMarker_inserts_marker()
        {
            await _database.ExecuteAsync(string.Format(
                "create table {0} ({1} integer)",
                Configuration.MarkersTableName,
                Configuration.MarkersTableColumnName));

            await _sut.UpdateCurrentMarker(123);
            await _sut.UpdateCurrentMarker(456);

            var markers = await GetAllMarkers();

            Assert.Equal(2, markers.Count());
        }

        private async Task<IEnumerable<long>> GetAllMarkers()
        {
            return await _database.QueryAsync<long>(string.Format(
                "select {0} from {1}",
                Configuration.MarkersTableColumnName,
                Configuration.MarkersTableName));
        }
    }
}