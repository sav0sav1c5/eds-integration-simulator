namespace Sotex.EDSPortal.IntegrationSimulation.SharedDTOs
{
    public class ExternalStatusUpdateDto
    {
        public Guid StatusUpdateRef { get; set; } // ExternalId
        public required string Message { get; set; }
        public List<ExternalStatusUpdateAttachmentDto>? AttachmentList { get; set; }
        public List<ExtendStatusUpdateDto>? ExtendList { get; set; }
        public DateTime Timestamp { get; set; }
        public DocumentInstanceStatus Status { get; set; }
        public Guid RequestRef { get; set; } // ExternalDocumentInstanceId
        public bool IsSentByThirdParty { get; set; }         
    }

    public class ExternalStatusUpdateAttachmentDto
    {
        public required string Name { get; set; }
        public required string Path { get; set; }
    }
}