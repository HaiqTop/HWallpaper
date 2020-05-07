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
using HandyControl.Controls;

namespace HWallpaper
{
    /// <summary>
    /// Wallpaper.xaml 的交互逻辑
    /// </summary>
    public partial class Setting
    {
        public Setting()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            // 初始化 标签
            TypeTotal typeTotal = WebImage.GetTypeList(true);
            foreach (var type in typeTotal.data)
            {
                Tag tag = new Tag();
                tag.Margin = new Thickness(0);
                tag.Selectable = true;
                tag.ShowCloseButton = false;
                tag.Content = type.name;
                tag.Tag = type.id;
                tag.IsSelected = (!string.IsNullOrEmpty(ConfigManage.Data.wallpaper_SelectedTypes)
                    && ConfigManage.Data.wallpaper_SelectedTypes.IndexOf(type.id) > -1);
                cbox_wallpaper_SelectedTypes.Children.Add(tag);
            }
            // 初始化 标签
            foreach (var type in typeTotal.data)
            {
                Tag tag = new Tag();
                tag.Margin = new Thickness(0);
                tag.Selectable = true;
                tag.ShowCloseButton = false;
                tag.Content = type.name;
                tag.Tag = type.id;
                tag.IsSelected = (!string.IsNullOrEmpty(ConfigManage.Data.screen_SelectedTypes)
                    && ConfigManage.Data.screen_SelectedTypes.IndexOf(type.id) > -1);
                cbox_screen_SelectedTypes.Children.Add(tag);
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton btn = sender as RadioButton;
            WallTimeType type = (WallTimeType)Convert.ToInt32(btn.Tag);
        }
    }
}
