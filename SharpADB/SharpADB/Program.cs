using SharpADB.Common;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.System;
using Windows.UI.Xaml;

namespace SharpADB
{
    public static partial class Program
    {
        private const int RO_INIT_MULTITHREADED = 1;

        private static void Main(string[] args)
        {
            switch (args)
            {
                case ["-RegisterProcessAsComServer", ..]:
                    ManualResetEventSlim comServerExitEvent = new(false);
                    comServerExitEvent.Reset();
                    ServerManager.ServerManagerDestructed += comServerExitEvent.Set;
                    RemoteThingFactory factory = new();
                    factory.RegisterClassObject();
                    _ = ServerManager.CheckReferenceAsync();
                    comServerExitEvent.Wait();
                    ServerManager.ServerManagerDestructed -= comServerExitEvent.Set;
                    factory.RevokeClassObject();
                    break;
                case ["-RegisterProcessAsWinRTServer", ..]:
                    _ = RoInitialize(RO_INIT_MULTITHREADED);
                    comServerExitEvent = new ManualResetEventSlim(false);
                    comServerExitEvent.Reset();
                    ServerManager.ServerManagerDestructed += comServerExitEvent.Set;
                    factory = new RemoteThingFactory();
                    factory.RegisterActivationFactory();
                    _ = ServerManager.CheckReferenceAsync();
                    comServerExitEvent.Wait();
                    ServerManager.ServerManagerDestructed -= comServerExitEvent.Set;
                    factory.RevokeActivationFactory();
                    break;
                default:
                    Application.Start(static p =>
                    {
                        DispatcherQueueSynchronizationContext context = new(DispatcherQueue.GetForCurrentThread());
                        SynchronizationContext.SetSynchronizationContext(context);
                        _ = new App();
                    });
                    break;
            }
        }

        [LibraryImport("api-ms-win-core-winrt-l1-1-0.dll")]
        private static partial int RoInitialize(int initType);
    }
}
