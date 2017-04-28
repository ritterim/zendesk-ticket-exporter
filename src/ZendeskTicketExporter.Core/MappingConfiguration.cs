using AutoMapper;
using ZendeskApi_v2.Models.Tickets;

namespace ZendeskTicketExporter.Core
{
    public class MappingConfiguration
    {
        public static void Configure()
        {
            Mapper.CreateMap<Ticket, FlattenedTicket>()
                .ForMember(
                    dest => dest.CustomFieldsNamesAndValues,
                    opt => opt.ResolveUsing<CustomFieldsResolver>()
                        .ConstructedBy(() => new CustomFieldsResolver(Exporter.ZendeskApi))
                        .FromMember(m => m.CustomFields));
        }
    }
}