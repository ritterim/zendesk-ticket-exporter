using KellermanSoftware.CompareNetObjects;
using Xunit;

namespace ZendeskTicketExporter.Core.Tests
{
    public static class CustomAssert
    {
        public static void Equivalent<T>(T expected, T actual, CompareLogic compareLogic = null)
        {
            if (compareLogic == null)
                compareLogic = new CompareLogic();

            var compare = compareLogic.Compare(expected, actual);
            Assert.True(compare.AreEqual, compare.DifferencesString);
        }
    }
}