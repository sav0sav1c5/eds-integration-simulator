

namespace Sotex.EDSPortal.IntegrationSimulation.SharedDTOs
{
    public class ResponsePackage<T>
    {
        public ResponseStatus Status { get; set; }

        public string Message { get; set; }

        public T? Data { get; set; }

        public ResponsePackage()
        {
            Status = ResponseStatus.OK;
            Message = string.Empty;
        }

        public ResponsePackage(T data, ResponseStatus status = ResponseStatus.OK, string message = "")
        {
            Data = data;
            Status = status;
            Message = message;
        }

        public ResponsePackage(ResponseStatus status, string message)
        {
            Status = status;
            Message = message;
        }
    }

    public class ResponsePackageNoData
    {
        public ResponseStatus Status { get; set; }

        public string Message { get; set; }

        public ResponsePackageNoData()
        {
            Status = ResponseStatus.OK;
            Message = string.Empty;
        }

        public ResponsePackageNoData(ResponseStatus status, string message)
        {
            Status = status;
            Message = message;
        }
    }
}
