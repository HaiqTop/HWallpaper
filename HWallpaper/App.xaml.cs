using System;
using System.Windows;

namespace HWallpaper
{
    public partial class App : Application
    {
        [STAThread]
        static void Main(string[] args)
        {
            bool createdNew;
            System.Threading.Mutex instance = new System.Threading.Mutex(true, nameof(HWallpaper), out createdNew);
            if (createdNew)
            {
                bool showWallpaper = args.Length == 0 || !args[0].Equals("autorun");
                var win = new MainWindow(showWallpaper);

                var application = new App();
                application.InitializeComponent();
                application.Run(win);
                instance.ReleaseMutex();
            }
            else
            {
                Environment.Exit(-2);
            }
        }
    }
}
