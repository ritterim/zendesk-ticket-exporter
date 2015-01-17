using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core
{
    /// <summary>
    /// Concrete implementation for an exporter which uses TicketExportResult
    /// </summary>
    public sealed class Exporter : ExporterBase<TicketExportResult>
    {
        private readonly IMarkerStorage _markerStorage;
        private readonly IMergedTicketExporter<TicketExportResult> _exporter;
        public static readonly string TableName = typeof(TicketExportResult).Name;

        public Exporter(ILog log, IDatabase database, IMarkerStorage markerStorage,
            ITicketRetriever ticketRetriever, IMergedTicketExporter<TicketExportResult> exporter,
            ICsvFileWriter csvFileWriter)
            : base(log,
                database,
                ticketRetriever,
                csvFileWriter,
                TableName)
        {
            _markerStorage = markerStorage;
            _exporter = exporter;
        }

        public static ExporterBase<TicketExportResult> GetDefaultInstance(string sitename, string username,
            string apiToken)
        {
            VerifyValidCredentials(sitename, username, apiToken);

            var log = LogManager.GetCurrentClassLogger();
            var database = new Database(sitename + ".sqlite");
            var wait = new Wait(log);
            var zendeskApi = new ZendeskApi(sitename, username, apiToken);
            return new Exporter(log, database, new SQLiteMarkerStorage(database),
                new TicketRetriever(wait, zendeskApi),
                new SqLiteMergedTicketExporter(database, TableName),
                new CsvFileWriter());
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

        public override async Task RefreshLocalCopyFromServer(bool newDatabase = false)
        {
            var marker = await _markerStorage.GetCurrentMarker();

            VerifyValidConfiguration(newDatabase, marker);

            while (true)
            {
                _log.InfoFormat("Begin copying tickets using marker {0}.", marker.GetValueOrDefault());

                var batch = await _ticketRetriever.GetBatchAsync(marker);
                if (batch.Results.Any())
                {
                    _log.InfoFormat(
                        "Inserting / updating {0} tickets in database retrieved from marker {1}.",
                        batch.Results.Count(),
                        marker.GetValueOrDefault());

                    await _exporter.WriteAsync(batch.Results);
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
    }
}