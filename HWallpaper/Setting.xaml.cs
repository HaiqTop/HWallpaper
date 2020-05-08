﻿using System;
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
using System.Windows.Controls.Primitives;

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
            LoadData();
        }

        private void LoadData()
        {
            TypeTotal typeTotal = WebImage.GetTypeList(true);
            #region 基本
            cbox_basic_AutoOn.IsChecked = ConfigManage.Base.AutoOn;
            cbox_basic_Cache.IsChecked = ConfigManage.Base.Cache;
            cbox_basic_AutoClearCache.IsChecked = ConfigManage.Base.AutoClearCache;
            tbox_basic_DownPath.Text = ConfigManage.Base.DownPath;
            tbox_basic_CachePath.Text = ConfigManage.Base.CachePath;
            #endregion 基本

            #region 壁纸
            cbox_wallpaper_AutoReplace.IsChecked = ConfigManage.Wallpaper.AutoReplace;
            nbox_wallpaper_Interval.Value = ConfigManage.Wallpaper.TimeInterval;
            // 壁纸类型
            foreach (var type in typeTotal.data)
            {
                Tag tag = new Tag();
                tag.Margin = new Thickness(0);
                tag.Selectable = true;
                tag.ShowCloseButton = false;
                tag.Content = type.name;
                tag.Tag = type.id;
                tag.IsSelected = (!string.IsNullOrEmpty(ConfigManage.Wallpaper.SelectedTypes)
                    && ConfigManage.Wallpaper.SelectedTypes.IndexOf(type.id) > -1);
                cbox_wallpaper_SelectedTypes.Children.Add(tag);
            }
            // 初始化 RadioButton
            switch (ConfigManage.Wallpaper.TimeType)
            {
                case TimeType.Day:
                    WallTimeType_Day.IsChecked = true;
                    break;
                case TimeType.Hour:
                    WallTimeType_Hour.IsChecked = true;
                    break;
                case TimeType.Minute:
                    WallTimeType_Minute.IsChecked = true;
                    break;
            }
            #endregion

            #region 屏保
            cbox_screen_AutoReplace.IsChecked = ConfigManage.Screen.Open;
            nbox_screen_OpenInterval.Value = ConfigManage.Screen.OpenInterval;
            nbox_screen_ReplaceInterval.Value = ConfigManage.Screen.ReplaceInterval;
            // 壁纸类型
            foreach (var type in typeTotal.data)
            {
                Tag tag = new Tag();
                tag.Margin = new Thickness(0);
                tag.Selectable = true;
                tag.ShowCloseButton = false;
                tag.Content = type.name;
                tag.Tag = type.id;
                tag.IsSelected = (!string.IsNullOrEmpty(ConfigManage.Screen.SelectedTypes)
                    && ConfigManage.Screen.SelectedTypes.IndexOf(type.id) > -1);
                cbox_screen_SelectedTypes.Children.Add(tag);
            }
            #endregion

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton btn = sender as RadioButton;
            ConfigManage.Wallpaper.TimeType = (TimeType)Convert.ToInt32(btn.Tag);
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigManage.Save();
        }
    }
}