using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

namespace ImagesListC
{
    /// <summary>
    /// 点击图片的信息类
    /// </summary>
    public class ImageInfo : EventArgs
    {
        /// <summary>
        /// 选中图片的下标
        /// </summary>
        public ImageAndValue ImgAndValue;
        /// <summary>
        /// 图像弹出
        /// </summary>
        /// <param name="imgAndValue">图像信息</param>
        public ImageInfo(ImageAndValue imgAndValue)
        {
            this.ImgAndValue = imgAndValue;
        }
    }

    /// <summary>
    /// 图像弹出事件处理程序
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="ilpea"></param>
    public delegate void ImagePopupEventHandler(object sender, ImageInfo imgInfo);

    public class ImageListC : Form
    {
        #region 矩形容器

        /// <summary>
        /// 图片显示矩形
        /// </summary>
        private Rectangle recImages;
        /// <summary>
        /// 图片组显示矩形
        /// </summary>
        private Rectangle recImageGroup;

        #endregion

        #region 私有字段

        /// <summary>
        /// 滑动条
        /// </summary>
        private ScrollBarC Scrollc = new ScrollBarC();
        /// <summary>
        /// 当前图片组的索引
        /// </summary>
        private int CurrentImageGroupIndex = 0;
        /// <summary>
        /// 焦点组索引
        /// </summary>
        private int FocusGroupIndex = -1;
        /// <summary>
        /// 显示组名称标签
        /// </summary>
        private Label groupL = new Label();
        /// <summary>
        /// 显示行数的索引
        /// </summary>
        private int showRowIndex = 0;
        private int ShowRowIndex
        {
            get => showRowIndex;
            set
            {
                if (showRowIndex == value) return;

                showRowIndex = value;
                Scrollc.Value = value;
            }
        }
        /// <summary>
        /// 显示行数的最大索引（最大行数-显示行数）
        /// </summary>
        private int showRowMaxIndex = 0;
        /// <summary>
        /// 是否按下
        /// </summary>
        private bool isPress = false;
        /// <summary>
        /// 当前选中图片的X坐标
        /// </summary>
        private int selectedX = -1;
        /// <summary>
        /// 当前选中图片的Y坐标
        /// </summary>
        private int selectedY = -1;
        /// <summary>
        /// 显示行数
        /// </summary>
        private int showRows = 6;
        /// <summary>
        /// 显示列数
        /// </summary>
        private int showColumns = 12;
        /// <summary>
        /// 显示每一张图片的宽
        /// </summary>
        private int imageWidth = 25;
        /// <summary>
        /// 显示每一张图片的高
        /// </summary>
        private int imageHeight = 25;
        /// <summary>
        /// 图片之间间距宽
        /// </summary>
        private int spacingWidth = 10;
        /// <summary>
        /// 图片之间高间距
        /// </summary>
        private int spacingHeight = 10;
        /// <summary>
        /// 左边留白大小
        /// </summary>
        private int leftLeaveBlank = 15;
        /// <summary>
        /// 右边留白大小
        /// </summary>
        private int rightLeaveBlank = 15;
        /// <summary>
        /// 顶部留白大小
        /// </summary>
        private int topLeaveBlank = 40;
        /// <summary>
        /// 底部留白大小
        /// </summary>
        private int bottomLeaveBlank = 15;
        /// <summary>
        /// 显示组图片的高
        /// </summary>
        private int groupHeight = 40;
        /// <summary>
        /// 显示组图片的高宽
        /// </summary>
        private int groupWidth = 50;

        #endregion

        #region 公共属性

        /// <summary>
        /// 整体背景颜色
        /// </summary>
        public Color BackgroundColor { get; set; } = Color.FromArgb(255, 255, 255);

        /// <summary>
        /// 图片框架水平线的颜色
        /// </summary>
        public Color HLinesColor { get; set; } = Color.FromArgb(222, 222, 222);
        /// <summary>
        /// 图片框架垂直线的颜色
        /// </summary>
        public Color VLinesColor { get; set; } = Color.FromArgb(165, 182, 222);
        /// <summary>
        /// 选中图片背景颜色
        /// </summary>
        public Color SelectedBackColor { get; set; } = Color.FromArgb(243, 243, 244);
        /// <summary>
        /// 选中图片边框颜色
        /// </summary>
        public Color SelectedBorderColor { get; set; } = Color.FromArgb(0, 16, 123);

