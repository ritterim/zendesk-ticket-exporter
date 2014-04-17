using CsvHelper;
using LiteGuard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ZendeskTicketExporter.Core
{
    public class CsvFileWriter : ICsvFileWriter
    {
        public void WriteFile<T>(IEnumerable<T> records, string filePath, bool allowOverwrite = false)
        {
            Guard.AgainstNullArgument("records", records);
            Guard.AgainstNullArgument("filePath", filePath);

            HandleFileOverwrite(filePath, allowOverwrite);

            using (var streamWriter = new StreamWriter(filePath))
            using (var writer = new CsvWriter(streamWriter))
            {
                writer.WriteRecords(records as IEnumerable);
            }
        }

        private static void HandleFileOverwrite(string filePath, bool allowOverwrite)
        {
            if (File.Exists(filePath))
            {
                if (allowOverwrite)
                {
                    File.Delete(filePath);
                }
                else
                {
                    throw new InvalidOperationException(string.Format(
                        "Unable to export CSV: {0} exists and allowOverwrite is false.",
                        filePath));
                }
            }
        }
    }
}