using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Common.Logging;
using LiteGuard;
using System.Threading.Tasks;

namespace ZendeskTicketExporter.Core
{
    /// <summary>
    /// Base class that allows for the export from the database to a csv file
    /// </summary>
    /// <typeparam name="T">The type to deserialize from the database to</typeparam>
    public abstract class ExporterBase<T> : IExporter 
    {
        protected readonly ILog _log;
        protected readonly IDatabase _database;
        protected readonly ITicketRetriever _ticketRetriever;
        protected readonly ICsvFileWriter _csvFileWriter;
        protected readonly string _tableName;

        protected ExporterBase(
            ILog log,
            IDatabase database,
            ITicketRetriever ticketRetriever,
            ICsvFileWriter csvFileWriter,
            string tableName)
        {
            _log = log;
            _database = database;
            _ticketRetriever = ticketRetriever;
            _csvFileWriter = csvFileWriter;
            _tableName = tableName;
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

            var records = await _database.QueryAsync<T>("select * from " + _tableName);
            _csvFileWriter.WriteFile(records, csvFilePath, allowOverwrite);
        }
    }
}