        /// <summary>
        /// 组信息背景颜色
        /// </summary>
        public Color GroupBackColor { get; set; } = Color.FromArgb(243, 243, 244);
        /// <summary>
        /// 选中组背景颜色
        /// </summary>
        public Color SelectedGBackColor { get; set; } = Color.FromArgb(255, 255, 255);
        /// <summary>
        /// 焦点组背景颜色，鼠标移动上去的颜色
        /// </summary>
        public Color FucusGBackColor { get; set; } = Color.FromArgb(225, 225, 226);

        /// <summary>
        /// 按下鼠标选中图片X坐标偏移 多少像素
        /// </summary>
        public int PressXOffset { get; set; } = 2;
        /// <summary>
        /// 按下鼠标选中图片Y坐标偏移 多少像素
        /// </summary>
        public int PressYOffset { get; set; } = 2;


        #endregion

        #region 事件

        /// <summary>
        /// 图片的弹出事件
        /// </summary>
        public event ImagePopupEventHandler ImageClick = null;

        #endregion

        #region 初始化窗体信息

        public ImageListC()
        {
            NewThis();//初始化必要信息
        }

        #endregion

        /// <summary>
        /// 加载图片容器基本信息
        /// </summary>
        public bool LoadImageContainer()
        {
            recImages = new Rectangle(leftLeaveBlank, topLeaveBlank, (imageWidth + spacingWidth) * showColumns, (imageHeight + spacingHeight) * showRows);//显示图片容器的大小和位置
            this.Size = new Size(leftLeaveBlank + recImages.Width + rightLeaveBlank, topLeaveBlank + recImages.Height + bottomLeaveBlank + groupHeight);//显示窗体的大小
            recImageGroup = new Rectangle(0, topLeaveBlank + recImages.Height + bottomLeaveBlank, this.Width, groupHeight);//显示组信息容器

            groupL.Location = new Point(leftLeaveBlank, topLeaveBlank / 3);//设置显示组名称的位置
            groupL.Font = new Font("Verdana", (float)topLeaveBlank / 3, FontStyle.Regular, GraphicsUnit.Pixel);//设置显示组名称的大小
            groupL.AutoSize = true;
            groupL.BackColor = Color.Transparent;//设置显示组名称的背景透明

            Scrollc.Size = new Size(11, recImages.Height);//滑动条大小
            Scrollc.Location = new Point(leftLeaveBlank + recImages.Width + rightLeaveBlank - Scrollc.Width - 2, recImages.Y);//滑动条位置
            //Scrollc.ScrollStyle = ScrollBarC.ScrollExStyle.thinSlideway;
            //Scrollc.BackgroundColor = Color.Gray;
            Scrollc.BackColor = Color.Transparent;
            //Scrollc.IsGloba = true;

            LoadImageData();//加载图片数据

            if (ImageLS.ImageListS.Count < 1)
                return false;

            return true;
        }
        /// <summary>
        /// 加载图片容器基本信息
        /// </summary>
        /// <param name="showRows">显示的行数</param>
        /// <param name="showColumns">显示的列数</param>
        /// <param name="imageW_H">图片的高和宽</param>
        /// <param name="spacingW_H">图片间距的高和宽</param>
        /// <param name="groupW_H">显示组的高和宽</param>
        /// <param name="leftLeaveBlank">左边界留白</param>
        /// <param name="rightLeaveBlank">右边界留白</param>
        /// <param name="topLeaveBlank">上边界留白</param>
        /// <param name="bottomLeaveBlank">下边界留白</param>
        /// <returns></returns>
        public bool LoadImageContainer(int showRows, int showColumns, int imageW_H, int spacingW_H, int groupW_H, int leftLeaveBlank, int rightLeaveBlank, int topLeaveBlank, int bottomLeaveBlank)
        {
            this.leftLeaveBlank = leftLeaveBlank;
            this.rightLeaveBlank = rightLeaveBlank;
            this.topLeaveBlank = topLeaveBlank;
            this.bottomLeaveBlank = bottomLeaveBlank;

            return LoadImageContainer(showRows, showColumns, imageW_H, imageW_H, spacingW_H, spacingW_H, groupW_H, groupW_H);
        }
        /// <summary>
        /// 加载图片容器基本信息
        /// </summary>
        /// <param name="showRows">显示的行数</param>
        /// <param name="showColumns">显示的列数</param>
        /// <param name="imageWidth">显示的图片宽</param>
        /// <param name="imageHeight">显示的图片高</param>
        /// <param name="spacingWidth">图片之间间距宽</param>
        /// <param name="spacingHeight">图片之间间距高</param>
        /// <param name="groupWidth">显示组的宽</param>
        /// <param name="groupHeight">显示组的高</param>
        /// <returns></returns>
        public bool LoadImageContainer(int showRows, int showColumns, int imageWidth, int imageHeight, int spacingWidth, int spacingHeight, int groupWidth, int groupHeight)
        {
            this.showRows = showRows;
            this.showColumns = showColumns;
            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;
            this.spacingWidth = spacingWidth;
            this.spacingHeight = spacingHeight;
            this.groupWidth = groupWidth;
            this.groupHeight = groupHeight;

            return LoadImageContainer();
        }

