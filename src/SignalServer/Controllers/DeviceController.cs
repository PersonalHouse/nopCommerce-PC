using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SignalChannel.Common;
using SignalServer.Devices;
using System;
using System.Linq;

namespace SignalServer.Core
{
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IDeviceManager _deviceManager;

        public DeviceController(IDeviceManager deviceManager, ILogger<AuthController> logger)
        {
            _logger = logger;
            _deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("devices/active")]
        public IActionResult GetActiveDevices()
        {
            try
            {
                var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    return Ok(_deviceManager.GetActiveDeviceListByUserId(userId)
                        .Select(x => new DeviceInfo { DeviceId = x }));
                }
                return BadRequest(ApiErrors.Invalid_Grant("Invalid token, username is missing"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal Error");
                return BadRequest(ApiErrors.Fatal_Error("Fatal Error"));
            }
        }
    }
}
