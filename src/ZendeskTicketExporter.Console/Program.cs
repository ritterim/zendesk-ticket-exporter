using CommandLine;
using CommandLine.Text;
using ZendeskTicketExporter.Core;

namespace ZendeskTicketExporter.Console
{
    using Console = System.Console;

    internal class Options
    {
        [Option('s', "sitename", Required = true,
            HelpText = "Site name for accessing Zendesk API (https://[your-site-name].zendesk.com)")]
        public string SiteName { get; set; }

        [Option('u', "username", Required = true,
            HelpText = "Username for accessing Zendesk API (do not include \"/token\")")]
        public string Username { get; set; }

        [Option('t', "token", Required = true,
            HelpText = "API token for accessing Zendesk API. You can enable and view this at https://[your-site-name].zendesk.com/settings/api")]
        public string ApiToken { get; set; }

        [Option('n', "new-database", Required = false,
            HelpText = "Permit creation of a new database, does not permit refresh of existing database. This is to ensure compliance with the Zendesk API guidelines.")]
        public bool NewDatabase { get; set; }

        [Option('e', "export-csv-file", Required = false,
            HelpText = "Path to CVS export file, if not specified no CSV export will be performed.")]
        public string CsvExportPath { get; set; }

        [Option('o', "export-csv-file-overwrite", Required = false,
            HelpText = "Permit overwriting of export-csv-file")]
        public bool CsvExportPathPermitOverwrite { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                var exporter = Exporter.GetDefaultInstance(
                    options.SiteName,
                    options.Username,
                    options.ApiToken);

                exporter.RefreshLocalCopyFromServer(options.NewDatabase).Wait();

                if (string.IsNullOrWhiteSpace(options.CsvExportPath) == false)
                {
                    exporter.ExportLocalCopyToCsv(
                        options.CsvExportPath,
                        options.CsvExportPathPermitOverwrite)
                        .Wait();
                }
            }
            else
            {
                var helpText = HelpText.AutoBuild(options);
                Console.WriteLine(helpText);
            }
        }
    }
}