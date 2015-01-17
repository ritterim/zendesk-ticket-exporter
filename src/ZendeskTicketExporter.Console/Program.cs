using CommandLine;
using CommandLine.Text;
using Common.Logging;
using Common.Logging.Simple;
using ZendeskTicketExporter.Core;

namespace ZendeskTicketExporter.Console
{
    using Console = System.Console;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                ConfigureLogging(options);
                IExporter exporter = null;
                switch (options.Method)
                {
                    case false:
                        exporter = Exporter.GetDefaultInstance(options.Sitename, options.Username, options.ApiToken);
                        break;
                    case true:
                        exporter = TicketExporter.GetDefaultInstance(options.Sitename, options.Username, options.ApiToken);
                        break;
                }

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

        private static void ConfigureLogging(Options options)
        {
            if (options.Quiet == false)
            {
                LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(
                    level: LogLevel.All,
                    showDateTime: true,
                    showLogName: false,
                    showLevel: false,
                    dateTimeFormat: null);
            }
        }
    }
}