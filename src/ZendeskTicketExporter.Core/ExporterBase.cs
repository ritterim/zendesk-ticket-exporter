using Common.Logging;
using LiteGuard;
using System.Threading.Tasks;

namespace ZendeskTicketExporter.Core
{
    /// <summary>
    /// Base class that allows for the export from the database to a csv file
    /// </summary>
    /// <typeparam name="T">The type to deserialize from the database to</typeparam>
    public abstract class ExporterBase<T> : IExporter where T:new()
    {
        protected readonly ILog _log;
        protected readonly IDatabase _database;
        protected readonly ITicketRetriever _ticketRetriever;
        protected readonly ICsvFileWriter _csvFileWriter;
        protected static readonly string TableName = typeof(T).Name;

        protected ExporterBase(
            ILog log,
            IDatabase database,
            ITicketRetriever ticketRetriever,
            ICsvFileWriter csvFileWriter
     )
        {
            _log = log;
            _database = database;
            _ticketRetriever = ticketRetriever;
            _csvFileWriter = csvFileWriter;
        }
   
        public abstract Task RefreshLocalCopyFromServer(bool newDatabase = false);

        protected static void VerifyValidCredentials(string sitename, string username, string apiToken)
        {
            Guard.AgainstNullArgument("sitename", sitename);
            Guard.AgainstNullArgument("username", username);
            Guard.AgainstNullArgument("apiToken", apiToken);
        }

        public  async Task ExportLocalCopyToCsv(string csvFilePath, bool allowOverwrite = false)
        {
            Guard.AgainstNullArgument("csvFilePath", csvFilePath);

            _log.Info("Writing tickets to csv file from local database.");

            var records = await _database.QueryAsync<T>("select * from " + TableName);
            _csvFileWriter.WriteFile(records, csvFilePath, allowOverwrite);
        }
    }
}