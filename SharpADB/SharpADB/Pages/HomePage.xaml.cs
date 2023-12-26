using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Models;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SharpADB.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            Test();
        }

        private async void Test()
        {
            StartServerResult startServerResult = await AdbServer.Instance.StartServerAsync(@"C:\Users\qq251\OneDrive\应用\Win32\platform-tools\adb.exe", false, default);
            Debug.WriteLine(startServerResult);
            AdbClient client = new();
            Debug.WriteLine(await client.GetAdbVersionAsync());
            IEnumerable<DeviceData> devices = await client.GetDevicesAsync();
        }
    }
}
