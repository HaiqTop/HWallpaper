using System;
using System.Windows.Media.Imaging;

namespace HWallpaper
{
    public partial class MainWindow
    {
        Wallpaper wallpaper;
        //HandyControl.Controls.NotifyIcon notifyIcon;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (wallpaper == null || wallpaper.IsClosed)
            {
                wallpaper = new Wallpaper();
                wallpaper.Show();
            }
            wallpaper.Activate();
            wallpaper.WindowState = System.Windows.WindowState.Normal;
        }

        private void NotifyIconContextContent_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void NotifyIconContextContent_MouseDoubleClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (wallpaper == null || wallpaper.IsClosed)
            { 
                wallpaper = new Wallpaper();
                wallpaper.Show();
            }
            wallpaper.Activate();
            wallpaper.WindowState = System.Windows.WindowState.Normal;
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            wallpaper = new Wallpaper();
            wallpaper.Show();
            this.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
