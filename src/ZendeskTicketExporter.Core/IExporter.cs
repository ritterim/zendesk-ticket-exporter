using System.Threading.Tasks;

namespace ZendeskTicketExporter.Core
{
    public interface IExporter
    {
        Task RefreshLocalCopyFromServer(bool newDatabase = false);
        /// <summary>
        /// Exports the local copy to CSV.
        /// </summary>
        /// <param name="csvFilePath">The CSV file path.</param>
        /// <param name="allowOverwrite">if set to <c>true</c> [allow overwrite].</param>
        /// <returns></returns>
        Task ExportLocalCopyToCsv(string csvFilePath, bool allowOverwrite = false);
    }
}