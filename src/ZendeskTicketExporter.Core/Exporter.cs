using Common.Logging;
using LiteGuard;
using System;
using System.Linq;
using System.Threading.Tasks;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core
{
    public class Exporter
    {
        private readonly ILog _log;
        private readonly IDatabase _database;
        private readonly IMarkerStorage _markerStorage;
        private readonly ITicketRetriever _ticketRetriever;
        private readonly IMergedTicketExporter _mergeExporter;
        private readonly ICsvFileWriter _csvFileWriter;

        public Exporter(
            ILog log,
            IDatabase database,
            IMarkerStorage markerStorage,
            ITicketRetriever ticketRetriever,
            IMergedTicketExporter mergeExporter,
            ICsvFileWriter csvFileWriter)
        {
            _log = log;
            _database = database;
            _markerStorage = markerStorage;
            _ticketRetriever = ticketRetriever;
            _mergeExporter = mergeExporter;
            _csvFileWriter = csvFileWriter;
        }

        public static Exporter GetDefaultInstance(string siteName, string username, string apiToken)
        {
            Guard.AgainstNullArgument("siteName", siteName);
            Guard.AgainstNullArgument("username", username);
            Guard.AgainstNullArgument("apiToken", apiToken);

            var log = LogManager.GetCurrentClassLogger();
            var dbFile = siteName + ".sqlite";
            var database = new Database(dbFile);

            return new Exporter(
                log,
                database,
                new SQLiteMarkerStorage(database),
                new TicketRetriever(siteName, username, apiToken, log),
                new SQLiteMergedTicketExporter(database),
                new CsvFileWriter());
        }

        public async Task RefreshLocalCopyFromServer(bool newDatabase = false)
        {
            var marker = await _markerStorage.GetCurrentMarker();

            VerifyValidConfiguration(newDatabase, marker);

            while (true)
            {
                _log.InfoFormat("Begin copying tickets using marker {0}", marker.GetValueOrDefault());

                var batch = await _ticketRetriever.GetBatch(marker);
                if (batch.Results.Any())
                {
                    _log.InfoFormat(
                        "Inserting / updating {0} tickets in database retrieved from marker {1}",
                        batch.Results.Count(),
                        marker.GetValueOrDefault());

                    await _mergeExporter.WriteAsync(batch.Results);
                }

                marker = batch.EndTime;
                await _markerStorage.UpdateCurrentMarker(marker.Value);

                // Terminate when less than MaxItemsReturnedFromZendeskApi returned
                // rather than when zero returned, otherwise could end up in an infinite
                // loop if one or more tickets are created/updated in the course of the
                // _ticketRetriever.GetBatch(marker); cooldown period over and over.
                if (batch.Results.Count() < Configuration.ZendeskMaxItemsReturnedFromTicketExportApi)
                    break;
            }

            _log.Info("Completed copying tickets.");
        }

        public async Task ExportLocalCopyToCsv(string csvFilePath, bool allowOverwrite = false)
        {
            Guard.AgainstNullArgument("csvFilePath", csvFilePath);

            _log.Info("Writing tickets to csv file from local database");

            var records = await _database.QueryAsync<TicketExportResult>(
                "select * from " + Configuration.TicketsTableName);

            _csvFileWriter.WriteFile(records, csvFilePath, allowOverwrite);
        }

        /// <summary>
        /// This is to help ensure compliance with Zendesk API guidelines
        /// for not retriving all records all the time.
        /// This should force clients to run once with newDatabase = 'true',
        /// then subsequent runs must be newDatabase = 'false' to avoid
        /// potential issues with an unexpected missing database.
        /// </summary>
        private void VerifyValidConfiguration(bool newDatabase, long? marker)
        {
            if (newDatabase && marker.HasValue)
                throw new InvalidOperationException(
                    "Cannot specify newDatabase 'true' when updating from an existing marker.");

            if (newDatabase == false && marker.HasValue == false)
                throw new InvalidOperationException(
                    "marker must have a value when not creating a new database.");
        }
    }
}