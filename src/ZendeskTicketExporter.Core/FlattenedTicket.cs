using System.Collections.Generic;

namespace ZendeskTicketExporter.Core
{
    public class FlattenedTicket
    {
        public FlattenedTicket()
        {
            CollaboratorEmails = new List<string>();
            CollaboratorIds = new List<long>();
            CustomFieldsNamesAndValues = new Dictionary<string, string>();
            Tags = new List<string>();
        }

        public long? AssigneeId { get; set; }

        public IList<string> CollaboratorEmails { get; set; }

        //public IList<long> CollaboratorIds { get; }
        public IList<long> CollaboratorIds { get; set; }

        //public Comment Comment { get; set; }

        public string CreatedAt { get; set; }

        //public IList<CustomField> CustomFields { get; set; }
        public IDictionary<string, string> CustomFieldsNamesAndValues { get; set; }

        //public string Description { get; }
        public string Description { get; set; }

        public string DueAt { get; set; }

        public object ExternalId { get; set; }

        public object ForumTopicId { get; set; }

        public long? GroupId { get; set; }

        public bool HasIncidents { get; set; }

        public long? Id { get; set; }

        public long? OrganizationId { get; set; }

        public string Priority { get; set; }

        public object ProblemId { get; set; }

        public string Recipient { get; set; }

        //public Requester Requester { get; set; }
        public string RequesterEmail { get; set; }
        public long RequesterLocaleId { get; set; }
        public string RequesterName { get; set; }

        public long? RequesterId { get; set; }

        //public SatisfactionRating SatisfactionRating { get; set; }
        public string SatisfactionRatingComment { get; set; }
        public string SatisfactionRatingScore { get; set; }

        public string Status { get; set; }

        public string Subject { get; set; }

        public long? SubmitterId { get; set; }

        public IList<string> Tags { get; set; }

        public long? TicketFormId { get; set; }

        public string Type { get; set; }

        public string UpdatedAt { get; set; }

        public string Url { get; set; }

        //public Via Via { get; set; }
        public string ViaChannel { get; set; }
        public string ViaSourceFromAddress { get; set; }
        public string ViaSourceFromName { get; set; }
        public string ViaSourceRel { get; set; }
        public string ViaSourceToAddress { get; set; }
        public string ViaSourceToName { get; set; }
    }
}