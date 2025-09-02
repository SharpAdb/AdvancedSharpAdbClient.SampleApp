using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Models;
using Microsoft.Extensions.Logging;
using SharpADB.Common;
using SharpADB.Helpers;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.System;

namespace SharpADB.ViewModels
{
    public partial class HomeViewModel : INotifyPropertyChanged
    {
        public DispatcherQueue Dispatcher { get; } = DispatcherQueue.GetForCurrentThread();

        private bool isLoading = false;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        private Version adbVersion = new(1, 0, 0);
        public Version AdbVersion
        {
            get => adbVersion;
            set => SetProperty(ref adbVersion, value);
        }

        private DeviceData[] deviceList = [];
        public DeviceData[] DeviceList
        {
            get => deviceList;
            set => SetProperty(ref deviceList, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (property == null ? value != null : !property.Equals(value))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        public async Task Refresh()
        {
            try
            {
                if (isLoading) { return; }
                IsLoading = true;

                AdbServerStatus status = await AdbServer.Instance.GetStatusAsync(default).ConfigureAwait(false);
                AdbVersion = status.Version;

                if (!status.IsRunning)
                {
                    if (await AdbServer.Instance.StartServerAsync("adb", false, default).ConfigureAwait(false)
                        is not StartServerResult.Started or StartServerResult.AlreadyRunning)
                    {
                        return;
                    }
                    status = await AdbServer.Instance.GetStatusAsync(default).ConfigureAwait(false);
                    AdbVersion = status.Version;
                }

                AdbClient adbClient = new();
                DeviceList = await adbClient.GetDevicesAsync().ContinueWith(x => x.Result.ToArray());
                await RegisterMonitor();
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<HomeViewModel>().LogError(ex, "Failed to refresh home page. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                return;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RegisterMonitor()
        {
            DeviceMonitor monitor = await ADBHelper.GetMonitorAsync().ConfigureAwait(false);
            monitor.DeviceListChanged -= OnDeviceListChanged;
            monitor.DeviceListChanged += OnDeviceListChanged;
        }

        public async Task UnregisterMonitor()
        {
            DeviceMonitor monitor = await ADBHelper.GetMonitorAsync().ConfigureAwait(false);
            monitor.DeviceListChanged -= OnDeviceListChanged;
        }

        private async void OnDeviceListChanged(object sender, DeviceDataNotifyEventArgs e)
        {
            AdbClient adbClient = new();
            DeviceList = await adbClient.GetDevicesAsync().ContinueWith(x => x.Result.ToArray());
        }
    }
}
