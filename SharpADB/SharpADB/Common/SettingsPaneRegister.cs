using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Search;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace SharpADB.Common
{
    public static class SettingsPaneRegister
    {
        public static bool IsSearchPaneSupported { get; } = ApiInformation.IsTypePresent("Windows.ApplicationModel.Search.SearchPane") && CheckSearchExtension();
        public static bool IsSettingsPaneSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.ApplicationSettings.SettingsPane");

        public static void Register(Window window)
        {
            if (IsSettingsPaneSupported)
            {
                SettingsPane searchPane = SettingsPane.GetForCurrentView();
                searchPane.CommandsRequested -= OnCommandsRequested;
                searchPane.CommandsRequested += OnCommandsRequested;
                window.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
                window.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            }
        }

        public static void Unregister(Window window)
        {
            if (IsSettingsPaneSupported)
            {
                SettingsPane.GetForCurrentView().CommandsRequested -= OnCommandsRequested;
                window.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
            }
        }

        private static void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
        //    ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("SettingsPane");
        //    args.Request.ApplicationCommands.Add(
        //        new SettingsCommand(
        //            "Settings",
        //            loader.GetString("Settings"),
        //            async (handler) => new SettingsFlyoutControl { RequestedTheme = await ThemeHelper.GetActualThemeAsync() }.Show()));
        //    args.Request.ApplicationCommands.Add(
        //        new SettingsCommand(
        //            "Feedback",
        //            loader.GetString("Feedback"),
        //            (handler) => _ = Launcher.LaunchUriAsync(new Uri("https://github.com/Coolapk-UWP/Coolapk-Lite/issues"))));
        //    args.Request.ApplicationCommands.Add(
        //        new SettingsCommand(
        //            "LogFolder",
        //            loader.GetString("LogFolder"),
        //            async (handler) => _ = Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists))));
        //    args.Request.ApplicationCommands.Add(
        //        new SettingsCommand(
        //            "Translate",
        //            loader.GetString("Translate"),
        //            (handler) => _ = Launcher.LaunchUriAsync(new Uri("https://crowdin.com/project/CoolapkLite"))));
        //    args.Request.ApplicationCommands.Add(
        //        new SettingsCommand(
        //            "Repository",
        //            loader.GetString("Repository"),
        //            (handler) => _ = Launcher.LaunchUriAsync(new Uri("https://github.com/Coolapk-UWP/Coolapk-Lite"))));
        }

        private static void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType.HasFlag(CoreAcceleratorKeyEventType.KeyDown) || args.EventType.HasFlag(CoreAcceleratorKeyEventType.SystemKeyUp))
            {
                CoreVirtualKeyStates ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
                if (ctrl.HasFlag(CoreVirtualKeyStates.Down))
                {
                    CoreVirtualKeyStates shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);
                    if (shift.HasFlag(CoreVirtualKeyStates.Down))
                    {
                        switch (args.VirtualKey)
                        {
                            case VirtualKey.X when IsSettingsPaneSupported:
                                SettingsPane.Show();
                                args.Handled = true;
                                break;
                            case VirtualKey.Q when IsSearchPaneSupported:
                                SearchPane.GetForCurrentView().Show();
                                args.Handled = true;
                                break;
                        }
                    }
                }
            }
        }

        private static bool CheckSearchExtension()
        {
            XDocument doc = XDocument.Load(Path.Combine(Package.Current.InstalledLocation.Path, "AppxManifest.xml"));
            XNamespace ns = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10");
            IEnumerable<XElement> extensions = doc.Root.Descendants(ns + "Extension");
            if (extensions != null)
            {
                foreach (XElement extension in extensions)
                {
                    XAttribute category = extension.Attribute("Category");
                    if (category != null && category.Value == "windows.search")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
