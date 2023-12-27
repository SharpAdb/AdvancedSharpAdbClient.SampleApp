using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.DeviceCommands.Models;
using AdvancedSharpAdbClient.Models;
using AdvancedSharpAdbClient.Polyfills;
using SharpADB.Common;
using SharpADB.Helpers;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;

namespace SharpADB.ViewModels
{
    public class ScreenViewModel : INotifyPropertyChanged
    {
        private Task loopTask;
        private ManualResetEventSlim manualResetEvent = new(true);
        private CancellationTokenSource cancellationTokenSource = new();

        public DispatcherQueue Dispatcher { get; } = DispatcherQueue.GetForCurrentThread();

        private bool isLoading = false;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        private bool isStopped = false;
        public bool IsStopped
        {
            get => isStopped;
            set
            {
                if (isStopped != value)
                {
                    isStopped = value;
                    RaisePropertyChangedEvent();
                    if (value) { manualResetEvent.Reset(); }
                    else { manualResetEvent.Set(); }
                }
            }
        }

        private DeviceData[] deviceList = [];
        public DeviceData[] DeviceList
        {
            get => deviceList;
            set => SetProperty(ref deviceList, value);
        }

        private DeviceData selectDevice;
        public DeviceData SelectDevice
        {
            get => selectDevice;
            set
            {
                if (selectDevice != value)
                {
                    selectDevice = value;
                    RaisePropertyChangedEvent();
                    OnSelectDeviceChanged(value);
                }
            }
        }

        private Element[] elements;
        public Element[] Elements
        {
            get => elements;
            set => SetProperty(ref elements, value);
        }

        private Element selectElement;
        public Element SelectElement
        {
            get => selectElement;
            set => SetProperty(ref selectElement, value);
        }

        private WriteableBitmap screenShot;
        public WriteableBitmap ScreenShot
        {
            get => screenShot;
            set => SetProperty(ref screenShot, value);
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

                if (!status.IsRunning)
                {
                    return;
                }

                AdbClient adbClient = new();
                DeviceList = await adbClient.GetDevicesAsync(default).ToArrayAsync();
                await RegisterMonitor();

                manualResetEvent ??= new(true);
                cancellationTokenSource ??= new();
                loopTask ??= UpdateLoopAsync(cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(HomeViewModel)).Error(ex.ExceptionToMessage());
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

        public async Task StopUpdateLoopAsync()
        {
            if (loopTask != null)
            {
                cancellationTokenSource.Cancel();
                manualResetEvent.Set();
                await loopTask.ConfigureAwait(false);
                loopTask.Dispose();
                loopTask = null;
            }
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
            manualResetEvent.Dispose();
            manualResetEvent = null;
        }

        private async void OnDeviceListChanged(object sender, DeviceDataNotifyEventArgs e)
        {
            AdbClient adbClient = new();
            DeviceList = await adbClient.GetDevicesAsync(default).ToArrayAsync();
        }

        private void OnSelectDeviceChanged(DeviceData deviceData)
        {
            if (isStopped || deviceData == null)
            {
                manualResetEvent.Reset();
            }
            else
            {
                manualResetEvent.Set();
            }
        }

        private async Task UpdateLoopAsync(CancellationToken cancellationToken)
        {
            AdbClient adbClient = null;
            Framebuffer framebuffer = null;
            await ThreadSwitcher.ResumeBackgroundAsync();
            do
            {
                try
                {
                    if (selectDevice == null)
                    {
                        manualResetEvent.Reset();
                        continue;
                    }
                    else
                    {
                        if (framebuffer == null)
                        {
                            adbClient = new();
                            framebuffer = await adbClient.GetFrameBufferAsync(selectDevice, cancellationToken).ConfigureAwait(false);
                        }

                        if (framebuffer != null)
                        {
                            await framebuffer.RefreshAsync(true, cancellationToken);
                            Task.WaitAll(
                                framebuffer.ToBitmapAsync(Dispatcher, cancellationToken).ContinueWith(x => ScreenShot = x.Result),
                                adbClient.FindElementsAsync(selectDevice).ContinueWith(x => Elements = x.Result.ToArray()));
                            GC.Collect();
                        }
                    }
                    manualResetEvent.Wait();
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(ScreenViewModel)).Error(ex.ExceptionToMessage());
                }
            }
            while (!cancellationToken.IsCancellationRequested);
        }
    }
}
