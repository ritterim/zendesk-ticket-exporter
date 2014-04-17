using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace ZendeskTicketExporter.Core.Tests
{
    public class CsvFileWriterTests
    {
        private static readonly string TestFileName = typeof(CsvFileWriterTests).Name + "_TestFile.csv";

        private readonly CsvFileWriter _sut;

        public CsvFileWriterTests()
        {
            _sut = new CsvFileWriter();

            Cleanup();
        }

        private void Cleanup()
        {
            if (File.Exists(TestFileName))
                File.Delete(TestFileName);
        }

        [Fact]
        public void WriteFile_writes_expected_csv_for_list_of_dynamic()
        {
            var records = new List<dynamic>()
            {
                new { A = "1", B = "2" },
                new { A = "3", B = "4" },
            };

            _sut.WriteFile(records, TestFileName);

            var expectedCsv = "A,B" + Environment.NewLine +
                              "1,2" + Environment.NewLine +
                              "3,4" + Environment.NewLine;

            var actualCsv = File.ReadAllText(TestFileName);

            Assert.Equal(expectedCsv, actualCsv);
        }

        [Fact]
        public void WriteFile_permits_overwrite_when_allowOverwrite_is_true_when_file_exists()
        {
            CreateEmptyFile(TestFileName);

            _sut.WriteFile(new List<dynamic>(), TestFileName, allowOverwrite: true);
        }

        [Fact]
        public void WriteFile_denies_overwrite_when_allowOverwrite_is_false_when_file_exists()
        {
            CreateEmptyFile(TestFileName);

            Assert.Throws<InvalidOperationException>(
                () => _sut.WriteFile(Enumerable.Empty<object>(), TestFileName, allowOverwrite: false));
        }

        private void CreateEmptyFile(string TestFileName)
        {
            if (File.Exists(TestFileName))
                throw new InvalidOperationException("File already exists.");

            File.WriteAllText(TestFileName, string.Empty);
        }
    }
}