        #region 重写事件

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (ImageLS.ImageListS.Count < 1) return;

            DrawBorder(e.Graphics);//绘制框架

            DrawImages(e.Graphics);//绘制图片

            DrawGroup(e.Graphics);//绘制图片组信息
        }
        /// <summary>
        /// 滚轮滚动事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (ImageLS.ImageListS.Count < 1) return;

            base.OnMouseWheel(e);

            if (!recImages.Contains(e.Location)) return;

            if (e.Delta < 0 && ShowRowIndex < showRowMaxIndex)//向下滑
            {
                ShowRowIndex = Math.Min(showRowMaxIndex, ShowRowIndex + 2);
                this.Invalidate(recImages);
            }
            else if (e.Delta > 0 && ShowRowIndex > 0)
            {
                ShowRowIndex = Math.Max(0, ShowRowIndex - 2);
                this.Invalidate(recImages);
            }
        }
        /// <summary>
        /// 鼠标移动
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (ImageLS.ImageListS.Count < 1) return;

            base.OnMouseMove(e);

            if (!recImages.Contains(e.Location))//不在图片区域
            {
                selectedX = selectedY = -1;
                this.Invalidate(recImages);
            }
            else
            {
                //还是之前的坐标
                if ((e.Location.X - leftLeaveBlank) / (imageWidth + spacingWidth) != selectedX || (e.Location.Y - topLeaveBlank) / (imageHeight + spacingHeight) != selectedY)
                {
                    selectedX = (e.Location.X - leftLeaveBlank) / (imageWidth + spacingWidth);
                    selectedY = (e.Location.Y - topLeaveBlank) / (imageHeight + spacingHeight);
                    this.Invalidate(recImages);
                }
            }
            if (!recImageGroup.Contains(e.Location))//不在组区域内
            {
                FocusGroupIndex = -1;
                this.Invalidate(recImageGroup);
            }
            else
            {
                if (FocusGroupIndex == e.Location.X / groupWidth) return;
                else
                {
                    FocusGroupIndex = e.Location.X / groupWidth;
                    if (FocusGroupIndex > ImageLS.ImageListS.Count - 1)
                        FocusGroupIndex = -1;
                    this.Invalidate(recImageGroup);
                }
            }
        }
        /// <summary>
        /// 鼠标移开
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeave(EventArgs e)
        {
            if (ImageLS.ImageListS.Count < 1) return;

            base.OnMouseLeave(e);
            selectedX = selectedY = FocusGroupIndex = -1;
            this.Invalidate();
        }
        /// <summary>
        /// 鼠标按下
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (ImageLS.ImageListS.Count < 1) return;

            base.OnMouseDown(e);

            if (recImages.Contains(e.Location))//在图片区域
            {
                isPress = true;
            }

            this.Invalidate(recImages);
        }
        /// <summary>
        /// 鼠标弹起
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (ImageLS.ImageListS.Count < 1) return;

            base.OnMouseUp(e);

            isPress = false;

            int selectedImageIndex = selectedY != -1 && selectedX != -1 ? (showRowIndex + selectedY) * showColumns + selectedX : -1;//选中图片的下标
            if (ImageClick != null && selectedImageIndex >= 0 && selectedImageIndex < ImageLS.ImageListS[CurrentImageGroupIndex].Images.Count)//图片点击的事件不为空并且有这个张图
            {
                ImageClick(this, new ImageInfo(ImageLS.ImageListS[CurrentImageGroupIndex].Images[selectedImageIndex]));
                selectedX = selectedY = -1;
                this.Hide();
            }
            else if (FocusGroupIndex != -1)//有焦点组
            {
                CurrentImageGroupIndex = FocusGroupIndex;
                ShowRowIndex = 0;
                selectedY = selectedX = -1;
                this.Invalidate();
            }
            else
            {
                this.Invalidate(recImages);
            }
        }
        /// <summary>
        /// 键盘按下
        /// </summary>
        /// <param name="e"></param>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (ImageLS.ImageListS.Count < 1) return false;

            if (keyData == Keys.Escape)//退出
            {
                selectedX = selectedY = -1;
                this.Hide();
                return base.ProcessDialogKey(keyData);
            }
            if (selectedX == -1 || selectedY == -1)//如果没有选中，选中第一个
            {
                selectedX = 0;
                selectedY = 0;
                this.Invalidate(recImages);
            }
            else
            {
                switch (keyData)
                {
                    case Keys.Down:
                        if (selectedY < showRows - 1)
                        {
                            selectedY++;
                        }
                        else if (ShowRowIndex < showRowMaxIndex)
                        {
                            ShowRowIndex++;
                        }
                        this.Invalidate(recImages);
                        break;
                    case Keys.Up:
                        if (selectedY > 0)
                        {
                            selectedY--;
                        }
                        else if (ShowRowIndex > 0)
                        {
                            ShowRowIndex--;
                        }
                        this.Invalidate(recImages);
                        break;
                    case Keys.Right:
                        if (selectedX < showColumns - 1)
                        {
                            selectedX++;
                            this.Invalidate(recImages);
                        }
                        break;
                    case Keys.Left:
                        if (selectedX > 0)
                        {
                            selectedX--;
                            this.Invalidate(recImages);
                        }
                        break;
                    case Keys.Home:
                        ShowRowIndex = 0;
                        selectedX = selectedY = 0;
                        this.Invalidate(recImages);
                        break;
                    case Keys.End:
                        ShowRowIndex = showRowMaxIndex;
                        selectedX = 0;
                        selectedY = showRows - 1;
                        this.Invalidate(recImages);
                        break;
                    case Keys.Enter:
                    case Keys.Space:
                        // 回车和空格返回信息
                        int selectedImageIndex = selectedY != -1 && selectedX != -1 ? (showRowIndex + selectedY) * showColumns + selectedX : -1;//选中图片的下标
                        if (ImageClick != null && selectedImageIndex >= 0 && selectedImageIndex < ImageLS.ImageListS[CurrentImageGroupIndex].Images.Count)//图片点击的事件不为空并且有这个张图
                        {
                            ImageClick(this, new ImageInfo(ImageLS.ImageListS[CurrentImageGroupIndex].Images[selectedImageIndex]));
                            selectedX = selectedY = -1;
                            this.Hide();
                        }
                        break;
                }
            }

            return base.ProcessDialogKey(keyData);
        }
        /// <summary>
        /// 失去焦点
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            this.Hide();
        }

        #endregion

        #region 方法

        /// <summary>
        /// 加载图片数据
        /// </summary>
        private void LoadImageData()
        {
            try
            {
                string headImageStr = "_h";
                string imagePath = Application.StartupPath + @"\images\System\";
                ImageLS.EmojiLoad(this);//加载图片数据
                if (ImageLS.ImageListS == null || ImageLS.ImageListS.Count < 1)
                {
                    ImageLS.ImageListS = new List<ImageListr>();
                    List<string> filesName = new DirectoryInfo(imagePath).GetFiles().Where(f => !f.Name.Contains(".txt")).Select(f => f.Name).ToList();//得到文件下的所有文件
                    List<string> imgHFile = filesName.Where(f => f.Contains(headImageStr)).ToList();//得到所有的头图片
                    foreach (var hFileName in imgHFile)
                    {
                        string hName = Path.GetFileNameWithoutExtension(hFileName).Replace(headImageStr, "");
                        ImageListr ilr = new ImageListr(hName, new Bitmap(imagePath + hFileName));//添加一个集合
                        foreach (var fName in filesName)
                        {
                            if (hFileName == fName) continue;
                            if (fName.Remove(fName.IndexOf('_')) == hFileName.Remove(hFileName.IndexOf('_')))
                            {
                                string txtFileName = Path.GetFileNameWithoutExtension(fName) + ".txt";//得到对应txt文件，如果没有就跳过
                                if (!File.Exists(imagePath + txtFileName)) continue;

                                Bitmap[] bitmaps = ImageLS.GetImageCut(new Bitmap(imagePath + fName), 40, 40, 8, 8);//图片集合


                                List<string> imgValues = new List<string>();//图片对应文字集合
                                using (StreamReader sr = new StreamReader(imagePath + txtFileName))
                                {
                                    string lineText = string.Empty;
                                    while ((lineText = sr.ReadLine()) != null)
                                    {
                                        imgValues.Add(lineText);
                                        if (imgValues.Count == bitmaps.Length)
                                            break;
                                    }
                                }

                                //将图片加入集合
                                int countImgAndValue = bitmaps.Length > imgValues.Count ? imgValues.Count : bitmaps.Length;//得到两个里短的一个
                                for (int i = 0; i < countImgAndValue; i++)
                                {
                                    ilr.Images.Add(new ImageAndValue(imgValues[i], bitmaps[i]));
                                }
                            }
                        }
                        if (ilr.Images.Count > 0)
                            ImageLS.ImageListS.Add(ilr);
                    }
                    ImageLS.EmojiSave();
                }
            }
            catch (Exception)
            { }
        }
        /// <summary>
        /// 窗体加载
        /// </summary>
        private void NewThis()
        {
            // 设置控件属性（防止闪）
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            this.DoubleBuffered = true;//设置本窗体
            //窗体样式
            FormBorderStyle = FormBorderStyle.None;//无边框
            WindowState = FormWindowState.Minimized;//最小化
            WindowState = FormWindowState.Normal;//窗口大小正常
            ShowInTaskbar = false;//不显示在任务栏
            TopMost = true;//最顶层
            this.Show();//显示
            this.Hide();//隐藏

            this.Controls.Add(groupL);//添加显示组名称标签
            this.Controls.Add(Scrollc);//添加滑动条
            Scrollc.Scroll += Scrollc_Scroll;
        }
        /// <summary>
        /// 滚动条的滚动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scrollc_Scroll(object sender, ScrollEventArgs e)
        {
            this.ShowRowIndex = e.NewValue;
            this.Invalidate(recImages);
        }
        /// <summary>
        /// 显示
        /// </summary>
        /// <param name="x">显示位置的X坐标</param>
        /// <param name="y">显示位置的Y坐标</param>
        public void Show(int x, int y)
        {
            Rectangle screenArea = Screen.GetWorkingArea(this);//获取屏幕大小
            if (x < 0)
                x = 0;
            if (x + this.Width > screenArea.Width)
                x = screenArea.Width - this.Width;
            if (y < 0)
                y = 0;
            if (y + this.Height > screenArea.Height)
                y = screenArea.Height - this.Height;
            base.Left = x;
            base.Top = y;
            this.Show();
        }
        /// <summary>
        /// 绘制框架
        /// </summary>
        /// <param name="g"></param>
        private void DrawBorder(Graphics g)
        {
            g.FillRectangle(new SolidBrush(BackgroundColor), 0, 0, this.Width, this.Height);
            //绘图片容器的横线
            for (int i = 0; i <= showRows; i++)
                g.DrawLine(new Pen(HLinesColor), recImages.X, recImages.Y + i * (imageHeight + spacingHeight), recImages.X + recImages.Width, recImages.Y + i * (imageHeight + spacingHeight));
            //绘制图片容器的纵线
            for (int i = 0; i <= showColumns; i++)
                g.DrawLine(new Pen(VLinesColor), recImages.X + i * (imageWidth + spacingWidth), recImages.Y, recImages.X + i * (imageWidth + spacingWidth), recImages.Y + +recImages.Height);
            g.FillRectangle(new SolidBrush(GroupBackColor), recImageGroup);
        }

        /// <summary>
        /// 绘制图片
        /// </summary>
        /// <param name="g"></param>
        private void DrawImages(Graphics g)
        {
            int skip = Math.Max(0, ShowRowIndex * showColumns);//需要跳过的图片
            int loadImageIndex = 0;
            for (int i = 0; i < showRows; i++)
            {
                for (int j = 0; j < showColumns; j++)
                {
                    loadImageIndex = skip + i * showColumns + j;
                    if (loadImageIndex < ImageLS.ImageListS[CurrentImageGroupIndex].Images.Count)//有这个图片
                    {
                        if (i == selectedY && j == selectedX)//是当前焦点图片
                        {
                            g.FillRectangle(new SolidBrush(SelectedBackColor), leftLeaveBlank + j * (imageWidth + spacingWidth), topLeaveBlank + i * (imageHeight + spacingHeight), imageWidth + spacingWidth, imageHeight + spacingHeight);
                            g.DrawRectangle(new Pen(SelectedBorderColor), leftLeaveBlank + j * (imageWidth + spacingWidth), topLeaveBlank + i * (imageHeight + spacingHeight), imageWidth + spacingWidth, imageHeight + spacingHeight);
                            if (isPress)//是否按下
                                g.DrawImage(ImageLS.ImageListS[CurrentImageGroupIndex].Images[loadImageIndex].Image, recImages.X + (imageWidth + spacingWidth) * j + spacingWidth / 2 + PressXOffset, recImages.Y + (imageHeight + spacingHeight) * i + spacingHeight / 2 + PressYOffset, imageWidth, imageHeight);//绘制被点击图片
                            else
                                g.DrawImage(ImageLS.ImageListS[CurrentImageGroupIndex].Images[loadImageIndex].Image, recImages.X + (imageWidth + spacingWidth) * j + spacingWidth / 2, recImages.Y + (imageHeight + spacingHeight) * i + spacingHeight / 2, imageWidth, imageHeight);//绘制图片
                        }
                        else
                            g.DrawImage(ImageLS.ImageListS[CurrentImageGroupIndex].Images[loadImageIndex].Image, recImages.X + (imageWidth + spacingWidth) * j + spacingWidth / 2, recImages.Y + (imageHeight + spacingHeight) * i + spacingHeight / 2, imageWidth, imageHeight);//绘制图片
                    }
                    else
                        return;
                }
            }
        }

        /// <summary>
        /// 绘制底部分组
        /// </summary>
        /// <param name="g"></param>
        private void DrawGroup(Graphics g)
        {
            int leftBorder = groupWidth / 4;//组的左外边框
            int topBorder = groupHeight / 4;//组的上外边框
            for (int i = 0; i < ImageLS.ImageListS.Count; i++)
            {
                if (i == CurrentImageGroupIndex)//如果是当前组绘制背景颜色
                {
                    showRowMaxIndex = (int)Math.Ceiling((double)ImageLS.ImageListS[CurrentImageGroupIndex].Images.Count / (double)showColumns) - showRows;//得到最大显示索引

                    g.FillRectangle(new SolidBrush(SelectedGBackColor), i * groupWidth, recImageGroup.Y, groupWidth, groupHeight);
                    groupL.Text = ImageLS.ImageListS[i].Name;//显示组名称

                    Scrollc.ContentSize = showRowMaxIndex + showRows;//设置滑动条信息
                    Scrollc.ShowContentSize = Scrollc.LargeChange = showRows;
                    Scrollc.Maximum = showRowMaxIndex;
                }
                if (i == FocusGroupIndex)//当前焦点组
                {
                    g.FillRectangle(new SolidBrush(FucusGBackColor), i * groupWidth, recImageGroup.Y, groupWidth, groupHeight);
                }
                if (ImageLS.ImageListS[i].Imager == null)
                    ImageLS.ImageListS[i].Imager = new Bitmap(10, 10);
                g.DrawImage(ImageLS.ImageListS[i].Imager, i * groupWidth + leftBorder, recImageGroup.Y + topBorder, groupWidth / 2, groupHeight / 2);
            }
        }

        #endregion
    }
}