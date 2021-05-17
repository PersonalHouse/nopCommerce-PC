using System;
using System.Threading.Tasks;

namespace Nop.Web
{
    public interface IWebServerData
    {
        IServiceProvider ServiceProvider { get; set; }

        TaskCompletionSource<bool> MainWebStarted { get; }

        TaskCompletionSource<bool> SignalServerStopped { get; }
    }

    public class WebServerData : IWebServerData
    {
        private TaskCompletionSource<bool> _mainWebStarted = new TaskCompletionSource<bool>();

        private TaskCompletionSource<bool> _signalServerStopped = new TaskCompletionSource<bool>();

        public IServiceProvider ServiceProvider { get; set; }

        public TaskCompletionSource<bool> MainWebStarted { get => _mainWebStarted; }

        public TaskCompletionSource<bool> SignalServerStopped { get => _signalServerStopped; }
    }
}
