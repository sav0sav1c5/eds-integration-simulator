using Microsoft.AspNetCore.Mvc;
using System.Net;
using Sotex.EDSPortal.IntegrationSimulation.SharedDTOs;

namespace Sotex.EDSPortal.IntegrationSimulation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BlobController : ControllerBase
{
    private readonly string _uploadFolderPath;
    private readonly string _jsonRequestStatusPath;
    private readonly string _jsonRequestStatusUpdatePath;
    private readonly IConfiguration _config;

    public BlobController(IConfiguration config, IHostEnvironment env)
    {

        if (env.IsDevelopment())
        {
            _uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            _jsonRequestStatusPath = Path.Combine(Directory.GetCurrentDirectory(), "requestList.json");
            _jsonRequestStatusUpdatePath = Path.Combine(Directory.GetCurrentDirectory(), "requestStatusList.json");
        }
        else
        {
            _uploadFolderPath = "/var/www/sim-uploads";
            _jsonRequestStatusPath = "/var/www/sim-uploads/requestList.json";
            _jsonRequestStatusUpdatePath = "/var/www/sim-uploads/requestStatusList.json";
        }

        _config = config;

        if (!Directory.Exists(_uploadFolderPath))
        {
            Directory.CreateDirectory(_uploadFolderPath);
        }

        if (!System.IO.File.Exists(_jsonRequestStatusPath))
        {
            System.IO.File.WriteAllText(_jsonRequestStatusPath, "[]");
        }

        if (!System.IO.File.Exists(_jsonRequestStatusUpdatePath))
        {
            System.IO.File.WriteAllText(_jsonRequestStatusUpdatePath, "[]");
        }
    }

    // Method with logic to detect problem scenarios with authorization header
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

    // Method with logic to detect service unavailable scenarios
    private bool IsServiceUnavailableError(Exception ex)
    {
        return ex is HttpRequestException { StatusCode: HttpStatusCode.ServiceUnavailable }
               || ex.Message.Contains("service unavailable", StringComparison.OrdinalIgnoreCase);
    }

    // Method that is used for uploading multiple files required if there is need for extension
    [HttpPost("upload")]
    public async Task<ActionResult<ResponsePackage<string>>> UploadFile()
    {
        try
        {
            var authorizationResult = AuthorizeRequest();
            if (authorizationResult is UnauthorizedResult)
            {
                return Unauthorized(new ResponsePackage<string>(
                    ResponseStatus.Unauthorized,
                    "Authentication failed. Please provide valid credentials."
                ));
            }

            if (Request.Form.Files.Count == 0)
            {
                return BadRequest(new ResponsePackage<string>(
                    ResponseStatus.BadRequest,
                    "No file was uploaded. Please select a file to upload."
                ));
            }

            var file = Request.Form.Files[0];

            if (file.Length == 0)
            {
                return BadRequest(new ResponsePackage<string>(
                    ResponseStatus.BadRequest,
                    "The uploaded file is empty."
                ));
            }

            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new ResponsePackage<string>(
                    ResponseStatus.BadRequest,
                    "File size exceeds the 10MB limit."
                ));
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new ResponsePackage<string>(
                    ResponseStatus.BadRequest,
                    $"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}"
                ));
            }

            var fileName = Path.GetFileName(file.FileName);
            var tempFilePath = Path.Combine(_uploadFolderPath, fileName);
            if (System.IO.File.Exists(tempFilePath))
            {
                return Conflict(new ResponsePackage<string>(
                    ResponseStatus.Conflict,
                    "A file with this name already exists."
                ));
            }

            var uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_" +
                                $"{Guid.NewGuid().ToString().Substring(0, 8)}" +
                                $"{Path.GetExtension(fileName)}";

            var filePath = Path.Combine(_uploadFolderPath, uniqueFileName);

            try
            {
                Directory.CreateDirectory(_uploadFolderPath);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ResponsePackage<string>(
                        ResponseStatus.Forbidden,
                        "Insufficient permissions to create upload directory."
                    ));
            }

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { message = "File uploaded successfully.", filePath = uniqueFileName });
        }
        catch (IOException ioEx)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ResponsePackage<string>(
                    ResponseStatus.InternalServerError,
                    $"File storage error: {ioEx.Message}"
                ));
        }
        catch (Exception ex) when (IsServiceUnavailableError(ex))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new ResponsePackage<string>(
                    ResponseStatus.ServiceUnavailable,
                    "Service temporarily unavailable. Please try again later."
                ));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ResponsePackage<string>(
                    ResponseStatus.InternalServerError,
                    $"An unexpected error occurred during file upload: {ex.Message}"
                ));
        }
    }

    // Method that use relative filePath, find file and return it as downloadable file
    [HttpGet("download")]
    public async Task<IActionResult> DownloadFile(string filePath) 
    {
        try
        {
            var authorizationResult = AuthorizeRequest();
            if (authorizationResult is UnauthorizedResult)
                return authorizationResult;

            if (string.IsNullOrEmpty(filePath))
                return BadRequest("File path is required.");

            var fullPath = Path.Combine(_uploadFolderPath, filePath);

            if (!System.IO.File.Exists(fullPath))
                return NotFound("File not found.");

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

            return File(stream, "application/octet-stream", Path.GetFileName(fullPath));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
