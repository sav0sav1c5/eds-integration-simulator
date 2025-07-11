using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Sotex.EDSPortal.IntegrationSimulation.Models;
using Sotex.EDSPortal.Shared.DTOs;
using Sotex.EDSPortal.Shared.Enums;
using Sotex.EDSPortal.IntegrationSimulation.SharedDTOs;

namespace Sotex.EDSPortal.IntegrationSimulation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestStatusController : Controller
    {
        private readonly string _uploadFolderPath;
        private readonly string _jsonRequestListPath;
        private readonly string _jsonRequestStatusListPath;
        private readonly string _jsonStatusSnapshotsFilePath;
        private readonly IConfiguration _config;
        private List<RequestStatus> requestsToDisplay;

        public RequestStatusController(IConfiguration config, IHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                _uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                _jsonRequestListPath = Path.Combine(Directory.GetCurrentDirectory(), "requestList.json");
                _jsonRequestStatusListPath = Path.Combine(Directory.GetCurrentDirectory(), "requestStatusList.json");
                _jsonStatusSnapshotsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "statusUpdateSnapshots.json");
                requestsToDisplay = new List<RequestStatus>();
            }
            else
            {
                _uploadFolderPath = "/var/www/sim-uploads";
                _jsonRequestListPath = "/var/www/sim-uploads/requestList.json";
                _jsonRequestStatusListPath = "/var/www/sim-uploads/requestUpdateList.json";
                _jsonStatusSnapshotsFilePath = Path.Combine("/var/www/sim-uploads/", "Data", "statusUpdateSnapshots.json");
                requestsToDisplay = new List<RequestStatus>();
            }

            _config = config;

            if (!Directory.Exists(_uploadFolderPath))
            {
                Directory.CreateDirectory(_uploadFolderPath);
            }

            if (!System.IO.File.Exists(_jsonRequestListPath))
            {
                System.IO.File.WriteAllText(_jsonRequestListPath, "[]");
            }

            if (!System.IO.File.Exists(_jsonRequestStatusListPath))
            {
                System.IO.File.WriteAllText(_jsonRequestStatusListPath, "[]");
            }
        }

        // * Index - main method for view that gets all requests and try to apply filtering or searching
        [HttpGet("")]
        public IActionResult Index(string sortField = null, string sortOrder = null,
                                  string requestRefFilter = null, string firstNameFilter = null,
                                  string lastNameFilter = null, string emailFilter = null,
                                  string phoneFilter = null, string arrivalFromDateFilter = null,
                                  string arrivalToDateFilter = null)
        {
            var filePath = _jsonRequestListPath;

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("JSON file not found.");
            }

            var jsonComplete = System.IO.File.ReadAllText(filePath);
            var jsonArray = JsonDocument.Parse(jsonComplete).RootElement;

            var result = new List<RequestStatus>();
            foreach (var element in jsonArray.EnumerateArray())
            {
                result.Add(ParseRequestStatus(element));
            }

            result = FilterRequests(result, requestRefFilter, firstNameFilter, lastNameFilter,
                       emailFilter, phoneFilter, arrivalFromDateFilter, arrivalToDateFilter);

            if (!string.IsNullOrEmpty(sortField))
            {
                result = SortRequests(result, sortField, sortOrder);
            }

            ViewBag.CurrentSort = sortField;
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.RequestRefFilter = requestRefFilter;
            ViewBag.FirstNameFilter = firstNameFilter;
            ViewBag.LastNameFilter = lastNameFilter;
            ViewBag.EmailFilter = emailFilter;
            ViewBag.PhoneFilter = phoneFilter;
            ViewBag.ArrivalFromDateFilter = arrivalFromDateFilter;
            ViewBag.ArrivalToDateFilter = arrivalToDateFilter;

            requestsToDisplay = result;

            return View(result);
        }

        // * FindRequestStatus - method that read requestStatusList.json and trying to find ExternalStatusUploadDto with specific request ref and get status
        public ExternalStatusUpdateDto FindRequestStatus(string externalId)
        {
            var filePath = _jsonRequestStatusListPath;

            if (!System.IO.File.Exists(filePath))
            {
                throw new Exception("JSON file not found!");
            }

            var jsonComplete = System.IO.File.ReadAllText(filePath);
            var jsonArray = JsonDocument.Parse(jsonComplete).RootElement;

            var requestStatuses = new List<ExternalStatusUpdateDto>();

            foreach (var element in jsonArray.EnumerateArray())
            {
                var statusUpdateRef = element.GetProperty("StatusUpdateRef").GetString();

                ExternalStatusUpdateDto externalStatus = new ExternalStatusUpdateDto
                {
                    StatusUpdateRef = Guid.Parse(statusUpdateRef),
                    Message = element.GetProperty("Message").GetString(),
                    Status = (DocumentInstanceStatus)element.GetProperty("Status").GetInt32(),
                    RequestRef = Guid.Parse(element.GetProperty("RequestRef").GetString()),
                    Timestamp = element.GetProperty("Timestamp").GetDateTime()
                };

                requestStatuses.Add(externalStatus);
            }

            var statusHistory = requestStatuses
                .Where(u => u.RequestRef.ToString() == externalId)
                .OrderBy(u => u.Timestamp)
                .ToList();

            if (!statusHistory.Any())
            {
                return new ExternalStatusUpdateDto
                {
                    StatusUpdateRef = Guid.NewGuid(),
                    Message = "Потребна допуна захтева",
                    Timestamp = DateTime.UtcNow,
                    Status = DocumentInstanceStatus.NEED_EXTENSION,
                    RequestRef = Guid.Parse(externalId)
                };
            }

            return statusHistory.Last();
        }

        // * ParseRequestStatus - method that parse json of single request and get needed fields so its possible to create RequestStatus object
        private RequestStatus ParseRequestStatus(JsonElement element)
        {
            var requestRef = element.GetProperty("RequestRef").GetString();
            var completeJson = JsonSerializer.Serialize(element, new JsonSerializerOptions { WriteIndented = true });

            var client = element.GetProperty("Client");
            var firstName = client.TryGetProperty("PersonFirstName", out var firstNameValue) ?
                firstNameValue.GetString() : string.Empty;
            var lastName = client.TryGetProperty("PersonLastName", out var lastNameValue) ?
                lastNameValue.GetString() : string.Empty;

            var emails = new List<string>();
            if (client.TryGetProperty("Emails", out var emailsElement) && emailsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var email in emailsElement.EnumerateArray())
                {
                    emails.Add(email.GetString());
                }
            }

            var phones = new List<string>();
            if (client.TryGetProperty("Phones", out var phonesElement) && phonesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var phone in phonesElement.EnumerateArray())
                {
                    phones.Add(phone.GetString());
                }
            }

            var status = FindRequestStatus(requestRef);

            return new RequestStatus
            {
                RequestRef = requestRef,
                PersonFirstName = firstName,
                PersonLastName = lastName,
                Emails = emails,
                PhoneNumbers = phones,
                ArrivalDate = DateTime.UtcNow,
                currentRequestStatus = status.Status,
                CompleteJson = completeJson
            };
        }

        // * FilterRequests - method for filtering requests by provided filter criterium
        private List<RequestStatus> FilterRequests(List<RequestStatus> requests, string requestRefFilter,
                                                  string firstNameFilter, string lastNameFilter,
                                                  string emailFilter, string phoneFilter,
                                                  string arrivalFromDateFilter, string arrivalToDateFilter)
        {
            var filteredRequests = requests;

            if (!string.IsNullOrEmpty(requestRefFilter))
            {
                filteredRequests = filteredRequests.Where(r =>
                    r.RequestRef.Contains(requestRefFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(firstNameFilter))
            {
                filteredRequests = filteredRequests.Where(r =>
                    r.PersonFirstName.Contains(firstNameFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(lastNameFilter))
            {
                filteredRequests = filteredRequests.Where(r =>
                    r.PersonLastName.Contains(lastNameFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(emailFilter))
            {
                filteredRequests = filteredRequests.Where(r =>
                    r.Emails != null && r.Emails.Any(e =>
                        e.Contains(emailFilter, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (!string.IsNullOrEmpty(phoneFilter))
            {
                filteredRequests = filteredRequests.Where(r =>
                    r.PhoneNumbers != null && r.PhoneNumbers.Any(p =>
                        p.Contains(phoneFilter, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (!string.IsNullOrEmpty(arrivalFromDateFilter) && DateTime.TryParse(arrivalFromDateFilter, out var fromDate))
            {
                filteredRequests = filteredRequests.Where(r => r.ArrivalDate >= fromDate).ToList();
            }

            if (!string.IsNullOrEmpty(arrivalToDateFilter) && DateTime.TryParse(arrivalToDateFilter, out var toDate))
            {
                // Add one day to include the entire selected day
                toDate = toDate.AddDays(1);
                filteredRequests = filteredRequests.Where(r => r.ArrivalDate < toDate).ToList();
            }

            return filteredRequests;
        }

        // * SortRequests - method for sorting request based on option that user choose
        private List<RequestStatus> SortRequests(List<RequestStatus> requests, string sortField, string sortOrder)
        {
            var isAscending = string.IsNullOrEmpty(sortOrder) || sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase);

            return sortField switch
            {
                "RequestRef" => isAscending
                    ? requests.OrderBy(r => r.RequestRef).ToList()
                    : requests.OrderByDescending(r => r.RequestRef).ToList(),
                "FirstName" => isAscending
                    ? requests.OrderBy(r => r.PersonFirstName).ToList()
                    : requests.OrderByDescending(r => r.PersonFirstName).ToList(),
                "LastName" => isAscending
                    ? requests.OrderBy(r => r.PersonLastName).ToList()
                    : requests.OrderByDescending(r => r.PersonLastName).ToList(),
                "Emails" => isAscending
                    ? requests.OrderBy(r => r.Emails != null && r.Emails.Any() ? r.Emails.First() : string.Empty).ToList()
                    : requests.OrderByDescending(r => r.Emails != null && r.Emails.Any() ? r.Emails.First() : string.Empty).ToList(),
                "Phone" => isAscending
                    ? requests.OrderBy(r => r.PhoneNumbers != null && r.PhoneNumbers.Any() ? r.PhoneNumbers.First() : string.Empty).ToList()
                    : requests.OrderByDescending(r => r.PhoneNumbers != null && r.PhoneNumbers.Any() ? r.PhoneNumbers.First() : string.Empty).ToList(),
                "ArrivalDate" => isAscending
                    ? requests.OrderBy(r => r.ArrivalDate).ToList()
                    : requests.OrderByDescending(r => r.ArrivalDate).ToList(),
                _ => requests
            };
        }


        // * GetDetails - method that retrieve complete json of request that is found by request ref  
        [HttpGet("GetDetails")]
        public IActionResult GetDetails(string id)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "requestList.json");
            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found.");

            var jsonContent = System.IO.File.ReadAllText(filePath);
            var jsonArray = JsonDocument.Parse(jsonContent).RootElement;

            var selected = jsonArray.EnumerateArray()
                .FirstOrDefault(e => e.GetProperty("RequestRef").GetString() == id);

            if (selected.ValueKind == JsonValueKind.Undefined)
                return NotFound("Request not found.");

            return Json(selected);
        }

        /* 
         IN_PROGRESS [status: 3]
         * getSpecificInProgress - reading and updating request, search by request ref and simulate changing status to 'IN_PROCESS'
         * UpdateRequest_InProgress - return ExternalStatusUpdateDto with updated status to IN PROCESS, message, timestamp, ...
        */

        [HttpGet("getSpecificInProgress")]
        public async Task<IActionResult> GetSpecificRequestInProgress(string externalId)
        {
            var requests = ReadRequestsFromJson();
            var request = requests.FirstOrDefault(r => r.RequestRef == externalId);
            
            if (request == null)
            {
                return NotFound(new { Message = "Request not found." });
            }
            
            var dto = UpdateRequest_InProgress(externalId);

            var requestStatusPath = Path.Combine(Directory.GetCurrentDirectory(), "requestStatusList.json");

            List<ExternalStatusUpdateDto> existingList = new();

            if (System.IO.File.Exists(requestStatusPath))
            {
                var existingJson = await System.IO.File.ReadAllTextAsync(requestStatusPath);
                existingList = JsonSerializer.Deserialize<List<ExternalStatusUpdateDto>>(existingJson) ?? new List<ExternalStatusUpdateDto>();
            }

            existingList.Add(dto);

            var updatedJson = JsonSerializer.Serialize(existingList, new JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync(requestStatusPath, updatedJson);

            foreach (var item in requestsToDisplay)
            {
                if (item.RequestRef == externalId)
                {
                    item.currentRequestStatus = DocumentInstanceStatus.IN_PROGRESS;
                }
            }

            return Ok(dto);
        }

        private ExternalStatusUpdateDto UpdateRequest_InProgress(string requestRef)
        {
            return new ExternalStatusUpdateDto
            {
                StatusUpdateRef = Guid.NewGuid(),
                Message = "Захтев је примљен у ЕД Нови Београд",
                Timestamp = DateTime.UtcNow,
                Status = DocumentInstanceStatus.IN_PROGRESS,
                RequestRef = Guid.Parse(requestRef)
            };
        }

        /* 
         NEED_EXTENSION [status: 4]
         * getSpecificNeedExtension - reading and updating request, search by request ref and simulate changing status to 'NEED_EXTENSION'
         * UpdateRequest_NeedExtension - return ExternalStatusUpdateDto with updated status to NEED_EXTENSION, message, timestamp, ...
        */

        [HttpGet("getSpecificNeedExtension")]
        public async Task<IActionResult> GetSpecificRequestNeedExtension(string externalId)
        {
            var requests = ReadRequestsFromJson();
            
            var request = requests.FirstOrDefault(r => r.RequestRef == externalId);
            
            if (request == null)
            {
                return NotFound(new { Message = "Request not found." });
            }
            
            var dto = UpdateRequest_NeedExtension(externalId);

            var requestStatusPath = Path.Combine(Directory.GetCurrentDirectory(), "requestStatusList.json");

            List<ExternalStatusUpdateDto> existingList = new();

            if (System.IO.File.Exists(requestStatusPath))
            {
                var existingJson = await System.IO.File.ReadAllTextAsync(requestStatusPath);
                existingList = JsonSerializer.Deserialize<List<ExternalStatusUpdateDto>>(existingJson) ?? new List<ExternalStatusUpdateDto>();
            }

            existingList.Add(dto);

            var updatedJson = JsonSerializer.Serialize(existingList, new JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync(requestStatusPath, updatedJson);

            foreach (var item in requestsToDisplay)
            {
                if (item.RequestRef == externalId)
                {
                    item.currentRequestStatus = DocumentInstanceStatus.NEED_EXTENSION;
                }
            }

            return Ok(dto);
        }

        private ExternalStatusUpdateDto UpdateRequest_NeedExtension(string requestRef, List<ExternalStatusUpdateAttachmentDto> statusAttachments = null)
        {
            var statusAttachmentFiles = statusAttachments ?? new List<ExternalStatusUpdateAttachmentDto>();

            return new ExternalStatusUpdateDto
            {
                StatusUpdateRef = Guid.NewGuid(),
                Message = "Потребна допуна захтева",
                Timestamp = DateTime.UtcNow,
                Status = DocumentInstanceStatus.NEED_EXTENSION,
                RequestRef = Guid.Parse(requestRef),
                AttachmentList = statusAttachmentFiles
            };
        }

        /* 
         APPROVED [status: 5]
         * getSpecificApproved - reading and updating request, search by request ref and simulate changing status to 'APPROVED'
         * UpdateRequest_Approved - return ExternalStatusUpdateDto with updated status to APPROVED, message, timestamp, ...
        */

        [HttpGet("getSpecificApproved")]
        public async Task<IActionResult> GetSpecificRequestApproved(string externalId)
        {
            var requests = ReadRequestsFromJson();
            
            var request = requests.FirstOrDefault(r => r.RequestRef == externalId);
            
            if (request == null)
            {
                return NotFound(new { Message = "Request not found." });
            }
            
            var dto = UpdateRequest_Approved(externalId);

            var requestStatusUpdatePath = Path.Combine(Directory.GetCurrentDirectory(), "requestStatusList.json");

            List<ExternalStatusUpdateDto> existingList = new();

            if (System.IO.File.Exists(requestStatusUpdatePath))
            {
                var existingJson = await System.IO.File.ReadAllTextAsync(requestStatusUpdatePath);
                existingList = JsonSerializer.Deserialize<List<ExternalStatusUpdateDto>>(existingJson) ?? new List<ExternalStatusUpdateDto>();
            }

            existingList.Add(dto);

            var updatedJson = JsonSerializer.Serialize(existingList, new JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync(requestStatusUpdatePath, updatedJson);

            foreach (var item in requestsToDisplay)
            {
                if (item.RequestRef == externalId)
                {
                    item.currentRequestStatus = DocumentInstanceStatus.APPROVED;
                }
            }

            return Ok(dto);
        }
        
        private ExternalStatusUpdateDto UpdateRequest_Approved(string requestRef)
        {
            return new ExternalStatusUpdateDto
            {
                StatusUpdateRef = Guid.NewGuid(),
                Message = "Издато одобрење захтева",
                Timestamp = DateTime.UtcNow,
                Status = DocumentInstanceStatus.APPROVED,
                RequestRef = Guid.Parse(requestRef)
            };
        }

        // * ReadRequestsFromJson - Reads and deserializes the JSON file containing all integration requests into a list of IntegrationRequestV2Dto objects.
        //                          Handles file reading and JSON parsing errors gracefully by returning an empty list.
        private List<IntegrationRequestV2Dto> ReadRequestsFromJson()
        {
            try
            {
                string json = System.IO.File.ReadAllText(_jsonRequestListPath);
                return JsonSerializer.Deserialize<List<IntegrationRequestV2Dto>>(json) ?? new List<IntegrationRequestV2Dto>();
            }
            catch
            {
                return new List<IntegrationRequestV2Dto>();
            }
        }

        private List<ExternalStatusUpdateDto> ReadRequestStatusFromJson()
        {
            try
            {
                string json = System.IO.File.ReadAllText(_jsonRequestStatusListPath);
                return JsonSerializer.Deserialize<List<ExternalStatusUpdateDto>>(json) ?? new List<ExternalStatusUpdateDto>();
            }
            catch
            {
                return new List<ExternalStatusUpdateDto>();
            }
        }

        // * AuthorizeRequest - Validates the Authorization header in the request against the configured bearer token.
        //                      Returns appropriate IActionResult for missing, malformed, or invalid tokens.
        private async Task WriteRequestsToJson(List<IntegrationRequestV2Dto> requests)
        {
            try
            {
                string json = JsonSerializer.Serialize(requests, new JsonSerializerOptions { WriteIndented = true });
                await System.IO.File.WriteAllTextAsync(_jsonRequestListPath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to write to JSON file: {ex.Message}");
            }
        }

        // * WriteRequestsToJson - Serializes a list of IntegrationRequestV2Dto objects to JSON and writes them to the request list file.
        //                         Throws an exception if the write operation fails.
        private IActionResult AuthorizeRequest()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return Unauthorized("Authorization header is missing.");

            var authHeader = Request.Headers["Authorization"].ToString();

            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized("Invalid Authorization header format.");

            var token = authHeader.Substring("Bearer ".Length).Trim();

            if (token != _config["IntegrationBearerToken"])
                return Unauthorized("Invalid or expired token.");

            return Ok();
        }

        // * GetStatusUpdates - Retrieves a paginated list of status updates for requests, simulating different status changes
        //                      (APPROVED, IN_PROGRESS, NEED_EXTENSION) in a round-robin pattern for demonstration purposes.
        [HttpGet("getStatusUpdates")]
        public async Task<IActionResult> GetStatusUpdates(int pageSize = 10, int page = 1, string? statusUpdateRef = null)
        {
            try
            {
                var allStatusUpdates = ReadRequestStatusFromJson();

                // If statusUpdateRef is provided
                if (!string.IsNullOrEmpty(statusUpdateRef))
                {
                    var specificUpdate = allStatusUpdates.FirstOrDefault(u =>
                        u.StatusUpdateRef.ToString().Equals(statusUpdateRef, StringComparison.OrdinalIgnoreCase));

                    if (specificUpdate == null)
                    {
                        return NotFound(new { Message = "Status update with the provided reference not found." });
                    }

                    // Get this update and all updates with timestamps after it for the same request
                    var subsequentUpdates = allStatusUpdates
                        .Where(u => u.RequestRef == specificUpdate.RequestRef &&
                                   (u.StatusUpdateRef == specificUpdate.StatusUpdateRef ||
                                    u.Timestamp >= specificUpdate.Timestamp))
                        .OrderBy(u => u.Timestamp)
                        .ToList();

                    // Update snapshots logic
                    var existingStatusSnapshots = ReadStatusSnapshotsFromJson();
                    var updatedStatusSnapshots = new List<ExternalStatusUpdateDto>(existingStatusSnapshots);

                    foreach (var update in subsequentUpdates)
                    {
                        // Check if this update already exists in snapshots
                        var existingSnapshot = updatedStatusSnapshots.FirstOrDefault(s =>
                            s.StatusUpdateRef == update.StatusUpdateRef);

                        if (existingSnapshot != null)
                        {
                            // Update existing snapshot
                            updatedStatusSnapshots.Remove(existingSnapshot);
                            updatedStatusSnapshots.Add(update);
                        }
                        else
                        {
                            // Add new snapshot
                            updatedStatusSnapshots.Add(update);
                        }
                    }

                    // Save updated snapshots
                    await SaveStatusSnapshotsToJsonAsync(updatedStatusSnapshots);
                    return Ok(subsequentUpdates);
                }

                // Regular pagination logic for when no statusUpdateRef is provided
                var requests = allStatusUpdates
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Read existing status snapshots from file
                var existingSnapshots = ReadStatusSnapshotsFromJson();
                var updatedSnapshots = new List<ExternalStatusUpdateDto>(existingSnapshots);

                // Process all updates in the current page
                foreach (var statusUpdate in requests)
                {
                    // Check if this update already exists in snapshots
                    var existingIndex = updatedSnapshots.FindIndex(s =>
                        s.StatusUpdateRef == statusUpdate.StatusUpdateRef);

                    if (existingIndex >= 0)
                    {
                        // Update existing snapshot
                        updatedSnapshots[existingIndex] = statusUpdate;
                    }
                    else
                    {
                        // Add new snapshot
                        updatedSnapshots.Add(statusUpdate);
                    }
                }

                await SaveStatusSnapshotsToJsonAsync(updatedSnapshots);

                return Ok(requests);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating status updates: {ex}");
                return StatusCode(500, new { Message = $"Error generating status updates: {ex.Message}" });
            }
        }

        /*[HttpGet("getStatusUpdates")]
        public async Task<IActionResult> GetStatusUpdates(int pageSize = 10, int page = 1, string? statusUpdateRef = null)
        {
            try
            {
                // Get paginated requests from requestStatusList.json
                var allStatusUpdates = ReadRequestStatusFromJson();
                var requests = allStatusUpdates
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Read existing status snapshots from file
                var existingSnapshots = ReadStatusSnapshotsFromJson();
                var updatedSnapshots = new List<ExternalStatusUpdateDto>(existingSnapshots);

                // If a specific statusUpdateRef is provided, find that specific update
                if (!string.IsNullOrEmpty(statusUpdateRef))
                {
                    var specificUpdate = allStatusUpdates.FirstOrDefault(u =>
                        u.StatusUpdateRef.ToString().Equals(statusUpdateRef, StringComparison.OrdinalIgnoreCase));

                    if (specificUpdate == null)
                    {
                        return NotFound(new { Message = "Status update with the provided reference not found." });
                    }

                    // Check if this update already exists in snapshots
                    var existingSnapshot = updatedSnapshots.FirstOrDefault(s =>
                        s.StatusUpdateRef == specificUpdate.StatusUpdateRef);

                    if (existingSnapshot != null)
                    {
                        // Update existing snapshot
                        updatedSnapshots.Remove(existingSnapshot);
                        updatedSnapshots.Add(specificUpdate);
                    }
                    else
                    {
                        // Add new snapshot
                        updatedSnapshots.Add(specificUpdate);
                    }

                    // Save updated snapshots
                    await SaveStatusSnapshotsToJsonAsync(updatedSnapshots);
                    return Ok(new List<ExternalStatusUpdateDto> { specificUpdate });
                }

                // Process all updates in the current page
                foreach (var statusUpdate in requests)
                {
                    // Check if this update already exists in snapshots
                    var existingIndex = updatedSnapshots.FindIndex(s =>
                        s.StatusUpdateRef == statusUpdate.StatusUpdateRef);

                    if (existingIndex >= 0)
                    {
                        // Update existing snapshot
                        updatedSnapshots[existingIndex] = statusUpdate;
                    }
                    else
                    {
                        // Add new snapshot
                        updatedSnapshots.Add(statusUpdate);
                    }
                }

                // Save updated snapshots back to file
                await SaveStatusSnapshotsToJsonAsync(updatedSnapshots);

                return Ok(requests);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating status updates: {ex}");
                return StatusCode(500, new { Message = $"Error generating status updates: {ex.Message}" });
            }
        }*/

        // Read status snapshots from the statusSnapshots.json file
        private List<ExternalStatusUpdateDto> ReadStatusSnapshotsFromJson()
        {
            try
            {
                if (!System.IO.File.Exists(_jsonStatusSnapshotsFilePath))
                {
                    // Create directory if it doesn't exist
                    Directory.CreateDirectory(Path.GetDirectoryName(_jsonStatusSnapshotsFilePath));

                    // Create empty file if it doesn't exist
                    System.IO.File.WriteAllText(_jsonStatusSnapshotsFilePath, "[]");
                    return new List<ExternalStatusUpdateDto
                        >();
                }

                string jsonContent = System.IO.File.ReadAllText(_jsonStatusSnapshotsFilePath);

                if (string.IsNullOrEmpty(jsonContent))
                {
                    return new List<ExternalStatusUpdateDto>();
                }

                return JsonSerializer.Deserialize<List<ExternalStatusUpdateDto>>(jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading status snapshots: {ex}");
                return new List<ExternalStatusUpdateDto>();
            }
        }

        // Save status snapshots to the statusSnapshots.json file
        private async Task SaveStatusSnapshotsToJsonAsync(List<ExternalStatusUpdateDto> snapshots)
        {
            try
            {
                // Create directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(_jsonStatusSnapshotsFilePath));

                string json = JsonSerializer.Serialize(snapshots, new JsonSerializerOptions { WriteIndented = true });
                await System.IO.File.WriteAllTextAsync(_jsonStatusSnapshotsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving status snapshots: {ex}");
                throw new Exception($"Failed to save status snapshots: {ex.Message}");
            }
        }

        /*
        // Method that converts status code to status representation
        private string ConvertToExternalStatus(int statusCode)
        {
            switch (statusCode)
            {
                case 2:
                    return "SENT";
                case 3:
                    return "IN_PROGRESS";
                case 4:
                    return "NEED_EXTENSION";
                case 5:
                    return "APPROVED";
                default:
                    return "Unknown";
            }
        }

        [HttpGet("getStatusUpdates")]
        public async Task<IActionResult> GetStatusUpdates(int pageSize, int page, DateTime? limit)
        {
            var requests = ReadRequestsFromJson()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var statusUpdateDtos = new List<ExternalStatusUpdateDto>();

            for (int i = 0; i < requests.Count; i++)
            {
                if (i % 3 == 0)
                {
                    statusUpdateDtos.Add(UpdateRequest_Approved(requests[i].RequestRef));
                }
                else if (i % 3 == 1)
                {
                    statusUpdateDtos.Add(UpdateRequest_InProgress(requests[i].RequestRef));
                }
                else
                {
                    statusUpdateDtos.Add(UpdateRequest_NeedExtension(requests[i].RequestRef));
                }
            }

            return Ok(statusUpdateDtos);
        }
        */

        // * ExtendRequest - Handles request extension submissions by validating the extension payload.
        //                   Currently just checks if extension attachments exist and returns success/failure accordingly.
        [HttpPost("extendRequest")]
        public async Task<IActionResult> ExtendRequest([FromBody] ExtendStatusUpdateDto request)
        {
            if (request.ExtendAttachmentList.Count > 0 && request is not null)
            {
                return Ok();
            }

            return BadRequest();
        }

        // * SendRequest - Processes new integration requests by validating authorization, checking for required fields,
        //                 and adding/updating the request in the JSON storage. Returns appropriate status updates.
        //                 Includes comprehensive error handling for various failure scenarios.
        [HttpPost("sendRequest")]
        public async Task<IActionResult> SendRequest([FromBody] IntegrationRequestV2Dto request)
        {
            var authorizationResult = AuthorizeRequest();
            if (authorizationResult is UnauthorizedResult)
                return authorizationResult;

            if (request == null)
            {
                return BadRequest(new ResponsePackageNoData(ResponseStatus.BadRequest, "Request cannot be null."));
            }

            if (string.IsNullOrEmpty(request.RequestRef))
            {
                return BadRequest(new ResponsePackageNoData(ResponseStatus.BadRequest, "RequestRef is required."));
            }

            try
            {
                // Read existing requests
                var requests = await Task.Run(() => ReadRequestsFromJson());

                // Check if request already exists
                var existingIndex = requests.FindIndex(r => r.RequestRef == request.RequestRef);
                if (existingIndex >= 0)
                {
                    requests[existingIndex] = request; // Update existing request
                }
                else
                {
                    requests.Add(request); // Add new request
                }

                // Save updated requests
                await WriteRequestsToJson(requests);

                // Create status update
                var responseDto = new ExternalStatusUpdateDto
                {
                    StatusUpdateRef = Guid.NewGuid(),
                    Message = "Захтев је поднет ка ЕД Нови Београд",
                    Timestamp = DateTime.UtcNow,
                    Status = DocumentInstanceStatus.SENT,
                    RequestRef = Guid.Parse(request.RequestRef)
                };

                // Read existing status updates
                var statusUpdates = ReadRequestStatusFromJson();

                // Add new status update
                statusUpdates.Add(responseDto);

                // Save updated status updates
                await System.IO.File.WriteAllTextAsync(
                    _jsonRequestStatusListPath,
                    JsonSerializer.Serialize(statusUpdates, new JsonSerializerOptions { WriteIndented = true })
                );

                return Ok(responseDto);
            }
            catch (FormatException ex)
            {
                return BadRequest(new ResponsePackageNoData(
                    ResponseStatus.BadRequest,
                    $"Invalid GUID format in RequestRef: {ex.Message}"
                ));
            }
            catch (FileNotFoundException ex)
            {
                return StatusCode((int)ResponseStatus.InternalServerError, new ResponsePackageNoData(
                    ResponseStatus.InternalServerError,
                    $"Request storage file not found: {ex.Message}"
                ));
            }
            catch (JsonException ex)
            {
                return StatusCode((int)ResponseStatus.InternalServerError, new ResponsePackageNoData(
                    ResponseStatus.InternalServerError,
                    $"Error parsing request data: {ex.Message}"
                ));
            }
            catch (IOException ex)
            {
                return StatusCode((int)ResponseStatus.InternalServerError, new ResponsePackageNoData(
                    ResponseStatus.InternalServerError,
                    $"File access error: {ex.Message}"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode((int)ResponseStatus.InternalServerError, new ResponsePackageNoData(
                    ResponseStatus.InternalServerError,
                    $"Error processing request: {ex.Message}"
                ));
            }
        }

        // * UploadFile - Handles file uploads for request extensions, saving files with unique names to prevent conflicts.
        //                Returns a NEED_EXTENSION status update with attachment information.
        //                Validates input parameters and handles file system operations safely.
        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile()
        {
            try
            {
                var form = await Request.ReadFormAsync();
                var files = form.Files;
                var externalId = form["externalId"].ToString();

                if (string.IsNullOrEmpty(externalId))
                {
                    return BadRequest(new { Message = "Request reference is required." });
                }

                if (files == null || files.Count == 0)
                {
                    return BadRequest(new { Message = "No file was uploaded." });
                }

                if (!Directory.Exists(_uploadFolderPath))
                {
                    Directory.CreateDirectory(_uploadFolderPath);
                }

                var uploadedFiles = new List<ExternalStatusUpdateAttachmentDto>();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        string uniqueNameOfFile = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
                        string saveFilePath = Path.Combine(_uploadFolderPath, uniqueNameOfFile);

                        using (var fileStream = new FileStream(saveFilePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        uploadedFiles.Add(new ExternalStatusUpdateAttachmentDto
                        {
                            Name = file.FileName,
                            Path = Path.Combine("Uploads", uniqueNameOfFile),
                        });
                    }
                }

                var dto = UpdateRequest_NeedExtension(externalId, uploadedFiles);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File upload error: {ex}");
                return StatusCode(500, new { Message = $"Error uploading file: {ex.Message}" });
            }
        }

        // * DownloadFile - Serves previously uploaded files for download based on the provided file path.
        //                  Supports both HEAD (for existence checking) and GET methods.
        //                  Validates input and handles file operations safely.
        [HttpGet, HttpHead]
        [Route("Download")]
        public async Task<IActionResult> DownloadFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return BadRequest("File path is required.");

                string fileName = Path.GetFileName(filePath);

                var fullPath = Path.Combine(_uploadFolderPath, fileName);

                if (!System.IO.File.Exists(fullPath))
                    return NotFound("File not found.");

                if (HttpContext.Request.Method == "HEAD")
                    return Ok();

                var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
