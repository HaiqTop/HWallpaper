using HandyControl.Controls;
using HWallpaper.Business;
using HWallpaper.Common;
using HWallpaper.Model;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace HWallpaper.Controls
{
    /// <summary>
    /// ImageList.xaml 的交互逻辑
    /// </summary>
    public partial class ImageList : UserControl
    {
        private string picType = "0";
        private string keyWord = string.Empty;
        private int picIndex = 0;
        private ImageListTotal curImageListTotal = null;
        private Size imgSize = new Size(320,180);
        private Size ScreenSize = new Size(1920,1080);
        private readonly int scrollWidth = 10;//定义滚动条宽度，需要根据实际滚动条宽度设置
        private string CachePath;
        private string DownPath;
        private HandyControl.Controls.ScrollViewer scr = new HandyControl.Controls.ScrollViewer();
        private Grid scrGrid = new Grid();
        private WrapPanel panel = new WrapPanel();
        Thickness margin = new Thickness(2);
        DownQueue downQueue = null;

        //首次加载页面异步实现需要用的
        private Thread thread = null;
        private event ComplateDelegate OnComplate;
        private delegate void ComplateDelegate(ImageListTotal total, string msg);

        public ImageList()
        {
            InitializeComponent();
            CachePath = ConfigManage.Base.CachePath;
            DownPath = ConfigManage.Base.DownPath;
            this.SizeChanged += new System.Windows.SizeChangedEventHandler(Resize);
            downQueue = new DownQueue(this,true);
            downQueue.OnComplate += DownQueue_OnComplate;
            downQueue.OnError += DownQueue_OnError;
            InitBtn();
        }

        /// <summary>
        /// 输入屏幕大小参数（默认1920x1080）
        /// </summary>
        /// <param name="size"></param>
        public void SetScreenSize(Size size)
        {
            if (size != null)
            {
                this.ScreenSize.Width = size.Width;
                this.ScreenSize.Height = size.Height;
            }
        }

        /// <summary>
        /// 根据壁纸类型加载壁纸
        /// </summary>
        /// <param name="picType">壁纸类型（love：最爱|down：下载的|0：最新|>0：其他分类）</param>
        /// <param name="index"></param>
        /// <returns></returns>
        public void LoadImage(string picType = "0", int index = 0)
        {
            if (thread == null)
            {
                this.picType = picType;
                this.picIndex = index;
                curImageListTotal = null;
                scr.ScrollToTop();
                panel.Children.Clear();
                this.OnComplate += ImageList_OnComplate;
                thread = new Thread(new ThreadStart(NextImages));
                thread.Name = "LoadImageList_" + picType;
                thread.Start();
            }
        }

        /// <summary>
        /// 壁纸列表数据加载完成事件
        /// </summary>
        /// <param name="total">壁纸列表</param>
        /// <param name="msg">返回的消息</param>
        private void ImageList_OnComplate(ImageListTotal total, string msg)
        {
            if (total != null && total.data.Count > 0)
            {
                var list = total.data;
                foreach (var picInfo in list)
                {
                    Image img = new Image();
                    img.Tag = PageType.MPA;
                    img.Height = imgSize.Height;
                    img.Width = imgSize.Width;
                    img.Margin = margin;
                    img.MouseDown += Image_MouseDown;
                    Grid grid = new Grid();
                    grid.Children.Add(new LoadingCircle());
                    grid.Children.Add(img);
                    downQueue.Queue(img, picInfo, picInfo.GetUrlBySize((int)img.Width, (int)img.Height));
                    grid.MouseEnter += Grid_MouseEnter;
                    grid.MouseLeave += Grid_MouseLeave;
                    panel.Children.Add(grid);
                    grid.Tag = this.picIndex;
                    picInfo.Index = this.picIndex++;
                }
                if (curImageListTotal == null)
                {
                    curImageListTotal = total;
                }
                else
                {
                    curImageListTotal.data.AddRange(list);
                }
            }
            else if(!string.IsNullOrEmpty(msg))
            {
                Growl.Info(msg);
            }

            zoomImage.Source = null;
            // 如果当前是单页显示模式，则需要加载新的壁纸类型的第一张壁纸
            if (this.Content is Grid pGrid && pGrid.Name == "zoomGrid")
            {
                if (curImageListTotal != null && curImageListTotal.data.Count > 0)
                {
                    this.ShowBigPic(curImageListTotal.data[0]);
                }
            }
        }

        /// <summary>
        /// 根据关键字搜索壁纸，并加载壁纸列表
        /// </summary>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        public int SearchImage(string keyWord)
        {
            this.keyWord = keyWord;
            this.picIndex = 0;
            curImageListTotal = null;
            scr.ScrollToTop();
            panel.Children.Clear();
            if(this.OnComplate == null)
                this.OnComplate += ImageList_OnComplate;
            NextImages();
            zoomImage.Source = null;
            // 如果当前是单页显示模式，则需要加载新的壁纸类型的第一张壁纸
            if (this.Content is Grid pGrid && pGrid.Name == "zoomGrid")
            {
                if (curImageListTotal != null && curImageListTotal.data.Count > 0)
                {
                    this.ShowBigPic(curImageListTotal.data[0]);
                }
            }
            return curImageListTotal == null ? 0 : curImageListTotal.total;
        }

        /// <summary>
        /// 下一组图片
        /// </summary>
        public void NextImages()
        {
            if (curImageListTotal == null || this.picIndex < curImageListTotal.total)
            {
                ImageListTotal total = null;
                if (string.IsNullOrEmpty(keyWord))
                {
                    total = ImageHelper.GetImageListTotal(this.picType, this.picIndex, 24);
                }
                else
                {
                    total = WebImage.GetImageListByKW(keyWord, this.picIndex, 24);
                }
                string msg = string.Empty;
                if (total.total == 0)
                {
                    switch (this.picType)
                    {
                        case "wall":
                            msg = "您还没有设置过壁纸"; break;// 设置壁纸
                        case "love":
                            msg = "您还没有收藏过壁纸"; break;// 收藏的
                        case "down":
                            msg = "您还没有下载过壁纸"; break;//下载的
                        default:
                            if (!string.IsNullOrEmpty(keyWord))//搜索
                                msg = $"未搜索到与【{keyWord}】相关的壁纸"; break;
                    }
                }
                this.Dispatcher.BeginInvoke(this.OnComplate, new Object[] { total, msg });
            }
            else if (curImageListTotal != null && this.picIndex >= curImageListTotal.total)
            { 
                Growl.Info($"已经滑到底了，总共{curImageListTotal.total}张壁纸");
            }
        }

        /// <summary>
        /// 初始化壁纸类型标签按钮
        /// </summary>
        private void InitBtn()
        {
            btn_down.ToolTip = "下载到本地";
            //btn_down.Margin = new Thickness(0, 10, 10, 0);
            btn_wallpaper.ToolTip = "设置为壁纸";
            btn_love.ToolTip = "喜欢该壁纸";
            btn_dislike.ToolTip = "不喜欢该壁纸";
            btnPanel.Visibility = Visibility.Hidden;
            zoomGrid.Children.Remove(btnPanel);
            zoomGrid.Height = this.Height;
            zoomGrid.Width = this.Width;
            zoomImage.Tag = PageType.SPA;
            scr.Width = this.Width;
            scr.Margin = new Thickness(0);
            scr.ScrollChanged += ListView_ScrollChanged;
            scr.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            panel.Margin = new Thickness(2);
            panel.Orientation = Orientation.Horizontal;
            panel.Width = scr.ActualWidth;

            scr.Content = panel;
            scrGrid.Children.Add(scr);
            this.Content = scrGrid;
        }

        /// <summary>
        /// 鼠标滑轮滚动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomGrid_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            try
            {
                ImgInfo imgInfo = null;
                // 滚轮向上
                if (e.Delta > 0)
                {
                    imgInfo = this.GetimgInfo(zoomGrid.Tag, -1);
                }
                // 滚轮向下
                else
                {
                    imgInfo = this.GetimgInfo(zoomGrid.Tag, 1);
                }
                this.ShowBigPic(imgInfo);
            }
            catch (Exception ex)
            {
                Growl.Info(ex.Message);
            }
        }

        /// <summary>
        /// 显示大图
        /// </summary>
        private void ShowBigPic(ImgInfo imgInfo)
        {
            if (imgInfo != null)
            {
                loading.Visibility = Visibility.Visible;
                zoomImage.Visibility = Visibility.Hidden;
                downQueue.Queue(zoomImage, imgInfo);
                zoomGrid.Tag = imgInfo.Index;
                lb_picName.Text = "文件名称：" + imgInfo.GetFileName();
                lb_picDate.Text = "上传时间：" + imgInfo.create_time;
                lb_picTags.Text = "标签：" + string.Join(" ", imgInfo.GetTagList());
            }
        }

        /// <summary>
        /// 队列中每个壁纸下载完成的事件
        /// </summary>
        /// <param name="i">关联的图片控件</param>
        /// <param name="b">下载好的壁纸</param>
        /// <param name="imgInfo">壁纸信息</param>
        private void DownQueue_OnComplate(Image i, BitmapImage b, ImgInfo imgInfo)
        {
            try
            {
                i.Source = b;
                Storyboard storyboard = new Storyboard();
                DoubleAnimation doubleAnimation = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(500.0)));
                Storyboard.SetTarget(doubleAnimation, i);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity", new object[0]));
                storyboard.Children.Add(doubleAnimation);
                storyboard.Begin();
                if ((PageType)i.Tag == PageType.SPA)
                {
                    zoomImage.Visibility = Visibility.Visible;
                    loading.Visibility = Visibility.Hidden;
                    // 判断按钮组的Panel的父容器是否为空，为空则将其添加到大图Grid中
                    if (btnPanel.Parent == null)
                    {
                        zoomGrid.Children.Add(btnPanel);
                    }
                    // 判断按钮组的Panel的父容器是不是大图浏览的Grid，如果不是则替换为大图
                    else if (btnPanel.Parent is Grid pGrid && pGrid.Name != "zoomGrid")
                    {
                        pGrid.Children.Remove(btnPanel);
                        zoomGrid.Children.Add(btnPanel);
                    }
                    InitBtnState(imgInfo);
                }
                else
                {
                    if (i.Parent is Grid grid)
                    {
                        RemoveLoading(grid);
                    }
                }
            }
            catch (Exception ex)
            {
                string logText = $"壁纸ID:{imgInfo.id}\n"
                    + $"壁纸Url:{imgInfo.url}\n"
                    + "壁纸加载失败：" + ex.Message;
                LogHelper.WriteLog(logText, EnumLogLevel.Error);
                if (i.Parent is Grid grid)
                {
                    RemoveLoading(grid);
                    TextBlock txb = new TextBlock();
                    txb.Text = "壁纸加载失败：" + ex.Message;
                    grid.Children.Add(txb);
                }
            }
        }

        /// <summary>
        /// 队列中每个壁纸下载出现错误的事件
        /// </summary>
        /// <param name="e"></param>
        private void DownQueue_OnError(Image i, Exception e)
        {
            LogHelper.WriteLog("壁纸加载失败：" + e.Message , EnumLogLevel.Error);
            if (i.Parent is Grid grid)
            {
                RemoveLoading(grid);
                TextBlock txb = new TextBlock();
                txb.Text = "壁纸加载失败：" + e.Message;
                grid.Children.Add(txb);
            }
        }

        /// <summary>
        /// 移除Grid内的加载中控件
        /// </summary>
        /// <param name="grid"></param>
        private void RemoveLoading(Grid grid)
        {
            foreach (var item in grid.Children)
            {
                if (item is LoadingCircle load)
                {
                    grid.Children.Remove(load);
                    break;
                }
            }
        }

        /// <summary>
        /// 窗口大小改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Resize(object sender, System.EventArgs e)
        {
            panel.Width = scr.ActualWidth;
            zoomGrid.Height = this.Height;
            zoomGrid.Width = this.Width;
            if (panel.Width > 0)
            {
                // 一行1张图
                if (panel.Width <= 350)
                {
                    imgSize.Width = panel.Width - scrollWidth;
                }
                // 一行2张图
                else if (panel.Width > 350 && panel.Width <= 700)
                {
                    imgSize.Width = (panel.Width - scrollWidth) / 2;
                }
                // 一行3张图
                else if (panel.Width > 700 && panel.Width <= 1050)
                {
                    imgSize.Width = (panel.Width - scrollWidth) / 3;
                }
                // 一行4张图
                else if (panel.Width > 1050 && panel.Width <= 1400)
                {
                    imgSize.Width = (panel.Width - scrollWidth) / 4;
                }
                // 一行5张图
                else if (panel.Width > 1400)
                {
                    imgSize.Width = (panel.Width - scrollWidth) / 5;
                }
                imgSize.Width = imgSize.Width - margin.Left - margin.Right;
                imgSize.Height = ScreenSize.Height * imgSize.Width / ScreenSize.Width;
                foreach (Grid grid in panel.Children)
                {
                    foreach (var item in grid.Children)
                    {
                        if (item.GetType() == typeof(Image))
                        {
                            Image img = item as Image;
                            img.Width = imgSize.Width;
                            img.Height = imgSize.Height;
                            break;
                        }
                    }
                }
            }
        }

        private double scrVerticalOffset = -1;//解决大图模式下点击图片切换回小图列表模式的时候，不能记住之前列表位置的问题

        /// <summary>
        /// 点击图片事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                Image img = sender as Image;
                if (img == null) return;
                Grid grid = img.Parent as Grid;
                if (grid == null) return;
                // 如果是单页模式，就切换回多页
                if ((PageType)img.Tag == PageType.SPA)
                {
                    zoomGrid.Tag = null;
                    zoomImage.Source = new BitmapImage(new Uri("/Controls;component/Images/screen.png", UriKind.Relative));
                    this.Content = scrGrid;
                    Resize(null,null);
                    scrVerticalOffset = scr.VerticalOffset;
                }
                else if ((PageType)img.Tag == PageType.MPA)
                {
                    // 进入大图模式
                    zoomGrid.Tag = grid.Tag;
                    ImgInfo imgInfo = this.GetimgInfo(grid.Tag);
                    //zoomImage.Source = new BitmapImage(new Uri(imgInfo.url, UriKind.Absolute));
                    this.Content = zoomGrid;
                    this.ShowBigPic(imgInfo);
                }

            }
        }
        
        /// <summary>
        /// 小图列表模式下滚动事件（实现滚动懒加载）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_ScrollChanged(object sender, RoutedEventArgs e)
        {
            if (scr.ViewportHeight + scr.VerticalOffset >= scr.ExtentHeight && scr.ExtentHeight > scr.ViewportHeight)
            {
                NextImages();
            }
            if (scrVerticalOffset > -1)
            {
                scr.ScrollToVerticalOffset(scrVerticalOffset);
                scrVerticalOffset = -1;
            }
        }

        /// <summary>
        /// 鼠标滑出事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid == null) return;
            if (grid.Name != "zoomGrid")// 多页（小图）浏览模式
            {
                btnPanel.Visibility = Visibility.Hidden;
                if (btnPanel.Parent is Grid pGrid && pGrid.Name != "zoomGrid")
                {
                    pGrid.Children.Remove(btnPanel);
                }
            }
        }

        /// <summary>
        /// 鼠标滑入事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid == null) return;
            if (grid.IsMouseOver)
            {
                if (grid.Name != "zoomGrid")// 多页（小图）浏览模式
                {
                    if (btnPanel.Parent is Grid pGrid)
                    {
                        pGrid.Children.Remove(btnPanel);
                    }
                    grid.Children.Add(btnPanel);
                    ImgInfo imgInfo = this.GetimgInfo(grid.Tag);
                    InitBtnState(imgInfo);
                }
            }
        }

        /// <summary>
        /// 初始化壁纸上显示的按钮状态
        /// </summary>
        /// <param name="imgInfo"></param>
        private void InitBtnState(ImgInfo imgInfo)
        {
            if (imgInfo == null)
            {
                return;
            }
            btnPanel.Tag = imgInfo;
            btn_love.Foreground = Brushes.White;
            btn_wallpaper.Foreground = Brushes.White;
            btn_dislike.Foreground = Brushes.White;
            btn_down.Foreground = Brushes.White;

            btnPanel.Visibility = Visibility.Visible;

            Love love = UserDataManage.GetLove(imgInfo.Id);
            if (love != null)
            {
                if (love.Type == 1)
                {
                    btn_love.Foreground = Brushes.Red;
                }
                else if (love.Type == -1)
                {
                    btn_dislike.Foreground = Brushes.Red;
                }
            }
            Download down = UserDataManage.GetDown(imgInfo.Id);
            string fullName = System.IO.Path.Combine(this.DownPath, imgInfo.GetFileName());
            if (System.IO.File.Exists(fullName))
            {
                btn_down.Foreground = Brushes.Red;
                UserDataManage.SetDown(fullName, imgInfo);
            }
        }

        /// <summary>
        /// 壁纸上动态显示的按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            ImgInfo imgInfo;
            if ((btn.Parent as StackPanel).Tag is ImgInfo info)
            {
                imgInfo = info;
            }
            else
            {
                imgInfo = this.GetimgInfo((btn.Parent as StackPanel).Tag);
            }
            switch (btn.Name)
            {
                case "btn_down":
                    {
                        if (btn.Foreground == Brushes.White)
                        {
                            btn.IsEnabled = false;
                            string imgFullName = System.IO.Path.Combine(this.DownPath, imgInfo.GetFileName());
                            if (!System.IO.File.Exists(imgFullName))
                            {
                                System.Drawing.Image img = WebHelper.GetImage(imgInfo.url);
                                img.Save(imgFullName);
                                img.Dispose();
                            }
                            btn.Foreground = Brushes.Red;
                            btn.IsEnabled = true;
                            Growl.Success("下载成功。");
                            UserDataManage.SetDown(imgFullName, imgInfo);
                            if (btn_love.Foreground == Brushes.White)
                            {
                                UserDataManage.SetLove(LoveType.Love, imgInfo);
                                btn_love.Foreground = Brushes.Red;
                                btn_dislike.Foreground = Brushes.White;
                            }
                        }
                    }
                    break;
                case "btn_wallpaper":
                    {
                        if (btn.Foreground == Brushes.White)
                        {
                            btn.IsEnabled = false;
                            string imgFullName = System.IO.Path.Combine(this.CachePath, imgInfo.GetFileName());
                            if (!System.IO.File.Exists(imgFullName))
                            {
                                System.Drawing.Image img = WebHelper.GetImage(imgInfo.url);
                                img.Save(imgFullName);
                                img.Dispose();
                            }
                            if (System.IO.File.Exists(imgFullName))
                            {
                                WinApi.SetWallpaper(imgFullName);
                                btn.Foreground = Brushes.Red;
                                Growl.Success("壁纸设置成功。");
                                btn.IsEnabled = true;
                                UserDataManage.AddRecord(RecordType.ManualWallpaper, imgInfo);
                            }
                            else
                            { 
                                Growl.Error("未找到壁纸文件，壁纸设置失败。");
                            }
                        }
                    }
                    break;
                case "btn_love":
                    {
                        if (btn.Foreground == Brushes.White)
                        {
                            UserDataManage.SetLove(LoveType.Love, imgInfo);
                            btn.Foreground = Brushes.Red;
                            btn_dislike.Foreground = Brushes.White;
                        }
                    }
                    break;
                case "btn_dislike":
                    {
                        if (btn.Foreground == Brushes.White)
                        {
                            UserDataManage.SetLove(LoveType.Dislike, imgInfo);
                            btn.Foreground = Brushes.Red;
                            btn_love.Foreground = Brushes.White;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag">tag对象</param>
        /// <param name="offset">偏移量，0：当前下标对应的图像，1：下一张，2：下两张，-1：上一张</param>
        /// <returns></returns>
        private ImgInfo GetimgInfo(object tag,int offset = 0)
        {
            if (tag != null && tag is Int32 index)
            {
                int nIndex = index + offset;
                if (curImageListTotal == null || curImageListTotal.data == null)
                {
                    NextImages();
                }
                if (nIndex < 0 || nIndex > curImageListTotal.total)
                {
                    throw new Exception("没有了，请看看其他类型的吧~_~");
                }
                if (nIndex >= curImageListTotal.data.Count)
                {
                    NextImages();
                }
                return curImageListTotal.data[nIndex];
            }
            return null;
        }

        /// <summary>
        /// 判断当前是不是大图显示模式
        /// </summary>
        /// <returns>大图返回true</returns>
        public bool IsBigImgModel()
        {
            return this.Content is Grid pGrid && pGrid.Name == "zoomGrid";
        }

        /// <summary>
        /// 退出大图显示模式
        /// </summary>
        public void ExitBigImgModel()
        {
            zoomGrid.Tag = null;
            zoomImage.Source = new BitmapImage(new Uri("/Controls;component/Images/screen.png", UriKind.Relative));
            this.Content = scrGrid;
            Resize(null, null);
            scrVerticalOffset = scr.VerticalOffset;
        }

        /// <summary>
        /// 图片页面浏览模式（单页/多页）
        /// </summary>
        private enum PageType
        {
            /// <summary>
            /// 多页模式（Multi-page Application）
            /// </summary>
            MPA,
            /// <summary>
            /// 单页模式（Single-page Application）
            /// </summary>
            SPA
        }

    }
}
