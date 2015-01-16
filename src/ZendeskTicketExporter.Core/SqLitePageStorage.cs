using System.Threading.Tasks;

namespace ZendeskTicketExporter.Core
{
    public class SqLitePageStorage : IPageStorage
    {
        private readonly IDatabase _database;

        public SqLitePageStorage(IDatabase database)
        {
            _database = database;
        }

        public async Task<string> GetCurrentPage()
        {
            if (await _database.TableExistsAsync(Configuration.PageTableName) == false)
                return null;

            var page = await _database.QueryScalerAsync<string>(string.Format(
                "select max({0}) from {1}",
                Configuration.PageTableColumnName,
                Configuration.PageTableName));

            return page;
        }

        public async Task UpdateCurrentPage(string page)
        {
            await _database.ExecuteAsync(
                string.Format(
                    "create table if not exists {0} ({1} string); insert into {0} values(@page)",
                    Configuration.PageTableName,
                    Configuration.PageTableColumnName),
                new { page = page });
        }
    }
}