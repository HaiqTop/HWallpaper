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

namespace HWallpaper
{
    /// <summary>
    /// Wallpaper.xaml 的交互逻辑
    /// </summary>
    public partial class Wallpaper
    {
        private int curType = 0;
        private string curTypeName = "最新";
        public Wallpaper()
        {
            InitializeComponent();
            InitPicTypeBtn();
        }
        public bool IsClosed { get; private set; }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }
        /// <summary>
        /// 初始化壁纸分类按钮
        /// </summary>
        private void InitPicTypeBtn()
        {
            try
            {
                TypeTotal total = WebImage.GetTypeList();
                if (total != null && total.data != null && total.data.Count > 0)
                {
                    int tabIndex = 100;
                    int index = 1;
                    foreach (TypeList type in total.data)
                    {
                        var item = new HandyControl.Controls.TabItem();
                        item.Name = "btn_type_" + index++;
                        item.TabIndex = ++tabIndex;
                        item.Header = type.name;
                        item.Tag = type.id;
                        tabControl.Items.Add(item);
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, EnumLogLevel.Error);
                MessageBox.Show(ex.Message, "错误");
            }

        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandyControl.Controls.TabControl tab = sender as HandyControl.Controls.TabControl;
            System.Windows.Media.Imaging.BitmapImage a;
        }
    }
}
