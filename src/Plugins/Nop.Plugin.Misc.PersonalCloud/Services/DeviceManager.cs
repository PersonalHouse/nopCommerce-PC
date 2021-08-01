using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Data;
using Nop.Plugin.Misc.PersonalCloud.Domain;
using Nop.Services.Customers;
using SignalServer.Devices;

namespace Nop.Plugin.Misc.PersonalCloud.Services
{
    public class DeviceManager : IDeviceManager
    {
        private readonly ICustomerService _customerService;
        private readonly IRepository<CustomerDevice> _customerDeviceRepository;

        private Dictionary<string, string> _activeUserDevices = new Dictionary<string, string>();
        private Dictionary<string, string> _connectionIds = new Dictionary<string, string>();

        public DeviceManager(ICustomerService customerService, IRepository<CustomerDevice> customerDeviceRepository)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _customerDeviceRepository = customerDeviceRepository ?? throw new ArgumentNullException(nameof(customerDeviceRepository));
        }

        public string RegisterDevice(string userId, string deviceId)
        {
            var customer = _customerService.GetCustomerByUsername(userId);
            if (customer != null)
            {
                var customerDevice = _customerDeviceRepository.Table
                    .Where(x => x.CustomerId == customer.Id && x.Identity == deviceId).FirstOrDefault();
                if (customerDevice != null)
                {
                    customerDevice.RefreshToken = Guid.NewGuid().ToString("N").ToLower();
                    _customerDeviceRepository.Update(customerDevice);
                }
                else
                {
                    customerDevice = new CustomerDevice
                    {
                        CustomerId = customer.Id,
                        Identity = deviceId,
                        RefreshToken = Guid.NewGuid().ToString("N").ToLower(),
                        DeviceInfo = "" // No Allow Null
                    };
                    _customerDeviceRepository.Insert(customerDevice);
                }
                return customerDevice.RefreshToken;
            }
            else
            {
                throw new Exception("No such customer!");
            }
        }

        public void UnregisterDevice(string userId, string deviceId)
        {
            var customer = _customerService.GetCustomerByUsername(userId);
            if (customer != null)
            {
                var customerDevice = _customerDeviceRepository.Table
                    .Where(x => x.CustomerId == customer.Id && x.Identity == deviceId).FirstOrDefault();
                if (customerDevice != null)
                {
                    _customerDeviceRepository.Delete(customerDevice);
                }
            }
        }

        public bool VerifyDeviceRefreshToken(string userId, string deviceId, string refreshToken)
        {
            var customer = _customerService.GetCustomerByUsername(userId);
            if (customer != null)
            {
                var userDevice = _customerDeviceRepository.Table
                    .Where(x => x.CustomerId == customer.Id && x.Identity == deviceId).FirstOrDefault();
                if (userDevice != null)
                {
                    return (userDevice.RefreshToken == refreshToken);
                }
            }
            return false;
        }

        public string AddConnection(string connectionId, string userId, string deviceId)
        {
            var customer = _customerService.GetCustomerByUsername(userId);
            if (customer != null)
            {
                var userDevice = _customerDeviceRepository.Table
                    .Where(x => x.CustomerId == customer.Id && x.Identity == deviceId)
                    .FirstOrDefault();
                if (userDevice != null)
                {
                    var key = $"{userId}-{deviceId}";
                    lock (_connectionIds)
                    {
                        string oldConnectionId = null;
                        if (_activeUserDevices.ContainsKey(key))
                        {
                            oldConnectionId = _activeUserDevices[key];
                            // unlink connectionId with user's deviceId, otherwise,
                            // new device will be removed when old connection get kick out.
                            _connectionIds[oldConnectionId] = null;
                        }
                        _activeUserDevices[key] = connectionId;
                        _connectionIds[connectionId] = key;
#if DEBUG
                        DumpConnections();
#endif
                        return oldConnectionId;
                    }
                }
                else
                {
                    // Invalid DeviceId (May be deleted)
                    throw new ArgumentException("Invalid Device Identity");
                }
            }
            else
            {
                throw new Exception("No such customer!");
            }
        }

        public void RemoveConnection(string connectionId)
        {
            lock (_connectionIds)
            {
                if (_connectionIds.ContainsKey(connectionId))
                {
                    var deviceId = _connectionIds[connectionId];
                    if (!string.IsNullOrWhiteSpace(deviceId))
                    {
                        // For conflicted device, the corresponding deviceId will be set to null
                        _activeUserDevices.Remove(_connectionIds[connectionId]);
                    }
                    _connectionIds.Remove(connectionId);
                }
#if DEBUG
                DumpConnections();
#endif
            }
        }

        public List<string> GetActiveDeviceListByUserId(string userId)
        {
            lock (_connectionIds)
            {
                List<string> deviceIds = new List<string>();
                var prefix = $"{userId}-";
                foreach (var item in _activeUserDevices)
                {
                    if (item.Key.StartsWith(prefix))
                    {
                        deviceIds.Add(item.Key.Substring(prefix.Length));
                    }
                }
                return deviceIds;
            }
        }

        public string FindConnectionId(string userId, string deviceId)
        {
            var key = $"{userId}-{deviceId}";
            lock (_connectionIds)
            {
                if (_activeUserDevices.ContainsKey(key))
                {
                    return _activeUserDevices[key];
                }
                return null;
            }
        }

        private void DumpConnections()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Connections:");
            foreach (var item in _connectionIds)
            {
                sb.AppendLine($"  {item.Key}: {item.Value}");
            }
            Console.WriteLine(sb.ToString());
        }
    }
}
