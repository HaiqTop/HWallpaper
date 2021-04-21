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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HWallpaper.Model;
using HWallpaper.Common;
using HWallpaper.Business;
using HWallpaper.Controls;
using HandyControl;

namespace HWallpaper
{
    /// <summary>
    /// Wallpaper.xaml 的交互逻辑
    /// </summary>
    public partial class Wallpaper
    {
        private MainWindow main;
        public Wallpaper(MainWindow main)
        {
            this.main = main;
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            AutoUpdate.Helper.UpdateHelper.CheckUpdateAsyn();
        }
        public bool IsClosed { get; private set; }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitPicTypeBtn();
        }
        /// <summary>
        /// 初始化壁纸分类按钮
        /// </summary>
        private void InitPicTypeBtn()
        {
            try
            {
                // 最新
                TabItem item = new TabItem();
                item.Name = "btn_type_0";
                item.TabIndex = 99;
                item.Header = "最新";
                item.Tag = 0;
                tabControl.Items.Add(item);

                TypeTotal total = WebImage.GetTypeList(true);
                if (total != null && total.data != null && total.data.Count > 0)
                {
                    int tabIndex = 100;
                    int index = 1;
                    foreach (TypeList type in total.data)
                    {
                        item = new TabItem();
                        item.Name = "btn_type_" + index++;
                        item.TabIndex = ++tabIndex;
                        item.Header = type.name;
                        item.Tag = type.id;
                        item.Visibility = Visibility.Collapsed;
                        tabControl.Items.Add(item);
                    }
                    window_SizeChanged(null, null);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, EnumLogLevel.Error);
                HandyControl.Controls.Growl.Error(ex.Message);
            }

        }
        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.AddedItems[0] as TabItem;
            if (item.Content == null)
            {
                string type = Convert.ToString(item.Tag);
                ImageList imageList = new ImageList();
                imageList.Margin = new Thickness(0);
                item.Margin = new Thickness(0);
                item.Content = imageList;
                imageList.LoadImage(type, 0);
            }
        }

        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double itemWidthTotal = 0;
            foreach (TabItem item in tabControl.Items)
            {
                if (itemWidthTotal < this.Width - 10)
                {
                    itemWidthTotal += 70;
                    item.Visibility = Visibility.Visible;
                }
                else
                {
                    item.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            switch (menuItem.Header)
            {
                case "搜索":
                    tabControl.Visibility = Visibility.Hidden;
                    searchGrid.Visibility = Visibility.Visible;
                    break;
                case "屏保":
                    Screensaver screen = new Screensaver();
                    screen.Show();
                    break;
                case "设置":
                    Setting setting = new Setting();
                    setting.ChangeConfigEvent += Setting_ChangeConfigEvent;
                    setting.ShowDialog();
                    break;
                case "关于":
                    About about = new About();
                    about.ShowDialog();
                    break;
            }
        }

        private void Setting_ChangeConfigEvent()
        {
            main.InitTimers_Screen();
            main.InitTimers_Wallpaper();
            main.InitTimers_Screen();
        }

        private void SearchBar_SearchStarted(object sender, HandyControl.Data.FunctionEventArgs<string> e)
        {
            if (!string.IsNullOrEmpty(e.Info))
            {
                searchList.SearchImage(e.Info);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 如果是大图浏览模式则先退出该模式
            if (searchList.IsBigImgModel())
            {
                searchList.ExitBigImgModel();
                return;
            }
            tabControl.Visibility = Visibility.Visible;
            searchGrid.Visibility = Visibility.Hidden;
        }
    }
}
