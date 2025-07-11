using Sotex.EDSPortal.IntegrationSimulation.SharedDTOs;

namespace Sotex.EDSPortal.IntegrationSimulation.Models
{
    public class RequestStatus
    {
        public string RequestRef { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public List<string> Emails { get; set; }
        public List<string> PhoneNumbers { get; set; }
        public DateTime ArrivalDate { get; set; }
        public DocumentInstanceStatus currentRequestStatus { get; set; }
        public string CompleteJson { get; set; }
    }
}
