using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;

namespace ZendeskTicketExporter.Core
{
    /// <summary>
    /// Uses the SearchFor() API method
    /// </summary>
    public class TicketExporter : ExporterBase<TicketResultFlattened>
    {
        private readonly SinglePropertyPerColumnExporter _exporter;
       // protected static string TableName = typeof(Result).Name;

        public TicketExporter(ILog log, IDatabase database, ITicketRetriever ticketRetriever, SinglePropertyPerColumnExporter exporter, ICsvFileWriter csvFileWriter)
            : base(log, database, ticketRetriever, csvFileWriter)
        {
            _exporter = exporter;
        }

        public static TicketExporter GetDefaultInstance(string sitename, string username, string apiToken)
        {
            VerifyValidCredentials(sitename, username, apiToken);

            var log = LogManager.GetCurrentClassLogger();
            var database = new Database(sitename + ".sqlite");
            var wait = new Wait(log);
            var zendeskApi = new ZendeskApi(sitename, username, apiToken);

            return new TicketExporter(log, database, new TicketRetriever(wait, zendeskApi), new SinglePropertyPerColumnExporter(database), new CsvFileWriter());
        }

        /// <summary>
        /// This is to help ensure compliance with Zendesk API guidelines
        /// for not retrieving all records all the time.
        /// This should force clients to run once with newDatabase = 'true',
        /// then subsequent runs must be newDatabase = 'false' to avoid
        /// potential issues with an unexpected missing database.
        /// </summary>
        private void VerifyValidConfiguration(bool newDatabase, string page)
        {
            if (newDatabase && !string.IsNullOrEmpty(page))
                throw new InvalidOperationException(
                    "Cannot specify newDatabase 'true' when updating from an existing marker.");

            if (newDatabase == false && string.IsNullOrEmpty(page) == false) //verify this makes sense
                throw new InvalidOperationException(
                    "page must have a value when not creating a new database.");
        }

        public override async Task RefreshLocalCopyFromServer(bool newDatabase = false)
        {
            int nextPage = 0;

            while (true)
            {
                _log.InfoFormat("Begin copying tickets using page {0}.", nextPage + 1);

                var batch = await _ticketRetriever.SearchFor(nextPage);
                if (batch.Results.Any())
                {
                    _log.InfoFormat(
                        "Inserting / updating {0} tickets in database retrieved from page {1}.",
                        batch.Results.Count(),
                        nextPage + 1);

                    await _exporter.WriteAsync(batch.Results);
                }

                if (string.IsNullOrEmpty(batch.NextPage))
                    break;

                nextPage++;
            }

            _log.Info("Completed copying tickets.");
        }
    }
}