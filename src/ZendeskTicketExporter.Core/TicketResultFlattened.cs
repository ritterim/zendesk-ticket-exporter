namespace ZendeskTicketExporter.Core
{
    /// <summary>
    /// A version of ZendeskApi_v2.Models.Search.Result which has had lists flattened to strings
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

        public string CreatedAt { get; set; }

        public string UpdatedAt { get; set; }

        public int Id { get; set; }

        public string ResultType { get; set; }

        public string Url { get; set; }

        public string ExternalId { get; set; }

        public string Type { get; set; }

        public string Subject { get; set; }

        public long? RequesterId { get; set; }

        public long? AssigneeId { get; set; }

        public string Status { get; set; }

        public long? BrandId { get; set; }

        public string DueAt { get; set; }

        public string FollowUpIds { get; set; }

        public long? ForumTopicId { get; set; }

        public bool HasIncidents { get; set; }

        public long? ProblemId { get; set; }

        public string RawSubject { get; set; }

        public string Recipient { get; set; }

        public string SatisfactionRating { get; set; }
    }
}

