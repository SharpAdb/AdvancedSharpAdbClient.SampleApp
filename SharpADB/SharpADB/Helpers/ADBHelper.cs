using AdvancedSharpAdbClient;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SharpADB.Helpers
{
    public static class ADBHelper
    {
        private static DeviceMonitor _monitor;
        public static DeviceMonitor Monitor
        {
            get
            {
                if (_monitor == null && AdbServer.Instance.GetStatus().IsRunning)
                {
                    _monitor = new(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
                    _monitor.Start();
                }
                return _monitor;
            }
        }

        public static async Task<DeviceMonitor> GetMonitorAsync(CancellationToken cancellationToken = default)
        {
            if (_monitor == null && await AdbServer.Instance.GetStatusAsync(cancellationToken).ContinueWith(x => x.Result.IsRunning).ConfigureAwait(false))
            {
                _monitor = new();
                await _monitor.StartAsync(cancellationToken).ConfigureAwait(false);
            }
            return _monitor;
        }
    }
}
