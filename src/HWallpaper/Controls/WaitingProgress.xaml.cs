using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HWallpaper.Controls
{
    /// <summary>
    /// WaitingProgress.xaml 的交互逻辑
    /// </summary>
    public partial class WaitingProgress : UserControl
    {
        private Storyboard story;
        public WaitingProgress()
        {
            InitializeComponent();
            this.story = (base.Resources["waiting"] as Storyboard);
            this.Visibility = Visibility.Hidden;
        }
        private void Image_Loaded_1(object sender, RoutedEventArgs e)
        {
            this.story.Begin(this.image, true);
        }
        public void Show()
        {
            this.Visibility = Visibility.Visible;
            this.story.Begin(this.image, true);
        }
        public void Stop()
        {
            base.Dispatcher.BeginInvoke(new Action(() => {
                this.story.Pause(this.image);
                base.Visibility = System.Windows.Visibility.Hidden;
            }));
        }
    }
}
