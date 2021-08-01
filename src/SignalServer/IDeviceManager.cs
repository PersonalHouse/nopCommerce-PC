using System.Collections.Generic;

namespace SignalServer.Devices
{
    public interface IDeviceManager
    {
        string RegisterDevice(string userId, string deviceId);

        void UnregisterDevice(string userId, string deviceId);

        bool VerifyDeviceRefreshToken(string userId, string deviceId, string refreshToken);

        string AddConnection(string connectionId, string username, string deviceId);

        void RemoveConnection(string connectionId);

        List<string> GetActiveDeviceListByUserId(string userId);

        string FindConnectionId(string userId, string deviceId);
    }
}
