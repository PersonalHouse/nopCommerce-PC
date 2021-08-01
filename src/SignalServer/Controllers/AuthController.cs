using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SignalChannel.Common;
using SignalServer.Devices;
using SignalServer.Users;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace SignalServer.Core
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ISignalServerOptions _signalServerOptions;
        private readonly IUserManager _userManager;
        private readonly IDeviceManager _deviceManager;

        private static ServerInfo _serverInfo = new ServerInfo
        {
            Name = "Signal Server",
            Version = "1"
        };

        public AuthController(ISignalServerOptions signalServerOptions, IUserManager userManager, IDeviceManager deviceManager, ILogger<AuthController> logger)
        {
            _logger = logger;
            _signalServerOptions = signalServerOptions;
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));
        }

        [HttpGet]
        [Route("info")]
        public ServerInfo GetServerInfo() => _serverInfo;

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("userinfo")]
        public IActionResult GetUserInfo()
        {
            try
            {
                var userName = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    return Ok(new { Username = userName });
                }
                return BadRequest(ApiErrors.Invalid_Grant("Invalid token, username is missing"));
            }
            catch (Exception)
            {
                return BadRequest(ApiErrors.Invalid_Grant("Invalid token"));
            }
        }

        [HttpPost]
        [Route("auth")]
        public IActionResult Auth(AuthRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DeviceId))
            {
                return BadRequest(ApiErrors.Invalid_Request("DeviceId is empty"));
            }

            try
            {
                var userInfo = _userManager.CheckUserCredentials(request.Username, request.Password);
                if (userInfo != null)
                {
                    var refreshToken = _deviceManager.RegisterDevice(request.Username, request.DeviceId);

                    return Ok(new AuthResult
                    {
                        AccessToken = GenerateJwtToken(request.Username, request.DeviceId,
                            DateTime.Now.AddSeconds(_signalServerOptions.AccessTokenLifetime)),
                        RefreshToken = refreshToken
                    });
                }
                else
                {
                    return BadRequest(ApiErrors.Invalid_Client("Invalid Username / Password"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal Error");
                return BadRequest(ApiErrors.Fatal_Error("Fatal Error"));
            }
        }

        [HttpPost]
        [Route("token")]
        public IActionResult GetToken(GetTokenRequest request)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(request.AccessToken);
                var userId = jwtToken.Claims.First(claim => claim.Type == "username").Value;
                var deviceId = jwtToken.Claims.First(claim => claim.Type == "device_id").Value;

                if (_deviceManager.VerifyDeviceRefreshToken(userId, deviceId, request.RefreshToken))
                {
                    return Ok(new GetTokenResult
                    {
                        AccessToken = GenerateJwtToken(userId, deviceId, DateTime.Now.AddSeconds(_signalServerOptions.AccessTokenLifetime))
                    });
                }
                else
                {
                    return BadRequest(ApiErrors.Invalid_Grant("Invalid RefreshToken"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal Error");
                return BadRequest(ApiErrors.Fatal_Error("Fatal Error"));
            }
        }

        private string GenerateJwtToken(string username, string deviceId, DateTime expireTime)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("username", username));
            claims.Add(new Claim("device_id", deviceId));

            JwtSecurityToken jwtToken = new JwtSecurityToken
            (
                claims: claims,
                signingCredentials: new SigningCredentials(new ECDsaSecurityKey(_signalServerOptions.SigningKey.GetECDsaPrivateKey()), SecurityAlgorithms.EcdsaSha256),
                expires: expireTime
            );

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(jwtToken);
        }
    }
}
