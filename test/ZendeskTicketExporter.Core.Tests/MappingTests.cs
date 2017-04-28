using AutoMapper;
using Moq;
using System.Collections.Generic;
using Xunit;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core.Tests
{
    public class MappingTests
    {
        public MappingTests()
        {
            MappingConfiguration.Configure();
        }

        [Fact]
        public void AssertConfigurationIsValid()
        {
            Mapper.AssertConfigurationIsValid();
        }

        [Fact]
        public void Can_map_Ticket_to_FlattenedTicket()
        {
            Exporter.ZendeskApi = Mock.Of<IZendeskApi>();
            Mock.Get(Exporter.ZendeskApi)
                .Setup(x => x.GetTicketFieldsAsync())
                .ReturnsAsync(new List<TicketField>()
                {
                    new TicketField() { Id = 1, Title = "CustomField1" },
                    new TicketField() { Id = 2, Title = "CustomField2" }
                });

            var ticket = new Ticket()
            {
                CustomFields = new List<CustomField>()
                {
                    new CustomField() { Id = 1, Value = "CustomField1_Value" },
                    new CustomField() { Id = 2, Value = "CustomField2_Value" }
                }
            };

            var flattenedTicket = Mapper.Map<Ticket, FlattenedTicket>(ticket);

            var expectedFlattenedTicket = new FlattenedTicket()
            {
                CustomFieldsNamesAndValues = new Dictionary<string, string>()
                {
                    { "CustomField1", "CustomField1_Value" },
                    { "CustomField2", "CustomField2_Value" }
                }
            };

            CustomAssert.Equivalent(expectedFlattenedTicket, flattenedTicket);
        }
    }
}