using AdvancedSharpAdbClient.DeviceCommands.Models;
using SharpADB.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SharpADB.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ScreenPage : Page
    {
        private readonly ScreenViewModel Provider = new();

        public ScreenPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _ = Provider.Refresh();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _ = Provider.UnregisterMonitor();
            _ = Provider.StopUpdateLoopAsync();
        }

        internal static Element[] AsElementArray(IEnumerable<Element> elements) => elements.ToArray();
    }
}
