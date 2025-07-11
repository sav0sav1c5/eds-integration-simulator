using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace Sotex.EDSPortal.IntegrationSimulation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IntegrationController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IntegrationController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        // Attachment Types
        [HttpGet("attachment-types")]
        public async Task<IActionResult> GetAttachmentTypes()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.attachment_types.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Building Purpose
        [HttpGet("building-purpose")]
        public async Task<IActionResult> GetBuildingPurpose()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.building_purpose.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Building Type
        [HttpGet("building-type")]
        public async Task<IActionResult> GetBuildingType()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.building_types.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Building Unit Purpose
        [HttpGet("build-unit-purpose")]
        public async Task<IActionResult> GetBuildingUnitPurpose()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.building_unit_purpose.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Cadastral Municipalities
        [HttpGet("cadastal-municipalities")]
        public async Task<IActionResult> GetCadastalMunicipalities()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.cadastral_municipalities.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Connection Durations
        [HttpGet("connection-durations")]
        public async Task<IActionResult> GetConnectionDurations()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.connection_durations.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Connection Powers
        [HttpGet("connection-powers")]
        public async Task<IActionResult> GetConnectionPowers()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.connection_powers.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Connection Types
        [HttpGet("connection-types")]
        public async Task<IActionResult> GetConnectionTypes()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.connection_types.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Construction Types
        [HttpGet("construction-types")]
        public async Task<IActionResult> GetConstructionTypes()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.construction_types.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Existing Instalation
        [HttpGet("existing-instalation")]
        public async Task<IActionResult> GetExistingInstalation()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.existing_instalation.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Heating Types
        [HttpGet("heading-types")]
        public async Task<IActionResult> GetHeatingTypes()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.heating_types.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Meter Classes
        [HttpGet("meter-classes")]
        public async Task<IActionResult> GetMeterClasses()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.meter_classes.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Meter Types
        [HttpGet("meter-types")]
        public async Task<IActionResult> GetMeterTypes()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.meter_types.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Municipalities
        [HttpGet("municipalities")]
        public async Task<IActionResult> GetMunicipalities()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.municipalities.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Power Limit
        [HttpGet("power-limit")]
        public async Task<IActionResult> GetPowerLimit()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.power_limit.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Settlements
        [HttpGet("settlements")]
        public async Task<IActionResult> GetSettlements()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.settlements.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Streets
        [HttpGet("streets")]
        public async Task<IActionResult> GetStreets()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.streets.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
    }

        // Tariff Numbers
        [HttpGet("tariff-numbers")]
        public async Task<IActionResult> GetTariffNumbers()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.tariff_numbers.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        // Voltages
        [HttpGet("voltages")]
        public async Task<IActionResult> GetVoltages()
        {
            // Getting absolute file path
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "SimulatorData", "integration.voltages.json");

            // Checking file exist
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Json file not found.");
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(jsonContent, "application/json");
        }

        /*
        public IActionResult Index()
        {
            return View();
        }
        */
    }
}
