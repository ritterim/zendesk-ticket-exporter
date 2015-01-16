using CommandLine;

namespace ZendeskTicketExporter.Console
{
    internal class Options
    {
        [Option('s', "sitename", Required = true,
            HelpText = "Sitename for accessing Zendesk API (https://[your-site-name].zendesk.com).")]
        public string Sitename { get; set; }

        [Option('u', "username", Required = true,
            HelpText = "Username for accessing Zendesk API (do not include \"/token\").")]
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
            HelpText = "Permit overwriting of export-csv-file.")]
        public bool CsvExportPathPermitOverwrite { get; set; }

        [Option('q', "quiet", Required = false,
            HelpText = "Suppress console logging output.")]
        public bool Quiet { get; set; }

        [Option('m', "method", Required = false,
            HelpText = "Selects which API Method to call. Set to true to use ticketing API which returns ticket data that includes custom properties. ")]
        public bool Method { get; set; }
    }
}