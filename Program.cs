using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ScreenRecorderLib;

namespace RecorderThing
{
    public class Program
    {
        private static Recorder _recorder;
        private static readonly string VideoFileDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly CancellationTokenSource CancellationTokenSource = new();
        public static string XemuWindowSearchPattern => "xemu |";

        public static bool IsRecording { get; private set; }
        public static bool HasFinishedRecordingFile { get; private set; }

        private static string VideoFileName => $"xemu {DateTime.Now:yyyy-MM-dd HH-mm-ss}.mp4";

        private static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            try
            {
                await ScanAndRecordXemuProcessesAsync(CancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
            }
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            CancellationTokenSource.Cancel();
        }

        private static async Task ScanAndRecordXemuProcessesAsync(CancellationToken token)
        {
            Console.WriteLine("Scanning for xemu processes...");

            while (!token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();

                switch (IsRecording)
                {
                    case false when HasFinishedRecordingFile:
                        await Task.Delay(100, token);
                        return;
                    case true:
                        continue;
                }

                var xemuProcess = FindXemuProcessByWindowTitle(XemuWindowSearchPattern);

                if (xemuProcess != null)
                {
                    Console.WriteLine($"Recording {xemuProcess.MainWindowTitle}...");
                    IsRecording = true;
                    RecordXemuWindow(xemuProcess, VideoFileName, VideoFileDirectory);
                }
                else
                {
                    await Task.Delay(100, token);
                }
            }
        }

        private static void RecordXemuWindow(Process xemuProcess, string fileName, string fileDirectory)
        {
            var fullPath = Path.Combine(fileDirectory, fileName);

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            _recorder = Recorder.CreateRecorder(
                DefaultRecorderOptions.GetDefaultRecorderOptionsFromXemuProcess(xemuProcess));

            xemuProcess.EnableRaisingEvents = true;

            xemuProcess.Exited += (_, _) =>
            {
                xemuProcess.EnableRaisingEvents = false;
                _recorder.Stop();
                IsRecording = false;
                HasFinishedRecordingFile = true;
                Console.WriteLine($"Recording for {xemuProcess.MainWindowTitle} complete");
            };

            _recorder.OnRecordingFailed += (_, e) => Console.WriteLine(e.Error);

            _recorder.Record(fullPath);
        }

        private static Process FindXemuProcessByWindowTitle(string pattern)
        {
            return Process.GetProcesses().FirstOrDefault(process => process.MainWindowTitle.Contains(pattern));
        }
    }
}