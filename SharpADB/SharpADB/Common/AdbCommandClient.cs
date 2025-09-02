using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.Logs;
using SharpADB.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace SharpADB.Common
{
    public partial class AdbCommandClient(string adbPath, bool isForce = false, ILogger<AdbCommandLineClient> logger = null) : AdbCommandLineClient(adbPath, isForce, logger)
    {
        ~AdbCommandClient() => m_serverManager?.Dispose();

        /// <summary>
        /// The singleton instance of the <see cref="IServerManager"/> COM interface.
        /// </summary>
        private IServerManager m_serverManager;

        /// <summary>
        /// Gets the singleton instance of the <see cref="IServerManager"/> COM interface.
        /// </summary>
        public IServerManager ServerManager
        {
            get
            {
                try
                {
                    if (m_serverManager?.IsServerRunning == true)
                    {
                        return m_serverManager;
                    }
                }
                catch { }

                m_serverManager = Factory.CreateRemoteThing();
                return m_serverManager;
            }
        }

        protected override int RunProcess(string filename, string command, ICollection<string> errorOutput, ICollection<string> standardOutput, int timeout)
        {
            using IServerManager manager = Factory.CreateRemoteThing();
            return manager.RunProcess.RunProcess(filename, command, AsVector(errorOutput), AsVector(standardOutput), timeout);
        }

        protected override async Task<int> RunProcessAsync(string filename, string command, ICollection<string> errorOutput, ICollection<string> standardOutput, CancellationToken cancellationToken = default)
        {
            using IServerManager manager = Factory.CreateRemoteThing();
            return await manager.RunProcess.RunProcessAsync(filename, command, AsVector(errorOutput), AsVector(standardOutput)).AsTask(cancellationToken);
        }

        private static CollectionVector AsVector(ICollection<string> collection) => collection switch
        {
            not null => new CollectionVector(collection),
            _ => null,
        };

        [EditorBrowsable(EditorBrowsableState.Never)]
        private partial class CollectionVector(ICollection<string> values) : IList<string>
        {
            string IList<string>.this[int index]
            {
                get => values.ElementAt(index);
                set => throw new NotImplementedException();
            }

            public int Count => values.Count;

            public bool IsReadOnly => values.IsReadOnly;

            public void Add(string item) => values.Add(item);

            public void Clear() => values.Clear();

            public bool Contains(string item) => values.Contains(item);

            public void CopyTo(string[] array, int arrayIndex) => values.CopyTo(array, arrayIndex);

            public IEnumerator<string> GetEnumerator() => values.GetEnumerator();

            public int IndexOf(string item)
            {
                int i = 0;
                EqualityComparer<string> comparer = EqualityComparer<string>.Default;
                foreach (string value in values)
                {
                    if (comparer.Equals(item, value))
                    {
                        return i;
                    }
                    i++;
                }
                return -1;
            }

            void IList<string>.Insert(int index, string item) => throw new NotImplementedException();

            public bool Remove(string item) => values.Remove(item);

            void IList<string>.RemoveAt(int index) => values.Remove(values.ElementAt(index));

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)values).GetEnumerator();
        }
    }

    public partial class RunProcess : IRunProcess
    {
        /// <summary>
        /// The <see cref="Array"/> of <see cref="char"/>s that represent a new line.
        /// </summary>
        private static readonly char[] separator = ['\r', '\n'];

        public static IRunProcess Instance { get; } = new RunProcess();

        IAsyncOperation<int> IRunProcess.RunProcessAsync(string filename, string command, IList<string> errorOutput, IList<string> standardOutput)
        {
            return AsyncInfo.Run(async (cancellationToken) =>
            {
                ProcessStartInfo psi = new(filename, command)
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardError = errorOutput != null,
                    RedirectStandardOutput = standardOutput != null
                };

                using Process process = Process.Start(psi) ?? throw new AdbException($"The adb process could not be started. The process returned null when starting {filename} {command}");

                using (CancellationTokenRegistration registration = cancellationToken.Register(process.Kill))
                {
                    if (errorOutput != null)
                    {
                        string standardErrorString = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                        errorOutput.AddRange(standardErrorString.Split(separator, StringSplitOptions.RemoveEmptyEntries));
                    }

                    if (standardOutput != null)
                    {
                        string standardOutputString = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                        standardOutput.AddRange(standardOutputString.Split(separator, StringSplitOptions.RemoveEmptyEntries));
                    }

                    if (!process.HasExited)
                    {
                        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
                    }
                }

                // get the return code from the process
                return process.ExitCode;
            });
        }

        int IRunProcess.RunProcess(string filename, string command, IList<string> errorOutput, IList<string> standardOutput, int timeout)
        {
            ProcessStartInfo psi = new(filename, command)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardError = errorOutput != null,
                RedirectStandardOutput = standardOutput != null
            };

            using Process process = Process.Start(psi) ?? throw new AdbException($"The adb process could not be started. The process returned null when starting {filename} {command}");

            // get the return code from the process
            if (!process.WaitForExit(timeout))
            {
                process.Kill();
            }

            if (errorOutput != null)
            {
                string standardErrorString = process.StandardError.ReadToEnd();
                errorOutput.AddRange(standardErrorString.Split(separator, StringSplitOptions.RemoveEmptyEntries));
            }

            if (standardOutput != null)
            {
                string standardOutputString = process.StandardOutput.ReadToEnd();
                standardOutput.AddRange(standardOutputString.Split(separator, StringSplitOptions.RemoveEmptyEntries));
            }

            return process.ExitCode;
        }
    }
}
