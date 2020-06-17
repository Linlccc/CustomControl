using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImagesListC;

namespace CustomControlTest
{
    public partial class Test : Form
    {
        public ImageListC imageEmoji;//表情图片窗体
        public Test()
        {
            InitializeComponent();
            InitImageEmoji();
        }
        /// <summary>
        /// 初始化emoji
        /// </summary>
        public void InitImageEmoji()
        {
            imageEmoji = new ImageListC();
            imageEmoji.LoadImageContainer(8, 12, 25, 25, 10, 10, 40, 40);
            imageEmoji.ImageClick += ImageEmoji_ImageClick;

            imageEmoji.HLinesColor = Color.Transparent;
            imageEmoji.VLinesColor = Color.Transparent;
            imageEmoji.SelectedBorderColor = Color.Transparent;
            imageEmoji.SelectedBackColor = Color.FromArgb(189, 195, 199);
        }
        /// <summary>
        /// 选择图像后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="imgInfo">选择的图像信息</param>
        private void ImageEmoji_ImageClick(object sender, ImageInfo imgInfo)
        {
            this.Image.Image = imgInfo.ImgAndValue.Image;
            this.tText.Text = imgInfo.ImgAndValue.Value;
        }

        private void OpenImage_Click(object sender, EventArgs e)
        {
            Point pt = PointToScreen(new Point(OpenImage.Location.X, OpenImage.Location.Y));
            imageEmoji.Show(pt.X-130, pt.Y -imageEmoji.Height-10);//显示表情窗口
        }
    }
}
