using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SignalServer.Devices;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SignalServer.Core
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EventHub : Hub
    {
        private readonly ILogger<EventHub> _logger;
        private readonly IDeviceManager _deviceManager;
        private ConnectionManagementService _connectionManagementService;

        public EventHub(ILogger<EventHub> logger, IDeviceManager deviceManager, ConnectionManagementService connectionManagementService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));
            _connectionManagementService = connectionManagementService;
        }

        public Task Send(string message)
        {
            var username = Context.User.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
            var fromDeviceId = Context.User.Claims.FirstOrDefault(c => c.Type == "device_id")?.Value;
            _logger.LogInformation($"{nameof(Send)} called. ConnectionId:{Context.ConnectionId}, Name:{username}, Message:{message}");
            return Clients.OthersInGroup(Guid.Empty.ToString()).SendAsync("BroadcastMessage", BroadcastScopes.All, username, fromDeviceId, message);
        }

        [HubMethodName("send_to_device")]
        public Task SendToDevice(string userId, string deviceId, string message)
        {
            var fromUserId = Context.User.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
            var fromDeviceId = Context.User.Claims.FirstOrDefault(c => c.Type == "device_id")?.Value;
            _logger.LogInformation($"{nameof(SendToDevice)} called. ConnectionId:{Context.ConnectionId}, Name:{fromUserId}, Message:{message}");
            if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(deviceId))
            {
                var connectionId = _deviceManager.FindConnectionId(userId, deviceId);
                if (!string.IsNullOrWhiteSpace(connectionId))
                {
                    Console.WriteLine($"{fromUserId} send \"{message}\" to connection {connectionId} ({userId})");
                    return Clients.Client(connectionId).SendAsync("UnicastMessage", fromUserId, fromDeviceId, message);
                }
            }
            else if (!string.IsNullOrWhiteSpace(userId))
            {
                if (string.Compare(userId, fromUserId) == 0)
                {
                    Console.WriteLine($"{fromUserId} send \"{message}\" to my other devices ({userId})");
                    return Clients.OthersInGroup($"User-{userId}").SendAsync("BroadcastMessage", BroadcastScopes.MyDevices, fromUserId, fromDeviceId, message);
                }
                else
                {
                    Console.WriteLine($"{fromUserId} send \"{message}\" to given user devices ({userId})");
                    return Clients.Group($"User-{userId}").SendAsync("BroadcastMessage", BroadcastScopes.MyDevices, fromUserId, fromDeviceId, message);
                }
            }
            else if (!string.IsNullOrWhiteSpace(deviceId))
            {
                var connectionId = _deviceManager.FindConnectionId(fromUserId, deviceId);
                if (!string.IsNullOrWhiteSpace(connectionId))
                {
                    Console.WriteLine($"{fromUserId} send \"{message}\" to self device, c={connectionId} u={userId}");
                    return Clients.Client(connectionId).SendAsync("UnicastMessage", fromUserId, fromDeviceId, message);
                }
            }
            return Task.CompletedTask;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"{nameof(OnConnectedAsync)} called.");

            _connectionManagementService.InitConnectionMonitoring(Context);

            var userId = Context.User.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
            var deviceId = Context.User.Claims.FirstOrDefault(c => c.Type == "device_id")?.Value;
            var oldConnectionId = _deviceManager.AddConnection(Context.ConnectionId, userId, deviceId);

            if (oldConnectionId != null)
            {
                Console.WriteLine($"Send conflict to {oldConnectionId}");
                await Clients.Client(oldConnectionId).SendAsync("SystemMessage", "session_conflict");
                await Groups.RemoveFromGroupAsync(oldConnectionId, Guid.Empty.ToString());
                await Groups.RemoveFromGroupAsync(oldConnectionId, $"User-{userId}");
                _connectionManagementService.AbortConnection(oldConnectionId);
            }

            await base.OnConnectedAsync();
            await Groups.AddToGroupAsync(Context.ConnectionId, Guid.Empty.ToString());
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User-{userId}");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation(exception, $"{nameof(OnDisconnectedAsync)} called.");

            var userId = Context.User.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
            _deviceManager.RemoveConnection(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Guid.Empty.ToString());
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User-{userId}");
        }
    }
}
