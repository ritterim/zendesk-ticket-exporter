using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core
{
    public class CustomFieldsResolver : ValueResolver<IEnumerable<CustomField>, IDictionary<string, string>>
    {
        private static IEnumerable<TicketField> CachedTicketFields;

        private readonly IZendeskApi _zendeskApi;

        public CustomFieldsResolver(IZendeskApi zendeskApi)
        {
            _zendeskApi = zendeskApi;
        }

        protected override IDictionary<string, string> ResolveCore(IEnumerable<CustomField> source)
        {
            if (CachedTicketFields == null)
                PopulateCachedTicketFieldsAsync().Wait();

            var results = new Dictionary<string, string>();

            foreach (var customField in source)
            {
                var ticketField = CachedTicketFields.SingleOrDefault(x => x.Id == customField.Id);
                results.Add(
                    ticketField == null
                        ? "UnknownCustomField_Id_" + customField.Id
                        : ticketField.Title,
                    customField.Value.ToString());
            }

            return results;
        }

        private async Task PopulateCachedTicketFieldsAsync()
        {
            var ticketFields = await _zendeskApi.GetTicketFieldsAsync();
            CachedTicketFields = ticketFields;
        }
    }
}