using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ZendeskTicketExporter.Core
{
    /// <summary>
    /// A version of ZendeskApi_v2.Models.Tickets.Ticket which has had lists flattened to strings
    /// </summary>
    public class TicketResultFlattened
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public string Description { get; set; }

        public string Notes { get; set; }

        public string Via { get; set; }

        public string Priority { get; set; }

        public string TopicType { get; set; }

        public long? SubmitterId { get; set; }

        public long? UpdaterId { get; set; }

        public long? ForumId { get; set; }

        public long? OrganizationId { get; set; }

        public long? GroupId { get; set; }

        public string CustomFields { get; set; }

        public string Tags { get; set; }

        public string Attachments { get; set; }

        public int? CommentsCount { get; set; }

        [JsonProperty("created_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTimeOffset? UpdatedAt { get; set; }

        public int Id { get; set; }

        public string ResultType { get; set; }

        public string Url { get; set; }

        public string ExternalId { get; set; }

        public string Type { get; set; }

        public string Subject { get; set; }

        public long? RequesterId { get; set; }

        public long? AssigneeId { get; set; }

        public string Status { get; set; }
  }
}

