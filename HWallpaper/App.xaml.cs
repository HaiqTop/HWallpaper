using System;
using System.Windows;

namespace HWallpaper
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            bool createdNew;
            System.Threading.Mutex instance = new System.Threading.Mutex(true, nameof(HWallpaper), out createdNew);
            if (createdNew)
            {
                bool showWallpaper = e.Args.Length == 0 || !e.Args[0].Equals("autorun");
                var win = new MainWindow(showWallpaper);
                win.Show();
                instance.ReleaseMutex();
            }
            else
            {
                Environment.Exit(-2);
            }
        }
    }
}
