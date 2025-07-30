using Serilog;
using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;

namespace ContactsManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string LogFolder { get { return Path.Combine(AppContext.BaseDirectory, "logs"); } }
        private string LogPath { get { return Path.Combine(LogFolder, _logFileName); } }
        private readonly string _logFileName = "log.txt";
        public App()
        {
            Directory.CreateDirectory(LogFolder);
            if (File.Exists(LogPath))
                File.Delete(LogPath);
            File.Create(LogPath).Close();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(LogPath)
                .CreateLogger();

            Log.Information("Init...");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Log.Information("Startup...");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Closing...");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }

}
