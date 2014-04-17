using System.Collections.Generic;

namespace ZendeskTicketExporter.Core
{
    public interface ICsvFileWriter
    {
        void WriteFile<T>(IEnumerable<T> records, string filePath, bool allowOverwrite = false);
    }
}