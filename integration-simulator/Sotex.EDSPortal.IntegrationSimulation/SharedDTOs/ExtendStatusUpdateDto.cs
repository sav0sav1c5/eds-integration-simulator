namespace Sotex.EDSPortal.IntegrationSimulation.SharedDTOs
{
    // class that is sent to the 3rd party when user extends a request/status update
    public class ExtendStatusUpdateDto
    {
        public Guid ExternalId { get; set; } //  RequestRef to identify specific document instance
        public string? Message { get; set; } // MessageToOperator
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public List<ExtendStatusUpdateAttachmentDto> ExtendAttachmentList { get; set; } = new();
    }

    public class ExtendStatusUpdateAttachmentDto
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty; // TODO: use existing uploadFile service?
        public string? Comment { get; set; }
    }
}
