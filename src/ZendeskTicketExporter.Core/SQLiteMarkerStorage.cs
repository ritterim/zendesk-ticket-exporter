using System.Threading.Tasks;

namespace ZendeskTicketExporter.Core
{
    public class SQLiteMarkerStorage : IMarkerStorage
    {
        private readonly IDatabase _database;

        public SQLiteMarkerStorage(IDatabase database)
        {
            _database = database;
        }

        public async Task<long?> GetCurrentMarker()
        {
            if (await _database.TableExistsAsync(Configuration.MarkersTableName) == false)
                return null;

            var marker = await _database.QueryScalerAsync<long?>(string.Format(
                "select max({0}) from {1}",
                Configuration.MarkersTableColumnName,
                Configuration.MarkersTableName));

            return marker;
        }

        public async Task UpdateCurrentMarker(long marker)
        {
            await _database.ExecuteAsync(
                string.Format(
                    "create table if not exists {0} ({1} integer); insert into {0} values(@marker)",
                    Configuration.MarkersTableName,
                    Configuration.MarkersTableColumnName),
                new { marker = marker });
        }
    }
}