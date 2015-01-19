using AutoMapper;
using Common.Logging;
using LiteGuard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZendeskApi_v2.Models.Tickets;
using ZendeskTicketExporter.Core.Extensions;

namespace ZendeskTicketExporter.Core
{
    public class Exporter
    {
        public static IZendeskApi ZendeskApi;

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

        public static Exporter GetDefaultInstance(string sitename, string username, string apiToken)
        {
            Guard.AgainstNullArgument("sitename", sitename);
            Guard.AgainstNullArgument("username", username);
            Guard.AgainstNullArgument("apiToken", apiToken);

            var log = LogManager.GetCurrentClassLogger();
            var dbFile = sitename + ".sqlite";
            var database = new Database(dbFile);
            var wait = new Wait(log);
            var zendeskApi = new ZendeskApi(sitename, username, apiToken, log);

            ZendeskApi = zendeskApi;

            return new Exporter(
                log,
                database,
                new SQLiteMarkerStorage(database),
                new TicketRetriever(wait, zendeskApi),
                new SQLiteMergedTicketExporter(database),
                new CsvFileWriter());
        }

        public async Task RefreshLocalCopyFromServer(bool getExtendedTicketInformation = false, bool newDatabase = false)
        {
            var marker = await _markerStorage.GetCurrentMarker();

            VerifyValidConfiguration(newDatabase, marker);

            await RefreshFromServer(marker);

            if (getExtendedTicketInformation)
            {
                _log.Info("Begin copying extended ticket information.");
                await RefreshExtendedInformationFromServer(marker);
            }

            _log.Info("Completed copying tickets.");
        }

        public async Task ExportLocalCopyToCsv(string csvFilePath, bool allowOverwrite = false)
        {
            Guard.AgainstNullArgument("csvFilePath", csvFilePath);

            _log.Info("Writing tickets to csv file from local database.");

            var records = await _database.QueryAsync<TicketExportResult>(
                "select * from " + Configuration.TicketsExportTableName);

            _csvFileWriter.WriteFile(records, csvFilePath, allowOverwrite);
        }

        public async Task ExportLocalCopyToCsvWithExtendedInformation(string csvFilePath, bool allowOverwrite = false)
        {
            Guard.AgainstNullArgument("csvFilePath", csvFilePath);

            _log.Info("Writing tickets to csv file from local database.");

            var records = await _database.QueryAsync<Ticket>(
                "select * from " + Configuration.TicketsTableName);

            _csvFileWriter.WriteFile(records, csvFilePath, allowOverwrite);
        }

        private async Task<long?> RefreshFromServer(long? marker)
        {
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

            return marker;
        }

        private async Task RefreshExtendedInformationFromServer(long? marker)
        {
            _log.InfoFormat(
                "Begin copying tickets extended information using marker {0}.",
                marker.GetValueOrDefault());

            var ticketIds = await GetTicketIdsToUpdateAsync(marker);

            var tickets = await _ticketRetriever.GetAsync(ticketIds);

            var flattenedTickets = Mapper.Map<IEnumerable<Ticket>, IEnumerable<FlattenedTicket>>(tickets);

            await _mergeExporter.WriteAsync(flattenedTickets);
        }

        private async Task<IEnumerable<long>> GetTicketIdsToUpdateAsync(long? marker)
        {
            IEnumerable<long> ticketIds = null;
            if (await _database.TableExistsAsync(Configuration.TicketsTableName))
            {
                // Update missing tickets and tickets that have been modified since previous run
                // (which could have happened in the past with getExtendedTicketInformation == false)

                var startingTimestamp = marker == null
                    ? (DateTime.Now)
                    : marker.Value.FromUnixTime();

                ticketIds = await _database.QueryAsync<long>(
                    string.Format(
                        @"select Id from {0} where UpdatedAt <= @startingTimestamp
                          union
                          select Id from {0} where not exists(select Id from {1})",
                        Configuration.TicketsExportTableName,
                        Configuration.TicketsTableName),
                        new { startingTimestamp = SqliteLocalDateTimeString(startingTimestamp) });
            }
            else
            {
                // Refresh all

                ticketIds = await _database.QueryAsync<long>(
                    string.Format("select Id from {0}", Configuration.TicketsExportTableName));
            }

            return ticketIds;
        }

        // 2014-04-28 13:00:00 -0400
        private string SqliteLocalDateTimeString(DateTime dateTime)
        {
            // Adapted from http://stackoverflow.com/a/4879166/941536
            var utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(dateTime.ToLocalTime());
            var dateTimeString = dateTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss ") +
                ((utcOffset < TimeSpan.Zero) ? "-" : "+") +
                utcOffset.ToString("hhmm");

            return dateTimeString;
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