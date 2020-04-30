using System;
using System.Windows.Media.Imaging;

namespace HWallpaper
{
    public partial class MainWindow
    {
        //HandyControl.Controls.NotifyIcon notifyIcon;
        public MainWindow()
        {
            InitializeComponent();
            //notifyIcon = new HandyControl.Controls.NotifyIcon();
            //notifyIcon.Icon = new BitmapImage(new Uri(@"D:\Git\HWallpaper\HWallpaper\Image\favicon.png", UriKind.Absolute));
            //notifyIcon.Text = "H壁纸";
            //notifyIcon.Visibility =  System.Windows.Visibility.Visible;
        }

        private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Hide();
        }

        private void NotifyIconContextContent_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void NotifyIconContextContent_MouseDoubleClick(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Show();
            this.Activate();
        }
    }
}
