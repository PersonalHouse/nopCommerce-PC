using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace SignalServer.Core
{
    public class ConnectionManagementService
    {
        readonly HashSet<string> PendingConnections = new HashSet<string>();
        readonly object PendingConnectionsLock = new object();

        public void AbortConnection(string ConnectionId)
        {
            if (!PendingConnections.Contains(ConnectionId))
            {
                lock (PendingConnectionsLock)
                {
                    PendingConnections.Add(ConnectionId);
                }
            }
        }

        public void InitConnectionMonitoring(HubCallerContext Context)
        {
            var feature = Context.Features.Get<IConnectionHeartbeatFeature>();

            feature.OnHeartbeat(state =>
            {
                if (PendingConnections.Contains(Context.ConnectionId))
                {
                    Context.Abort();
                    lock (PendingConnectionsLock)
                    {
                        PendingConnections.Remove(Context.ConnectionId);
                    }
                }
            }, Context.ConnectionId);
        }
    }
}
