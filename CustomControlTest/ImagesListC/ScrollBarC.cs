using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ImagesListC
{
    [Designer(typeof(ScrollBarControlDesigner))]
    [DefaultEvent("Scroll")]
    [DefaultProperty("Value")]
    public partial class ScrollBarC : Control
    {
        public ScrollBarC()
        {
            // 设置控件属性（防止闪）
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            this.DoubleBuffered = true;//设置本窗体

            InitializeComponent();

            this.SetUpScrollBar();//设置滑动条

            this.progressTimer.Interval = 20;//计时器，当鼠标在 滑块上方下方或箭头按钮上点击后 持续移动 直到松开鼠标或到需要的点
            this.progressTimer.Tick += this.ProgressTimerTick;//计时器启动后经过每指定时间执行事件

            this.Disposed += ScrollBarC_Disposed;//释放控件时
        }
        /// <summary>
        /// 加载布局
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Size = new Size(11, 200);
            this.ResumeLayout(false);

        }
        /// <summary>
        /// 处理滑块更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgressTimerTick(object sender, EventArgs e)
        {
            this.ProgressThumb(true);
        }
        /// <summary>
        /// 注销资源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollBarC_Disposed(object sender, EventArgs e)
        {
            //注销计时器事件
            this.progressTimer.Tick -= this.ProgressTimerTick;

            if (this.progressTimer != null)
                this.progressTimer.Dispose();
            this.progressTimer.Tick -= this.ProgressTimerTick;
        }

        #region 重写事件

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="pe"></param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            DrawThumb(pe.Graphics, this.thumbState, this.orientation);//绘制滑块
            DrawArrowButton(pe.Graphics, this.orientation);//绘制按钮（上/下）


            // 刷新被点击的轨道块信息
            if (this.topBarClicked)//点击了滑块上方的轨道
            {
                if (this.orientation == ScrollBarOrientation.Vertical)//垂直滑动条
                {
                    this.clickedBarRectangle.Y = this.thumbTopLimit;
                    this.clickedBarRectangle.Height = this.thumbRectangle.Y - this.thumbTopLimit;
                }
                else
                {
                    this.clickedBarRectangle.X = this.thumbTopLimit;
                    this.clickedBarRectangle.Width = this.thumbRectangle.X - this.thumbTopLimit;
                }
            }
            else if (this.bottomBarClicked)
            {
                if (this.orientation == ScrollBarOrientation.Vertical)
                {
                    this.clickedBarRectangle.Y = this.thumbRectangle.Bottom + 1;
                    this.clickedBarRectangle.Height = this.thumbBottomLimitBottom - this.clickedBarRectangle.Y;
                }
                else
                {
                    this.clickedBarRectangle.X = this.thumbRectangle.Right + 1;
                    this.clickedBarRectangle.Width = this.thumbBottomLimitBottom - this.clickedBarRectangle.X;
                }
            }
        }
        /// <summary>
        /// 鼠标按下
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();

            if (e.Button == MouseButtons.Left)
            {
                this.ContextMenuStrip = null;//禁止上下文菜单

                Point mouseLocation = e.Location;//获取鼠标相对于控件的坐标

                if (thumbRectangle.Contains(mouseLocation))//鼠标在滑块上
                {
                    thumbClicked = true;//滑块被点击
                    thumbPosition = orientation == ScrollBarOrientation.Vertical ? mouseLocation.Y - thumbRectangle.Y : mouseLocation.X - thumbRectangle.X;
                    thumbState = ScrollBarState.Pressed;//滑块被按下

                    Invalidate(this.thumbRectangle);//重绘滑块
                }
                else if (topArrowRectangle.Contains(mouseLocation))//在顶部按钮上
                {
                    topArrowClicked = true;//顶部按钮被点击
                    topButtonState = ScrollBarState.Pressed;//顶部按钮被按下

                    this.Invalidate(this.topArrowRectangle);//重绘顶部按钮

                    ProgressThumb(true);//开始移动滑块
                }
                else if (bottomArrowRectangle.Contains(mouseLocation))//在底部按钮上
                {
                    bottomArrowClicked = true;//底部按钮被点击
                    bottomButtonState = ScrollBarState.Pressed;//底部按钮被按下

                    this.Invalidate(this.bottomArrowRectangle);//重绘底部按钮

                    this.ProgressThumb(true);//开始移动滑块
                }
                else//点击了轨道
                {
                    trackPosition = orientation == ScrollBarOrientation.Vertical ? mouseLocation.Y : mouseLocation.X;//点击轨道的位置

                    //如果点击轨道的位置小于滑块的位置就是点击了上轨道，否则就是下轨道
                    if (trackPosition < (this.orientation == ScrollBarOrientation.Vertical ? this.thumbRectangle.Y : this.thumbRectangle.X))
                    {
                        this.topBarClicked = true;
                    }
                    else
                    {
                        this.bottomBarClicked = true;
                    }

                    this.ProgressThumb(true);//移动滑块
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                trackPosition = orientation == ScrollBarOrientation.Vertical ? e.Y : e.X;
            }
        }
        /// <summary>
        /// 鼠标弹起
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
            {

                if (thumbClicked)//如果滑块被点击
                {
                    thumbClicked = false;//滑块取消点击
                    thumbState = ScrollBarState.Normal;//滑块恢复正常

                    this.OnScroll(new ScrollEventArgs(ScrollEventType.EndScroll, -1, this.value, this.scrollOrientation));//激活滚动事件
                }
                else if (topArrowClicked)//如果顶部按钮被点击
                {
                    topArrowClicked = false;//顶部按钮取消被点击
                    topButtonState = ScrollBarState.Normal;//顶部按钮正常
                    StopTimer();//停止计时器
                }
                else if (bottomArrowClicked)
                {
                    bottomArrowClicked = false;
                    bottomButtonState = ScrollBarState.Normal;
                    StopTimer();
                }
                else if (topBarClicked)//上面轨道被点击
                {
                    topBarClicked = false;//取消上轨道被点击
                    StopTimer();
                }
                else if (bottomBarClicked)//下轨道被点击
                {
                    bottomBarClicked = false;
                    StopTimer();
                }

                Invalidate();//重绘
            }
        }
        /// <summary>
        /// 滑动滚轮，只有垂直滚动条有有
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (orientation == ScrollBarOrientation.Vertical)
            {
                if (thumbState == ScrollBarState.Active || thumbState == ScrollBarState.Hot)
                {
                    if (e.Delta < 0)//向下滚动
                        Value += largeChange;
                    else
                        Value -= largeChange;
                }
            }
        }
        /// <summary>
        /// 鼠标进入，修改按钮和滑动块状态
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            bottomButtonState = ScrollBarState.Active;
            topButtonState = ScrollBarState.Active;
            thumbState = ScrollBarState.Active;

            Invalidate();
        }
        /// <summary>
        /// 鼠标移开，重置滚动条
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            this.ResetScrollStatus();
        }
        /// <summary>
        /// 鼠标移动
        /// </summary>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // 并且按下鼠标左键
            if (e.Button == MouseButtons.Left)
            {
                if (this.thumbClicked)//如果滑块被点击
                {
                    int oldScrollValue = value;//旧的滑块值

                    topButtonState = ScrollBarState.Active;//修改顶部及底部按钮状态
                    bottomButtonState = ScrollBarState.Active;

                    int pos = this.orientation == ScrollBarOrientation.Vertical ? e.Location.Y : e.Location.X;//获取鼠标相对于控件的位置

                    if (pos <= (thumbTopLimit + thumbPosition))// 移动到了顶部
                    {
                        ChangeThumbPosition(thumbTopLimit);//滑块移动到最顶部

                        value = minimum;
                    }
                    else if (pos >= (thumbBottomLimitTop + thumbPosition))//移动到最底部
                    {
                        this.ChangeThumbPosition(thumbBottomLimitTop);//滑块移动到最底部

                        value = maximum;
                    }
                    else
                    {
                        this.ChangeThumbPosition(pos - this.thumbPosition);//移动到指定滑动的位置

                        //得到当前的value
                        //滑块可以移动的范围，当前滑块的位置，按钮的大小
                        int pixelRange, thumbPos, arrowSize;

                        if (this.orientation == ScrollBarOrientation.Vertical)//垂直滚动条
                        {
                            pixelRange = this.Height - (2 * this.z_ArrowHeight) - thumbHeight - 4;//滑块可以移动的范围
                            thumbPos = thumbRectangle.Y;//当前滑块的位置
                            arrowSize = z_ArrowHeight;//按钮的大小
                        }
                        else
                        {
                            pixelRange = this.Width - (2 * this.z_ArrowWidth) - thumbWidth - 4;
                            thumbPos = thumbRectangle.X;
                            arrowSize = z_ArrowWidth;
                        }

                        float perc = 0f;

                        if (pixelRange != 0)
                        {
                            // 当前滑块上方轨道占整个轨道的百分之多少
                            perc = (float)(thumbPos - arrowSize) / (float)pixelRange;
                        }

                        //得到当前的value
                        this.value = Convert.ToInt32((perc * (maximum - minimum)) + minimum);
                    }

                    // 如果value值发生了改变，激活滑动事件
                    if (oldScrollValue != this.value)
                    {
                        this.OnScroll(new ScrollEventArgs(ScrollEventType.ThumbTrack, oldScrollValue, this.value, this.scrollOrientation));

                        this.Refresh();//重绘
                    }
                }
            }
            else if (!ClientRectangle.Contains(e.Location))//不在工作区
            {
                this.ResetScrollStatus();//重置滚动条
            }
            else if (e.Button == MouseButtons.None) // 只移动了鼠标
            {
                if (topArrowRectangle.Contains(e.Location))//如果在顶部按钮上
                {
                    topButtonState = ScrollBarState.Hot;//修改顶部按钮状态

                    //Invalidate(topArrowRectangle);//重绘
                }
                else if (bottomArrowRectangle.Contains(e.Location))//如果在底部按钮上
                {
                    bottomButtonState = ScrollBarState.Hot;

                    //Invalidate(bottomArrowRectangle);
                }
                else if (thumbRectangle.Contains(e.Location))//如果在滑块上
                {
                    thumbState = ScrollBarState.Hot;

                    Invalidate(thumbRectangle);
                }
                else if (this.ClientRectangle.Contains(e.Location))//如果在控件的工作区
                {
                    topButtonState = ScrollBarState.Active;
                    bottomButtonState = ScrollBarState.Active;
                    thumbState = ScrollBarState.Active;

                    Invalidate();
                }
            }
        }
        /// <summary>
        /// 执行设置此控件的指定范围的工作
        /// </summary>
        /// <param name="x">控件的新x值</param>
        /// <param name="y">控件的新y值</param>
        /// <param name="width">控件的宽</param>
        /// <param name="height">控件的高</param>
        /// <param name="specified">按位组合<see cref="BoundsSpecified"/> values.</param>
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // 仅在设计模式下-限制尺寸
            if (this.DesignMode)
            {
                if (this.orientation == ScrollBarOrientation.Vertical)
                {
                    if (height < (2 * this.z_ArrowHeight) + 10)
                    {
                        height = (2 * this.z_ArrowHeight) + 10;
                    }
                }
                else
                {
                    if (width < (2 * this.z_ArrowWidth) + 10)
                    {
                        width = (2 * this.z_ArrowWidth) + 10;
                    }
                }
            }

            base.SetBoundsCore(x, y, width, height, specified);

            if (this.DesignMode)
            {
                this.SetUpScrollBar();
            }
        }
        /// <summary>
        /// 控件的尺寸更改
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.SetUpScrollBar();
        }
        /// <summary>
        /// 处理对话框建，键盘按键
        /// </summary>
        /// <param name="keyData">One of the <see cref="System.Windows.Forms.Keys"/> values that represents the key to process.</param>
        /// <returns>如果键是由控件处理的，则为true，否则为false.</returns>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (!(thumbState == ScrollBarState.Active || thumbState == ScrollBarState.Hot || thumbState == ScrollBarState.Pressed) && !IsGloba)//当前不是焦点控件，并且不是全局检查键盘鼠标
                return base.ProcessDialogKey(keyData);
            //处理键盘事件
            Keys keyUp = Keys.Up;
            Keys keyDown = Keys.Down;

            if (this.orientation == ScrollBarOrientation.Horizontal)//如果是水平滚动条保存左右
            {
                keyUp = Keys.Left;
                keyDown = Keys.Right;
            }

            if (keyData == keyUp)
            {
                this.Value -= this.smallChange;

                return true;
            }

            if (keyData == keyDown)
            {
                this.Value += this.smallChange;

                return true;
            }

            if (keyData == Keys.PageUp)
            {
                this.Value = this.GetValue(false, true);

                return true;
            }

            if (keyData == Keys.PageDown)
            {
                if (this.value + this.largeChange > this.maximum)
                {
                    this.Value = this.maximum;
                }
                else
                {
                    this.Value += this.largeChange;
                }

                return true;
            }

            if (keyData == Keys.Home)
            {
                this.Value = this.minimum;

                return true;
            }

            if (keyData == Keys.End)
            {
                this.Value = this.maximum;

                return true;
            }

            return base.ProcessDialogKey(keyData);
        }
        /// <summary>
        /// 控件的启用更改
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            if (this.Enabled)
            {
                thumbState = ScrollBarState.Normal;
                topButtonState = ScrollBarState.Normal;
                bottomButtonState = ScrollBarState.Normal;
            }
            else
            {
                thumbState = ScrollBarState.Disabled;
                topButtonState = ScrollBarState.Disabled;
                bottomButtonState = ScrollBarState.Disabled;
            }

            this.Refresh();
        }
        /// <summary>
        /// 滚动条滚动时发生
        /// </summary>
        /// <param name="e">The <see cref="ScrollEventArgs"/> that contains the event data.</param>
        protected virtual void OnScroll(ScrollEventArgs e)
        {
            this.Scroll?.Invoke(this, e);
        }

        #endregion

        #region 事件

        /// <summary>
        /// 在滚动条滚动时发生
        /// </summary>
        [Category("Behavior")]
        [Description("滚动滚动条时引发")]
        public event ScrollEventHandler Scroll;

        #endregion

        #region 枚举

        /// <summary>
        /// 滚动条状态
        /// </summary>
        public enum ScrollBarState
        {
            /// <summary>
            /// 正常
            /// </summary>
            Normal,

            /// <summary>
            /// 热滚动条
            /// </summary>
            Hot,

            /// <summary>
            /// 滚动
            /// </summary>
            Active,

            /// <summary>
            /// 按下
            /// </summary>
            Pressed,

            /// <summary>
            /// 禁用
            /// </summary>
            Disabled
        }
        /// <summary>
        /// 滚动条方向
        /// </summary>
        public enum ScrollBarOrientation
        {
            /// <summary>
            /// 水平滚动条
            /// </summary>
            Horizontal,

            /// <summary>
            /// 垂直滚动条
            /// </summary>
            Vertical
        }
        /// <summary>
        /// 滚动条样式枚举
        /// </summary>
        public enum ScrollExStyle
        {
            /// <summary>
            /// 宽轨道
            /// </summary>
            thickSlideway,
            /// <summary>
            /// 无轨道
            /// </summary>
            noSlideway,
            /// <summary>
            /// 窄轨道
            /// </summary>
            thinSlideway,
            /// <summary>
            /// 自定义
            /// </summary>
            Custom
        };

        #endregion

        #region 暂时没有使用的属性

        /// <summary>
        /// 边框颜色
        /// </summary>
        private Color borderColor = Color.FromArgb(93, 140, 201);
        /// <summary>
        /// 边框颜色
        /// </summary>
        [Category("Appearance")]
        [Description("边框颜色（暂时没用）")]
        [DefaultValue(typeof(Color), "93, 140, 201")]
        public Color BorderColor
        {
            get
            {
                return this.borderColor;
            }

            set
            {
                this.borderColor = value;

                this.Invalidate();
            }
        }

        /// <summary>
        /// 禁用状态的背景颜色
        /// </summary>
        private Color disabledBorderColor = Color.Gray;
        /// <summary>
        /// 禁用状态的背景颜色
        /// </summary>
        [Category("Appearance")]
        [Description("禁用状态的背景颜色（暂时没用）")]
        [DefaultValue(typeof(Color), "Gray")]
        public Color DisabledBorderColor
        {
            get
            {
                return this.disabledBorderColor;
            }

            set
            {
                this.disabledBorderColor = value;

                this.Invalidate();
            }
        }



        /// <summary>
        /// 背景颜色
        /// </summary>
        private Color backgroundColor;
        /// <summary>
        /// 背景颜色
        /// </summary>
        [Category("自定义属性")]
        [Description("背景颜色（轨道）")]
        [DefaultValue(typeof(Color), "Color.White")]
        public Color BackgroundColor
        {
            get { return this.backgroundColor; }
            set
            {
                if (backgroundColor == value) return;

                backgroundColor = value;
                backImage = null;//背景图片为空
                SetBackImage();//从新加载轨道背景图片

            }
        }

        #endregion

        #region 属性

        /// <summary>
        /// 显示内容容器的大小
        /// </summary>
        private int showContentSize = 1;
        [Category("自定义属性")]
        [Description("显示内容高度内容的大小")]
        [DefaultValue(1)]
        /// <summary>
        /// 显示内容容器的大小
        /// </summary>
        public int ShowContentSize
        {
            get
            {
                return showContentSize;
            }
            set
            {
                if (showContentSize == value || value < 1) return;
                showContentSize = value;

                this.SetUpScrollBar();
                this.Refresh();
            }
        }

        /// <summary>
        /// 整体内容的大小
        /// </summary>
        private int contentSize = 1;
        /// <summary>
        /// 整体内容的大小
        /// </summary>
        [Category("自定义属性")]
        [Description("整体内容的大小")]
        [DefaultValue(1)]
        public int ContentSize
        {
            get
            {
                return contentSize;
            }
            set
            {
                if (contentSize == value || value < 1) return;
                contentSize = value;

                this.SetUpScrollBar();
                this.Refresh();
            }
        }

        /// <summary>
        /// 滑块的最小值
        /// </summary>
        private int minimum = 0;
        /// <summary>
        /// 滑块最小值
        /// </summary>
        [Category("自定义属性")]
        [Description("滑块最小值")]
        [DefaultValue(0)]
        public int Minimum
        {
            get
            {
                return this.minimum;
            }

            set
            {
                if (this.minimum == value || value < 0 || value >= this.maximum)
                {
                    return;
                }

                this.minimum = value;

                if (this.value < value)
                {
                    this.value = value;
                }

                if (this.largeChange > this.maximum - this.minimum)
                {
                    this.largeChange = this.maximum - this.minimum;
                }

                this.SetUpScrollBar();

                if (this.value < value)
                {
                    this.Value = value;
                }
                else
                {
                    this.ChangeThumbPosition(this.GetThumbPosition());

                    this.Refresh();
                }
            }
        }

        /// <summary>
        /// 滑块的最大值
        /// </summary>
        private int maximum = 100;
        /// <summary>
        /// 滑块的最大值
        /// </summary>
        [Category("自定义属性")]
        [Description("滑块的最大值")]
        [DefaultValue(100)]
        public int Maximum
        {
            get
            {
                return this.maximum;
            }

            set
            {
                if (value == this.maximum || value < 1 || value <= this.minimum)
                {
                    return;
                }

                this.maximum = value;

                if (this.largeChange > this.maximum - this.minimum)
                {
                    this.largeChange = this.maximum - this.minimum;
                }

                this.SetUpScrollBar();

                if (this.value > value)
                {
                    this.Value = this.maximum;
                }
                else
                {
                    this.ChangeThumbPosition(this.GetThumbPosition());

                    this.Refresh();
                }
            }
        }

        /// <summary>
        /// 移动较大的变化值
        /// </summary>
        private int largeChange = 10;
        /// <summary>
        /// 移动较大值
        /// </summary>
        [Category("自定义属性")]
        [Description("移动较大值.")]
        [DefaultValue(10)]
        public int LargeChange
        {
            get
            {
                return this.largeChange;
            }

            set
            {
                // 无效
                if (value == this.largeChange || value < this.smallChange || value < 2)
                {
                    return;
                }

                // 超出范围
                if (value > this.maximum - this.minimum)
                {
                    this.largeChange = this.maximum - this.minimum;
                }
                else
                {
                    this.largeChange = value;
                }

                this.SetUpScrollBar();
            }
        }

        /// <summary>
        /// 移动小变化值
        /// </summary>
        private int smallChange = 1;
        /// <summary>
        /// 移动小变化值
        /// </summary>
        [Category("自定义属性")]
        [Description("移动小变化值.")]
        [DefaultValue(1)]
        public int SmallChange
        {
            get
            {
                return this.smallChange;
            }

            set
            {
                if (value == this.smallChange || value < 1 || value >= this.largeChange)
                {
                    return;
                }

                this.smallChange = value;

                this.SetUpScrollBar();
            }
        }

        /// <summary>
        /// 滚动条轨道样式
        /// </summary>
        private ScrollExStyle scrollStyle;
        /// <summary>
        /// 滚动条轨道样式
        /// </summary>
        [Category("自定义属性")]
        [Description("滚动条轨道样式")]
        public ScrollExStyle ScrollStyle
        {
            get { return scrollStyle; }
            set
            {
                if (scrollStyle != value)
                {
                    scrollStyle = value;
                    SetUpScrollBar();
                    SetBackImage();//会立即重绘轨道背景

                }
            }
        }
        /// <summary>
        /// 背景图片（轨道）
        /// </summary>
        private Image backImage;
        /// <summary>
        /// 背景图片（轨道）
        /// </summary>
        [Category("自定义属性")]
        [Description("滚动条背景图片")]
        public Image BackImage
        {
            get { return backImage; }
            set
            {
                if (backImage != value)
                {
                    backImage = value;
                    SetBackImage();//会立即重绘轨道背景
                }
            }
        }

        /// <summary>
        /// 滑动块背景图片
        /// </summary>
        private Image thumbImage;
        /// <summary>
        /// 滑动块背景图片
        /// </summary>
        [Category("自定义属性")]
        [Description("滑块背景图片")]
        public Image ThumbImage
        {
            get
            {
                return thumbImage;
            }
            set
            {
                if (thumbImage != value)
                {
                    thumbImage = value;
                    _arrowbtns = InitArrawBtns();//加载箭头按键的图片
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 互动时滑块的背景图片
        /// </summary>
        private Image thumbHoverImage;
        /// <summary>
        /// 互动时滑块的背景图片
        /// </summary>
        [Category("自定义属性")]
        [Description("滑块经过图片")]
        public Image ThumbHoverImage
        {
            get { return thumbHoverImage; }
            set
            {
                if (thumbHoverImage != value)
                {
                    thumbHoverImage = value;
                    _arrowbtns = InitArrawBtns();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 滑动块背景颜色（不设置滑块背景图片有效）
        /// </summary>
        private Color thumbColor = Color.FromArgb(220, 220, 220);
        /// <summary>
        /// 滑动块背景颜色（不设置滑块背景图片有效）
        /// </summary>
        [Category("自定义属性")]
        [Description("滑动块背景颜色")]
        [DefaultValue(typeof(Color), "220,220,220")]
        public Color ThumbColor
        {
            get
            {
                return thumbColor;
            }
            set
            {
                if (thumbColor != value)
                {
                    thumbColor = value;
                    thumbImage = null;//清空图片
                    _arrowbtns = InitArrawBtns();//加载箭头按键的图片
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 互动时滑块的背景颜色（不设置滑块互动背景图片有效）
        /// </summary>
        private Color thumbHoverColor = Color.Gray;
        /// <summary>
        /// 互动时滑块的背景颜色（不设置滑块互动背景图片有效）
        /// </summary>
        [Category("自定义属性")]
        [Description("互动时滑块的背景颜色")]
        [DefaultValue(typeof(Color), "Color.Gray")]
        public Color ThumbHoverColor
        {
            get { return thumbHoverColor; }
            set
            {
                if (thumbHoverColor != value)
                {
                    thumbHoverColor = value;
                    thumbHoverImage = null;
                    _arrowbtns = InitArrawBtns();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 滚动条方向，水平-垂直
        /// </summary>
        private ScrollBarOrientation orientation = ScrollBarOrientation.Vertical;
        /// <summary>
        /// 滚动条方向，水平-垂直
        /// </summary>
        [Category("自定义属性")]
        [Description("滚动条方向")]
        [DefaultValue(ScrollBarOrientation.Vertical)]
        public ScrollBarOrientation Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                if (value == orientation) return;

                orientation = value;

                scrollOrientation = value == ScrollBarOrientation.Vertical ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll;//滚动事件需要的方向

                this.Size = new Size(Height, Width);//转换宽和高

                this.SetUpScrollBar();
            }
        }

        /// <summary>
        /// 滑块的高度（只有水平滑动条，并且滑动条样式是自定义才有用）
        /// </summary>
        private int thumbHeight = 11;
        /// <summary>
        /// 滑块的高度（只有水平滑动条，并且滑动条样式是自定义才有用）
        /// </summary>
        [Category("自定义属性")]
        [Description("滑块高度（只有水平滑动条，并且滑动条样式是自定义才有用）")]
        [DefaultValue(11)]
        public int ThumbHeight
        {
            get { return thumbHeight; }
            set
            {
                if (thumbHeight != value)
                {
                    thumbHeight = value;
                    this.SetUpScrollBar();
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// 滑块的宽度（只有垂直滑动条，并且滑动条样式是自定义才有用）
        /// </summary>
        private int thumbWidth = 11;
        /// <summary>
        /// 滑块的宽度（只有垂直滑动条，并且滑动条样式是自定义才有用）
        /// </summary>
        [Category("自定义属性")]
        [Description("滑块的宽度（只有垂直滑动条，并且滑动条样式是自定义才有用）")]
        [DefaultValue(11)]
        public int ThumbWidth
        {
            get { return thumbWidth; }
            set
            {
                if (thumbWidth != value)
                {
                    thumbWidth = value;
                    SetUpScrollBar();
                }
            }
        }

        /// <summary>
        /// 箭头宽
        /// </summary>
        private int arrowWidth;
        /// <summary>
        /// 箭头宽
        /// </summary>
        [Category("自定义属性")]
        [Description("箭头宽度")]
        [DefaultValue(0)]
        public int z_ArrowWidth
        {
            get { return arrowWidth; }
            set
            {
                if (arrowWidth != value)
                {
                    arrowWidth = value;
                    SetUpScrollBar();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 箭头高
        /// </summary>
        private int arrowHeight;
        /// <summary>
        /// 箭头高
        /// </summary>
        [Category("自定义属性")]
        [Description("箭头高度")]
        [DefaultValue(0)]
        public int z_ArrowHeight
        {
            get { return arrowHeight; }
            set
            {
                if (arrowHeight != value)
                {
                    arrowHeight = value;
                    SetUpScrollBar();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 箭头图标
        /// </summary>
        private Image arrowImage;
        /// <summary>
        /// 箭头图标
        /// </summary>
        [Category("自定义属性")]
        [Description("箭头图标")]
        public Image ArrowImage
        {
            get { return arrowImage; }
            set
            {
                if (arrowImage != value)
                {
                    arrowImage = value;
                    _arrowbtns = InitArrawBtns();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 互动时箭头图标
        /// </summary>
        private Image arrowHoverImage;
        /// <summary>
        /// 互动时箭头图标
        /// </summary>
        [Category("自定义属性")]
        [Description("互动时箭头图标")]
        public Image ArrowHoverImage
        {
            get { return arrowHoverImage; }
            set
            {
                if (arrowHoverImage != value)
                {
                    arrowHoverImage = value;
                    _arrowbtns = InitArrawBtns();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 滑块的值
        /// </summary>
        private int value;
        /// <summary>
        /// 获取或设置值
        /// </summary>
        [Category("自定义属性")]
        [Description("滑块的值")]
        [DefaultValue(0)]
        public int Value
        {
            get
            {
                return value;
            }

            set
            {
                if (this.value == value)
                    return;
                else if (value < this.value && this.value <= this.minimum)
                    return;
                else if (value > this.value && this.value >= this.maximum)
                    return;

                if (value < this.minimum)
                    value = this.minimum;
                else if (value > this.maximum)
                    value = this.maximum;

                this.value = value;

                // 移动滑块位置
                this.ChangeThumbPosition(this.GetThumbPosition());

                // 激活滑动事件
                this.OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, -1, this.value, this.scrollOrientation));

                this.Refresh();
            }
        }

        /// <summary>
        /// 是否全局，识别键盘鼠标
        /// </summary>
        [Category("自定义属性")]
        [Description("是否全局，识别键盘鼠标")]
        [DefaultValue(0)]
        public bool IsGloba { get; set; } = false;

        #endregion



        #region 字段

        /// <summary>
        /// 正在对滚动条进行许多更改，因此请停止绘画直到完成
        /// </summary>
        private bool inUpdate;
        /// <summary>
        /// 用于移动滑块的进度计时器
        /// </summary>
        private Timer progressTimer = new Timer();
        /// <summary>
        /// 滚动事件中的滚动方向 （垂直-水平）
        /// </summary>
        private ScrollOrientation scrollOrientation = ScrollOrientation.VerticalScroll;

        #region 轨道

        /// <summary>
        /// 轨道矩形
        /// </summary>
        private Rectangle channelRectangle;
        /// <summary>
        /// 是否点击了滑块上方的轨道
        /// </summary>
        private bool topBarClicked;
        /// <summary>
        /// 是否点击了滑块下方的轨道
        /// </summary>
        private bool bottomBarClicked;
        /// <summary>
        /// 被点击的轨道块，滑块上方或下放
        /// </summary>
        private Rectangle clickedBarRectangle;
        /// <summary>
        /// 点击轨道位置（像素）
        /// </summary>
        private int trackPosition;

        #endregion

        #region 滑块

        /// <summary>
        /// 滑块矩形
        /// </summary>
        private Rectangle thumbRectangle;
        /// <summary>
        /// 是否点击了滑块
        /// </summary>
        private bool thumbClicked;
        /// <summary>
        /// 鼠标点击滑块的当前位置（像素）
        /// </summary>
        private int thumbPosition;
        /// <summary>
        /// 滑块的状态
        /// </summary>
        private ScrollBarState thumbState = ScrollBarState.Normal;
        /// <summary>
        /// 滑块顶部的上限（可以移动的最小值，像素）
        /// </summary>
        private int thumbTopLimit;
        /// <summary>
        /// 滑块顶部的下限（可以移动的最大值，像素）
        /// </summary>
        private int thumbBottomLimitTop;
        /// <summary>
        /// 滑块底部的下限（像素）
        /// </summary>
        private int thumbBottomLimitBottom;

        #endregion

        #region 箭头按钮

        /// <summary>
        /// 顶部箭头矩形
        /// </summary>
        private Rectangle topArrowRectangle;
        /// <summary>
        /// 底部箭头矩形
        /// </summary>
        private Rectangle bottomArrowRectangle;
        /// <summary>
        /// 顶部箭头状态
        /// </summary>
        private ScrollBarState topButtonState = ScrollBarState.Normal;
        /// <summary>
        /// 底部箭头的状态
        /// </summary>
        private ScrollBarState bottomButtonState = ScrollBarState.Normal;
        /// <summary>
        /// 是否点击击了顶部按钮（向上移动一小段）
        /// </summary>
        private bool topArrowClicked;
        /// <summary>
        /// 是否单击了底部箭头（向下移动一小段）
        /// </summary>
        private bool bottomArrowClicked;
        /// <summary>
        /// 箭头图标集合（上下左右以及hover）
        /// </summary>
        private static Bitmap[] _arrowbtns;

        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 绘制滚动条向上、向下的按钮.
        /// </summary>
        /// <param name="g">绘图实例</param>
        /// <param name="state">状态</param>
        /// <param name="orientation">滚动条方向</param>
        private void DrawArrowButton(Graphics g, ScrollBarOrientation orientation)
        {
            if (_arrowbtns == null) return;
            int topIndex = 0;
            int bottomIndex = 0;
            if (topButtonState == ScrollBarState.Pressed | topButtonState == ScrollBarState.Hot | topButtonState == ScrollBarState.Active)//顶部按钮活动状态（显示不同图片）
            {
                topIndex = 1;//显示的图片下标加一
            }
            if (bottomButtonState == ScrollBarState.Pressed | bottomButtonState == ScrollBarState.Hot | bottomButtonState == ScrollBarState.Active)//底部按钮活动状态（显示不同图片）
            {
                bottomIndex = 1;//显示的图片下标加一
            }

            if (orientation == ScrollBarOrientation.Vertical)//垂直滚动条
            {
                g.DrawImage(_arrowbtns[0 + topIndex], this.topArrowRectangle);//顶部箭头矩形
                g.DrawImage(_arrowbtns[2 + bottomIndex], this.bottomArrowRectangle);//底部箭头矩形
            }
            else
            {
                g.DrawImage(_arrowbtns[4 + bottomIndex], this.bottomArrowRectangle);
                g.DrawImage(_arrowbtns[6 + topIndex], this.topArrowRectangle);
            }
        }
        /// <summary>
        /// 控制滑块的运动
        /// </summary>
        /// <param name="enableTimer">启动计时器位true，否则为false</param>
        private void ProgressThumb(bool enableTimer)
        {
            int scrollOldValue = this.value;//旧的滑块值
            ScrollEventType type = ScrollEventType.First;//移动类型
            int thumbSize, thumbPos;//滑块的大小和位置

            if (this.orientation == ScrollBarOrientation.Vertical)//垂直滑动条
            {
                thumbPos = this.thumbRectangle.Y;//滑块当前的y坐标
                thumbSize = this.thumbRectangle.Height;//滑块的高
            }
            else
            {
                thumbPos = this.thumbRectangle.X;
                thumbSize = this.thumbRectangle.Width;
            }

            // 点击向下按钮，或（滑块下方的轨道，并且点击轨道的位置大于滑块的位置）
            if (this.bottomArrowClicked || (this.bottomBarClicked && (thumbPos + thumbSize) < this.trackPosition))
            {
                type = this.bottomArrowClicked ? ScrollEventType.SmallIncrement : ScrollEventType.LargeIncrement;//移动范围

                this.value = this.GetValue(this.bottomArrowClicked, false);//获取滑块的新值（点击底部按钮是微小移动）

                if (this.value == this.maximum)//如果是移动到最大
                {
                    this.ChangeThumbPosition(this.thumbBottomLimitTop);//移动到最后

                    type = ScrollEventType.Last;//滑动条被移动到了最后
                }
                else
                {
                    this.ChangeThumbPosition(Math.Min(this.thumbBottomLimitTop, this.GetThumbPosition()));//按距离范围移动滑块
                }
            }
            //点击向上按钮，或（滑块上方轨道并且滑块的位置大于点击轨道的位置）
            else if (this.topArrowClicked || (this.topBarClicked && thumbPos > this.trackPosition))
            {
                type = this.topArrowClicked ? ScrollEventType.SmallDecrement : ScrollEventType.LargeDecrement;//移动范围

                this.value = this.GetValue(this.topArrowClicked, true);//获取滑块的新值（点击按钮是微小移动）

                if (this.value == this.minimum)
                {
                    this.ChangeThumbPosition(this.thumbTopLimit);//移动到顶部

                    type = ScrollEventType.First;
                }
                else
                {
                    this.ChangeThumbPosition(Math.Max(this.thumbTopLimit, this.GetThumbPosition()));//移动指定位置
                }
            }
            //没有在最顶部 点击向上，并且没有在最底部 点击向下（基本不会满足）
            else if (!((this.topArrowClicked && thumbPos == this.thumbTopLimit) || (this.bottomArrowClicked && thumbPos == this.thumbBottomLimitTop)))
            {
                this.ResetScrollStatus();

                return;
            }
            //滑块有移动
            if (scrollOldValue != this.value)
            {
                //激活滚动事件
                this.OnScroll(new ScrollEventArgs(type, scrollOldValue, this.value, this.scrollOrientation));

                this.Invalidate(this.channelRectangle);//重绘

                //启动计时器
                if (enableTimer)
                {
                    this.EnableTimer();
                }
            }
            else//滑块没有移动
            {
                if (this.topArrowClicked)
                {
                    type = ScrollEventType.SmallDecrement;
                }
                else if (this.bottomArrowClicked)
                {
                    type = ScrollEventType.SmallIncrement;
                }
                //激活滚动事件
                this.OnScroll(new ScrollEventArgs(type, this.value));
            }
        }
        /// <summary>
        /// 启动计时器，如果计时器已开启，加快时间
        /// </summary>
        private void EnableTimer()
        {
            // 启动计时器
            if (!this.progressTimer.Enabled)
            {
                this.progressTimer.Interval = 600;
                this.progressTimer.Start();
            }
            else
            {
                // 已经开启修改执行时间
                this.progressTimer.Interval = 10;
            }
        }
        /// <summary>
        /// 停止进度计时器
        /// </summary>
        private void StopTimer()
        {
            this.progressTimer.Stop();
        }
        /// <summary>
        /// 重置滚动条的滚动状态
        /// </summary>
        private void ResetScrollStatus()
        {
            // 获取鼠标位置
            Point pos = this.PointToClient(Cursor.Position);

            if (this.ClientRectangle.Contains(pos))//鼠标在工作区
            {
                this.bottomButtonState = ScrollBarState.Active;
                this.topButtonState = ScrollBarState.Active;
            }
            else
            {
                this.bottomButtonState = ScrollBarState.Normal;
                this.topButtonState = ScrollBarState.Normal;
            }

            // 如果鼠标在滑块上滑块修改滑块的状态
            thumbState = this.thumbRectangle.Contains(pos) ? ScrollBarState.Hot : ScrollBarState.Normal;

            //重置移动滑块属性
            bottomArrowClicked = bottomBarClicked = topArrowClicked = topBarClicked = false;

            this.StopTimer();

            this.Refresh();
        }
        /// <summary>
        /// 计算滑动条的新值
        /// </summary>
        /// <param name="smallIncrement">微小的更改为true，否则为false</param>
        /// <param name="up">向上运动是true，否则的false</param>
        /// <returns>新的滚动条值</returns>
        private int GetValue(bool smallIncrement, bool up)
        {
            int newValue;

            if (up)//向上
            {
                newValue = this.value - (smallIncrement ? this.smallChange : this.largeChange);//得到变化后的值

                newValue = newValue < this.minimum ? this.minimum : newValue;//小于最小值时等于最小值
            }
            else
            {
                newValue = this.value + (smallIncrement ? this.smallChange : this.largeChange);

                newValue = newValue > this.maximum ? this.maximum : newValue;//大于最大值时等于最大值
            }

            return newValue;
        }
        /// <summary>
        /// 设置轨道背景
        /// </summary>
        private void SetBackImage()
        {
            if (backImage == null) //如果背景图片为空
            {
                if (BackgroundColor != Color.Empty)
                {
                    backImage = new Bitmap(10, 10);
                    Graphics gg = Graphics.FromImage(backImage);
                    gg.FillRectangle(new SolidBrush(BackgroundColor), new Rectangle(0, 0, backImage.Width, backImage.Height));
                }
            }
            if (backImage == null) return;
            if (this.ScrollStyle == ScrollExStyle.noSlideway)//无轨道
            {
                this.BackgroundImage = null;
                if (this.Orientation == ScrollBarOrientation.Vertical)
                {
                    this.Width = 11;
                }
                else
                {
                    this.Height = 11;
                }
            }
            else if (this.ScrollStyle == ScrollExStyle.thinSlideway)//窄轨道
            {
                if (this.Orientation == ScrollBarOrientation.Vertical)//垂直滑动条
                {
                    this.Width = 11;
                    Image img = new Bitmap(10, 10);
                    Graphics g = Graphics.FromImage(img);
                    g.DrawImage(backImage, new Rectangle(5, 0, 1, 10), 0, 0, backImage.Width, backImage.Height, GraphicsUnit.Pixel);
                    if (img.Width > img.Height) img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    this.BackgroundImage = img;
                }
                else
                {
                    this.Height = 11;
                    Image img = backImage;
                    if (img.Width < img.Height) img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    this.BackgroundImage = img;
                }
            }
            else if (this.ScrollStyle == ScrollExStyle.thickSlideway)//宽轨道
            {
                if (this.Orientation == ScrollBarOrientation.Vertical)//垂直滑动条
                {
                    this.Width = 10;
                    Image img = backImage;
                    if (img.Width > img.Height) img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    this.BackgroundImage = img;
                }
                else
                {
                    this.Height = 10;
                    Image img = backImage;
                    if (img.Width < img.Height) img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    this.BackgroundImage = img;
                }
            }
            else
            {
                if (BackImage == null) { BackgroundImage = null; return; }
                if (Orientation == ScrollBarOrientation.Vertical)//垂直滑动条
                {
                    Image img = BackImage;
                    if (img.Width > img.Height) img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    this.BackgroundImage = img;
                }
                else
                {
                    Image img = BackImage;
                    if (img.Width < img.Height) img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    this.BackgroundImage = img;
                }
            }

            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;//拉伸布局
        }
        /// <summary>
        /// 初始化箭头图标
        /// </summary>
        /// <returns></returns>
        private Bitmap[] InitArrawBtns()
        {
            if (ArrowImage == null || ArrowHoverImage == null || z_ArrowHeight == 0 || z_ArrowWidth == 0) return null;

            Bitmap[] result = new Bitmap[8];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new Bitmap(z_ArrowWidth, z_ArrowHeight);
            }
            //将图片按要求旋转
            result[0] = new Bitmap(ArrowImage);
            result[1] = new Bitmap(ArrowHoverImage);

            Graphics g1 = Graphics.FromImage(result[2]);
            g1.DrawImage(ArrowImage, new Rectangle(0, 0, z_ArrowWidth, z_ArrowHeight), 0, 0, ArrowImage.Width, arrowImage.Height, GraphicsUnit.Pixel);
            result[2].RotateFlip(RotateFlipType.Rotate180FlipNone);

            Graphics g2 = Graphics.FromImage(result[3]);
            g2.DrawImage(ArrowHoverImage, new Rectangle(0, 0, z_ArrowWidth, z_ArrowHeight), 0, 0, ArrowHoverImage.Width, ArrowHoverImage.Height, GraphicsUnit.Pixel);
            result[3].RotateFlip(RotateFlipType.Rotate180FlipNone);

            Graphics g3 = Graphics.FromImage(result[4]);
            g3.DrawImage(ArrowImage, new Rectangle(0, 0, z_ArrowWidth, z_ArrowHeight), 0, 0, ArrowImage.Width, arrowImage.Height, GraphicsUnit.Pixel);
            result[4].RotateFlip(RotateFlipType.Rotate90FlipNone);

            Graphics g4 = Graphics.FromImage(result[5]);
            g4.DrawImage(ArrowHoverImage, new Rectangle(0, 0, z_ArrowWidth, z_ArrowHeight), 0, 0, ArrowHoverImage.Width, ArrowHoverImage.Height, GraphicsUnit.Pixel);
            result[5].RotateFlip(RotateFlipType.Rotate90FlipNone);

            Graphics g5 = Graphics.FromImage(result[6]);
            g5.DrawImage(ArrowImage, new Rectangle(0, 0, z_ArrowWidth, z_ArrowHeight), 0, 0, ArrowImage.Width, arrowImage.Height, GraphicsUnit.Pixel);
            result[6].RotateFlip(RotateFlipType.Rotate270FlipNone);

            Graphics g6 = Graphics.FromImage(result[7]);
            g6.DrawImage(ArrowHoverImage, new Rectangle(0, 0, z_ArrowWidth, z_ArrowHeight), 0, 0, ArrowHoverImage.Width, ArrowHoverImage.Height, GraphicsUnit.Pixel);
            result[7].RotateFlip(RotateFlipType.Rotate270FlipNone);

            g1.Dispose();
            g2.Dispose();
            g3.Dispose();
            g4.Dispose();
            g5.Dispose();
            g6.Dispose();

            //注销之前的箭头图标
            if (_arrowbtns != null)
            {
                foreach (var item in _arrowbtns)
                {
                    item.Dispose();
                }
                _arrowbtns = null;
            }

            return result;
        }
        /// <summary>
        /// 计算新的滑块位置
        /// </summary>
        /// <returns></returns>
        private int GetThumbPosition()
        {
            //滑块可以移动的范围，上下箭头大小
            int pixelRange, arrowSize;

            if (this.orientation == ScrollBarOrientation.Vertical)//垂直滑动条
            {
                pixelRange = this.Height - (2 * this.z_ArrowHeight) - this.thumbHeight - 4;
                arrowSize = this.z_ArrowHeight;
            }
            else
            {
                pixelRange = this.Width - (2 * this.z_ArrowWidth) - this.thumbWidth - 4;
                arrowSize = this.z_ArrowWidth;
            }

            int realRange = this.maximum - this.minimum;
            float perc = 0f;//滑块位置在滑动条的百分比

            if (realRange != 0)
            {
                perc = ((float)this.value - (float)this.minimum) / (float)realRange;
            }

            //float perc = (float)this.value/ (float)this.maximum;//和上面计算 滑块位置在滑动条的百分比一样

            //返回在最大/最下移动值之间的移动后位置
            return Math.Max(this.thumbTopLimit, Math.Min(
               this.thumbBottomLimitTop,
               Convert.ToInt32((perc * pixelRange) + arrowSize)));
        }
        /// <summary>
        /// 改变滑块的位置
        /// </summary>
        /// <param name="position">新的位置</param>
        private void ChangeThumbPosition(int position)
        {
            if (this.orientation == ScrollBarOrientation.Vertical)//如果是垂直滑动条
            {
                this.thumbRectangle.Y = position;
            }
            else
            {
                this.thumbRectangle.X = position;
            }
        }
        /// <summary>
        /// 绘制滑块
        /// </summary>
        /// <param name="g">绘制的实例</param>
        /// <param name="state">滚动条状态</param>
        /// <param name="orientation">滚动条方向</param>
        private void DrawThumb(Graphics g, ScrollBarState state, ScrollBarOrientation orientation)
        {
            Image thumbImg;

            if (state == ScrollBarState.Pressed | state == ScrollBarState.Hot | state == ScrollBarState.Active)//滚动条活动状态
            {
                if (thumbHoverImage == null)//如果图片为空就新建一张图
                {
                    thumbHoverImage = new Bitmap(10, 10);
                    Graphics gg = Graphics.FromImage(thumbHoverImage);
                    gg.FillPie(new SolidBrush(thumbHoverColor), new Rectangle(0, 0, 10, 10), 0, 360);
                }
                thumbImg = thumbHoverImage;
            }
            else//非活动状态
            {
                if (ThumbImage == null)//如果图片为空就新建一张图
                {
                    ThumbImage = new Bitmap(10, 10);
                    Graphics gg = Graphics.FromImage(ThumbImage);
                    gg.FillPie(new SolidBrush(thumbColor), new Rectangle(0, 0, 10, 10), 0, 360);
                }
                thumbImg = ThumbImage;
            }
            int x = this.thumbRectangle.X;
            int y = this.thumbRectangle.Y;
            using (ImageAttributes ImgAtt = new ImageAttributes())
            {
                ImgAtt.SetWrapMode(System.Drawing.Drawing2D.WrapMode.Tile);//图像平铺
                if (orientation == ScrollBarOrientation.Vertical)//垂直方向
                {
                    if (thumbImg == null) return;
                    if (thumbImg.Width > thumbImg.Height) thumbImg.RotateFlip(RotateFlipType.Rotate270FlipNone);//如果宽大于长，逆时针旋转90度
                    //画中间部分
                    Rectangle rf = new Rectangle(x, y + 3, thumbRectangle.Width, this.thumbRectangle.Height - 6);//图片绘制的坐标和大小
                    g.DrawImage(thumbImg, rf, 0, 3, thumbImg.Width, thumbImg.Height - 6, GraphicsUnit.Pixel, ImgAtt);
                    //画上部分
                    g.DrawImage(thumbImg, new Rectangle(x, y, thumbRectangle.Width, 3), 0, 0, thumbImg.Width, 3, GraphicsUnit.Pixel, ImgAtt);
                    //画下部分
                    g.DrawImage(thumbImg, new Rectangle(x, thumbRectangle.Bottom - 3, thumbRectangle.Width, 3), 0, thumbImg.Height - 3, thumbImg.Width, 3, GraphicsUnit.Pixel, ImgAtt);
                }
                else
                {
                    if (thumbImg == null) return;
                    if (thumbImg.Width < thumbImg.Height) thumbImg.RotateFlip(RotateFlipType.Rotate270FlipNone);//如果宽小于长，逆时针旋转90度
                    //画中间部分
                    g.DrawImage(thumbImg, new Rectangle(x + 3, y, this.thumbRectangle.Width - 6, this.thumbRectangle.Height), 3, 0, thumbImg.Width - 6, thumbImg.Height, GraphicsUnit.Pixel, ImgAtt);
                    ///画上部分
                    g.DrawImage(thumbImg, new Rectangle(x, y, 3, thumbRectangle.Height), 0, 0, 3, thumbImg.Height, GraphicsUnit.Pixel, ImgAtt);
                    ///画下部分
                    g.DrawImage(thumbImg, new Rectangle(thumbRectangle.Right - 3, y, 3, thumbRectangle.Height), thumbImg.Width - 3, 0, 3, thumbImg.Height, GraphicsUnit.Pixel, ImgAtt);

                }
            }
        }
        /// <summary>
        /// 设置滑动条
        /// </summary>
        private void SetUpScrollBar()
        {
            // 滚动条正在执行更改返回
            if (this.inUpdate)
            {
                return;
            }


            if (this.orientation == ScrollBarOrientation.Vertical)//垂直滚动条
            {
                if (ScrollStyle == ScrollExStyle.noSlideway || ScrollStyle == ScrollExStyle.thinSlideway)//无轨道或窄轨道
                {
                    thumbWidth = 11;//滑块宽度
                    z_ArrowHeight = z_ArrowWidth = 0;//没有上下按钮
                }
                else if (ScrollStyle == ScrollExStyle.thickSlideway)//宽轨道
                {
                    thumbWidth = 10;
                    z_ArrowHeight = z_ArrowWidth = 0;
                }
                else
                {
                    thumbWidth = thumbWidth < thumbHeight ? thumbWidth : thumbHeight;
                    //if (z_ArrowHeight >= z_ArrowWidth)
                    //{
                    //    int temp = z_ArrowHeight;
                    //    z_ArrowHeight = z_ArrowWidth;
                    //    z_ArrowWidth = temp;
                    //}
                }
                thumbHeight = this.GetThumbSizer();

                clickedBarRectangle = this.ClientRectangle;
                clickedBarRectangle.Inflate(-1, -1);
                clickedBarRectangle.Y += this.z_ArrowHeight;
                clickedBarRectangle.Height -= this.z_ArrowHeight * 2;
                //得到轨道矩形
                channelRectangle = this.clickedBarRectangle;
                //得到滑块矩形
                thumbRectangle = new Rectangle(ClientRectangle.X, ClientRectangle.Y + z_ArrowHeight + 1, this.thumbWidth, this.thumbHeight);
                //得到顶部箭头矩形
                topArrowRectangle = new Rectangle(ClientRectangle.X + (ClientRectangle.Width - z_ArrowWidth) / 2, ClientRectangle.Y + 1, this.z_ArrowWidth, this.z_ArrowHeight);
                //得到底部箭头矩形
                bottomArrowRectangle = new Rectangle(ClientRectangle.X + (ClientRectangle.Width - z_ArrowWidth) / 2, ClientRectangle.Bottom - this.z_ArrowHeight - 1, this.z_ArrowWidth, this.z_ArrowHeight);

                //默认滑块滑块被点击的位置
                thumbPosition = thumbRectangle.Height / 2;

                // 滑块下边框的下限
                thumbBottomLimitBottom = ClientRectangle.Bottom - this.z_ArrowHeight - 2;

                // 滑块上边框的下限
                thumbBottomLimitTop = thumbBottomLimitBottom - thumbRectangle.Height;

                // 滑块顶部的上限
                thumbTopLimit = ClientRectangle.Y + this.z_ArrowHeight + 2;
            }
            else
            {
                if (ScrollStyle == ScrollExStyle.noSlideway || ScrollStyle == ScrollExStyle.thinSlideway)
                {
                    this.thumbHeight = 11;
                    z_ArrowHeight = z_ArrowWidth = 0;
                }
                else if (ScrollStyle == ScrollExStyle.thickSlideway)
                {
                    this.thumbHeight = 10;
                    z_ArrowHeight = z_ArrowWidth = 0;
                }
                else
                {
                    thumbHeight = thumbHeight < thumbWidth ? thumbHeight : thumbWidth;
                    if (z_ArrowHeight <= z_ArrowWidth)
                    {
                        int temp = z_ArrowHeight;
                        z_ArrowHeight = z_ArrowWidth;
                        z_ArrowWidth = temp;
                    }
                }
                thumbWidth = this.GetThumbSizer();

                clickedBarRectangle = ClientRectangle;
                clickedBarRectangle.Inflate(-1, -1);
                clickedBarRectangle.X += z_ArrowWidth;
                clickedBarRectangle.Width -= z_ArrowWidth * 2;

                channelRectangle = clickedBarRectangle;

                thumbRectangle = new Rectangle(
                   ClientRectangle.X + z_ArrowWidth + 1,
                   ClientRectangle.Y,
                   thumbWidth,
                   thumbHeight
                );


                topArrowRectangle = new Rectangle(
                   ClientRectangle.X + 1,
                   ClientRectangle.Y + (ClientRectangle.Height - z_ArrowWidth) / 2,
                   z_ArrowWidth,
                   z_ArrowHeight
                );

                bottomArrowRectangle = new Rectangle(
                   ClientRectangle.Right - z_ArrowWidth - 1,
                   ClientRectangle.Y + (ClientRectangle.Height - z_ArrowWidth) / 2,
                   z_ArrowWidth,
                   z_ArrowHeight
                );

                // Set the default starting thumb position.
                this.thumbPosition = this.thumbRectangle.Width / 2;

                // Set the bottom limit of the thumb's bottom border.
                this.thumbBottomLimitBottom =
                   ClientRectangle.Right - this.z_ArrowWidth - 2;

                // Set the bottom limit of the thumb's top border.
                this.thumbBottomLimitTop =
                   this.thumbBottomLimitBottom - this.thumbRectangle.Width;

                // Set the top limit of the thumb's top border.
                this.thumbTopLimit = ClientRectangle.X + this.z_ArrowWidth + 2;
            }

            this.ChangeThumbPosition(this.GetThumbPosition());
            SetBackImage();
            _arrowbtns = InitArrawBtns();
            this.Refresh();//重绘
        }
        /// <summary>
        /// 计算滑块大小
        /// </summary>
        /// <returns></returns>
        private int GetThumbSizer()
        {
            ///得到轨道整体大小
            int trackSize = orientation == ScrollBarOrientation.Vertical ? this.Height - (2 * this.z_ArrowHeight) - 4 : this.Width - (2 * this.arrowWidth) - 4;
            trackSize = Math.Max(0, trackSize);//轨道整体大小至少是0

            int thumbS = Convert.ToInt32((double)trackSize * (double)ShowContentSize / (double)ContentSize);
            return Math.Min(trackSize, thumbS);
        }

        #endregion

        #region 公开方法

        [DllImport("User32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, bool wParam, int lParam);

        /// <summary>
        /// 在调用 EndUpdate 之前禁止绘制控件
        /// </summary>
        public void BeginUpdate()
        {
            SendMessage(this.Handle, (int)(0x000B)/*WindowsMessage.WM_SETREDRAW*/, false, 0);
            this.inUpdate = true;
        }

        /// <summary>
        /// 结束更新过程，控件可以再次绘制自身
        /// </summary>
        public void EndUpdate()
        {
            SendMessage(this.Handle, (int)(0x000B)/*WindowsMessage.WM_SETREDRAW*/, true, 0);
            this.inUpdate = false;
            this.SetUpScrollBar();
            this.Refresh();
        }

        #endregion
    }

    /// <summary>
    /// ScrollBarC 控件设计器
    /// </summary>
    internal class ScrollBarControlDesigner : ControlDesigner
    {
        /// <summary>
        /// 控件的选择规则
        /// </summary>
        public override SelectionRules SelectionRules
        {
            get
            {
                // 获取属性“Orientation”的属性描述符
                PropertyDescriptor propDescriptor = TypeDescriptor.GetProperties(this.Component)["Orientation"];

                // 不是空可以获取滑动条的当前方向
                if (propDescriptor != null)
                {
                    // 获取当前方向
                    ScrollBarC.ScrollBarOrientation orientation = (ScrollBarC.ScrollBarOrientation)propDescriptor.GetValue(this.Component);

                    // 如果是垂直方向
                    if (orientation == ScrollBarC.ScrollBarOrientation.Vertical)
                    {
                        return SelectionRules.Visible | SelectionRules.Moveable | SelectionRules.BottomSizeable | SelectionRules.TopSizeable;
                    }

                    return SelectionRules.Visible | SelectionRules.Moveable | SelectionRules.LeftSizeable | SelectionRules.RightSizeable;
                }

                return base.SelectionRules;
            }
        }

        /// <summary>
        /// 隐藏部分不需要的属性
        /// </summary>
        /// <param name="properties">属性字典</param>
        protected override void PreFilterProperties(System.Collections.IDictionary properties)
        {
            properties.Remove("Text");
            properties.Remove("BackgroundImage");
            properties.Remove("ForeColor");
            properties.Remove("ImeMode");
            properties.Remove("Padding");
            properties.Remove("BackgroundImageLayout");
            properties.Remove("Font");
            properties.Remove("RightToLeft");

            base.PreFilterProperties(properties);
        }
    }
}
